using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CipherWords;
using KModkit;
using System.Text.RegularExpressions;
using KeepCoding;
using Rnd = UnityEngine.Random;

public class DistanceCipherScript : ModuleScript
{
    private KMBombModule _module;

    [SerializeField]
    private KMSelectable[] GenerationZero, GenerationOne, GenerationTwo;
    [SerializeField]
    private KMSelectable SubmitButton;
    [SerializeField]
    private TextMesh[] DisplayTexts;
    [SerializeField]
    private Material[] ColourPalette;
    [SerializeField]
    private GameObject StatusLight;

	private char[] _alphabet = { '0', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

	private int[][][] _generations = new int[][][]
    {
        new int[][]
        {
            new int[] { 0 },
            new int[] { 1 },
            new int[] { 2 }
        },
        new int[][]
        {
			new int[] { 3, 4, 5 },
			new int[] { 6, 7, 8 },
			new int[] { 9, 10, 11 }
        },
		new int[][]
        {
			new int[] { 12, 13, 14 },
			new int[] { 15, 16, 17 },
			new int[] { 18, 19, 20 },
			new int[] { 21, 22, 23 },
			new int[] { 24, 25, 26 },
			new int[] { 27, 28, 29 },
			new int[] { 30, 31, 32 },
			new int[] { 33, 34, 35 },
			new int[] { 36, 37, 38 }
        }
    };

	private List<int> _usedPositions = new List<int>();
	private List<int> _usedSummations = new List<int>();

	private int[][] _keyTriangle = new int[7][]
	{
		new int[7],
		new int[6],
		new int[5],
		new int[4],
		new int[3],
		new int[2] { 1, 1 },
		new int[1] { 0 }
	};

	private int[][] _keySummation = new int[7][]
	{
		new int[7],
		new int[6],
		new int[5],
		new int[4],
		new int[3],
		new int[2] { 1, 1 },
		new int[1] { 0 }
	};
	
	private float _step;
	private float _increment = 1f;
	private MeshRenderer[] _TileMeshes = new MeshRenderer[40];
	private int _seed;
	private System.Random _Rnd;
	private string _decryptedWord, _keyword, _encryptedWord;
	private char _unusedLetter;
	private bool _isSolved, _isSeedSet, _isInInputMode, _isVisitingFirst;
	private bool[] _isTileHighlighted = new bool[40];
	private string[][] _pages = new string[3][]
    {
		new string[2],
		new string[2],
		new string[2]
    };

	void Start()
	{
		if (_isVisitingFirst)
			Log("Module encountered ambiguous case, regenerating.");
		_module = Get<KMBombModule>();
		_keySummation = new int[7][]
		{
			new int[7],
			new int[6],
			new int[5],
			new int[4],
			new int[3],
			new int[2] { 1, 1 },
			new int[1] { 0 }
		};
		_keyTriangle = new int[7][]
		{
			new int[7],
			new int[6],
			new int[5],
			new int[4],
			new int[3],
			new int[2] { 1, 1 },
			new int[1] { 0 }
		};
		if (!_isSeedSet)
        {
			_isSeedSet = true;
			_seed = Rnd.Range(int.MinValue, int.MaxValue);
			_Rnd = new System.Random(_seed);
			Log("The seed is: " + _seed.ToString());
        }

		// SET SEED ABOVE IN CASE OF BUGS!!
		// _Rnd = new System.Random(loggedSeed);

		_decryptedWord = Wordlist.wordlist[_Rnd.Next(0, Wordlist.wordlist.Length)];
		_keyword = Wordlist.wordlist[_Rnd.Next(0, Wordlist.wordlist.Length)];
		_pages[1][1] = _keyword;
		while(_keyword.Length != 7 || _keyword == _decryptedWord)
			_keyword = Wordlist.wordlist[_Rnd.Next(0, Wordlist.wordlist.Length)];

		Log("The decrypted word is: " + _decryptedWord);
		Log("The keyword used is: " + _keyword);

		for (int i = 0; i < GenerationOne.Length; i++)
			GenerationOne[i].gameObject.SetActive(false);
		for (int i = 0; i < GenerationTwo.Length; i++)
			GenerationTwo[i].gameObject.SetActive(false);

		for (int i = 0; i < GenerationZero.Length; i++)
        {
			var x = i;
			_TileMeshes[i] = GenerationZero[i].GetComponentInChildren<MeshRenderer>();
			GenerationZero[i].Assign(onHighlight: () => { _isTileHighlighted[x] = true; });
			GenerationZero[i].Assign(onHighlightEnded: () => { _isTileHighlighted[x] = false; });
			GenerationZero[i].Assign(onInteract: () => { PressTile(x); });
        }

		for (int i = 0; i < GenerationOne.Length; i++)
		{
			var x = i;
			_TileMeshes[i + 3] = GenerationOne[i].GetComponentInChildren<MeshRenderer>();
			GenerationOne[i].Assign(onHighlight: () => { _isTileHighlighted[x + 3] = true; });
			GenerationOne[i].Assign(onHighlightEnded: () => { _isTileHighlighted[x + 3] = false; });
			GenerationOne[i].Assign(onInteract: () => { PressTile(x + 3); });
		}

		for (int i = 0; i < GenerationTwo.Length; i++)
		{
			var x = i;
			_TileMeshes[i + 12] = GenerationTwo[i].GetComponentInChildren<MeshRenderer>();
			GenerationTwo[i].Assign(onHighlight: () => { _isTileHighlighted[x + 12] = true; });
			GenerationTwo[i].Assign(onHighlightEnded: () => { _isTileHighlighted[x + 12] = false; });
			GenerationTwo[i].Assign(onInteract: () => { PressTile(x + 12); });
		}

		_TileMeshes[39] = SubmitButton.GetComponentInChildren<MeshRenderer>();
		SubmitButton.Assign(onHighlight: () => { _isTileHighlighted[39] = true; });
		SubmitButton.Assign(onHighlightEnded: () => { _isTileHighlighted[39] = false; });
		SubmitButton.Assign(onInteract: () => { SubmitSequence(); });

		_isVisitingFirst = true;
		MakeKey();
	}

	private void MakeKey()
    {
		char[] keywordLetters = _keyword.ToCharArray();
		int[] keywordPositions = new int[keywordLetters.Length];
		for (int i = 0; i < keywordLetters.Length; i++)
        {
			keywordPositions[i] = Array.IndexOf(_alphabet, keywordLetters[i]);
			while (_usedPositions.Contains(keywordPositions[i]))
            {
				int tempMinus = (keywordPositions[i] - 1) % 26;
				int tempPlus = (keywordPositions[i] + 1) % 26;
				trial:
				if (tempMinus == 0)
					tempMinus = 26;
				if (tempPlus == 0)
					tempPlus = 26;
				if(_usedPositions.Contains(tempMinus) && _usedPositions.Contains(tempPlus))
                {
					tempMinus = (tempMinus - 1) % 26;
					tempPlus = (tempPlus + 1) % 26;
					if (tempMinus == 0)
						tempMinus = 26;
					if (tempPlus == 0)
						tempPlus = 26;
					goto trial;
				}
				else if(!_usedPositions.Contains(tempMinus))
					keywordPositions[i] = tempMinus;
				else if(!_usedPositions.Contains(tempPlus))
					keywordPositions[i] = tempPlus;
            }
			_usedPositions.Add(keywordPositions[i]);
			_keyTriangle[0][i] = keywordPositions[i];
		}
		for(int i = 1; i <= 4; i++)
        {
			for(int j = 0; j < _keyTriangle[i].Length; j++)
            {
				int diff = Math.Abs(_keyTriangle[i - 1][j] - _keyTriangle[i - 1][j + 1]);
				while(_usedPositions.Contains(diff))
                {
					int tempMinus = (diff - 1) % 26;
					int tempPlus = (diff + 1) % 26;
					trialSecond:
					if (tempMinus == 0)
						tempMinus = 26;
					if(tempPlus == 0)
						tempPlus = 26;
					if (_usedPositions.Contains(tempMinus) && _usedPositions.Contains(tempPlus))
					{
						tempMinus = (tempMinus - 1) % 26;
						tempPlus = (tempPlus + 1) % 26;
						if (tempMinus == 0)
							tempMinus = 26;
						if (tempPlus == 0)
							tempPlus = 26;
						goto trialSecond;
					}
					else if (!_usedPositions.Contains(tempMinus))
						diff = tempMinus;
					else if (!_usedPositions.Contains(tempPlus))
						diff = tempPlus;
                }
				_usedPositions.Add(diff);
				_keyTriangle[i][j] = diff;
            }
        }
		for(int i = 0; i < _keyTriangle.Length; i++)
        {
			string logMessage = "";
			for(int j = 0; j < _keyTriangle[i].Length; j++)
            {
				if (i != 5)
					logMessage += _alphabet[_keyTriangle[i][j]];
				else
					logMessage = "1 | 1";
				if (j != _keyTriangle[i].Length - 1 && i != 5)
					logMessage += " | ";
            }
			Log(logMessage);
        }
		bool foundMissingLetter = false;
		int iterator = 1;
		while(!foundMissingLetter)
        {
			if (!_usedPositions.Contains(iterator))
			{
				foundMissingLetter = true;
				_unusedLetter = _alphabet[iterator];
			}
			else
				iterator++;
        }
		_pages[0][0] = "";
		_pages[0][1] = _keyword;
		DisplayTexts[0].text = _pages[0][0];
		DisplayTexts[1].text = _pages[0][1];
		Log("Therefore, the unused letter is: " + _unusedLetter.ToString());
	}

	private void PressTile(int x)
    {
		if (_isSolved)
			return;
		if (x / 3 == 0)
        {
			for (int i = 0; i < GenerationZero.Length; i++)
				GenerationZero[i].gameObject.SetActive(true);

			GenerationZero[x].gameObject.SetActive(false);

			for (int i = 0; i < GenerationOne.Length; i++)
				GenerationOne[i].gameObject.SetActive(false);

			for (int i = 0; i < GenerationTwo.Length; i++)
				GenerationTwo[i].gameObject.SetActive(false);

			for (int i = 3 * x; i < (3 * x) + 3; i++)
				GenerationOne[i].gameObject.SetActive(true);
        }
		else if (x / 3 >= 1 && x / 3 <= 3)
        {
			foreach(int idx in _generations[1][(x / 3) - 1])
				GenerationOne[idx - 3].gameObject.SetActive(true);
			for (int i = 0; i < GenerationTwo.Length; i++)
				GenerationTwo[i].gameObject.SetActive(false);

			GenerationOne[x - 3].gameObject.SetActive(false);

			foreach(int idx in _generations[2][x - 3])
				GenerationTwo[idx - 12].gameObject.SetActive(true);
		}
		else if (x / 3 >= 4 && x / 3 <= 12)
        {
			if(!_isInInputMode)
            {
				_isInInputMode = true;
				for (int i = 0; i < DisplayTexts.Length; i++)
					DisplayTexts[i].text = "";
				if(x - 12 == 0)
                {
					_isInInputMode = false;
					DisplayTexts[0].text = _encryptedWord;
					DisplayTexts[1].text = _keyword;
                }
				else
					DisplayTexts[0].text += _alphabet[x - 12];
            }
			else
            {
				if(x - 12 == 0)
                {
					DisplayTexts[0].text = DisplayTexts[0].text.Remove(DisplayTexts[0].text.Length - 1, 1);
					if(DisplayTexts[0].text.Length == 0)
                    {
						_isInInputMode = false;
						DisplayTexts[0].text = _encryptedWord;
						DisplayTexts[1].text = _keyword;
                    }
                }
                else if(DisplayTexts[0].text.Length < 10)
					DisplayTexts[0].text += _alphabet[x - 12];
            }
        }
    }

	private void SubmitSequence()
    {
		if (_isSolved)
			return;
		if(_isInInputMode)
        {
			if(DisplayTexts[0].text == _decryptedWord)
            {
				_isSolved = true;
				Log("Submitted correct word. Module solved!");
				_module.HandlePass();
				StartCoroutine(PostSolve());
			}
			else
            {
				_isInInputMode = false;
				Log("Submitted incorrect string: " + DisplayTexts[0].text + ". Striking and reverting to page 1.");
				DisplayTexts[0].text = _encryptedWord;
				DisplayTexts[1].text = _keyword;
            }
        }
    }

	private IEnumerator PostSolve()
    {
		for (int i = 0; i < GenerationZero.Length; i++)
			GenerationZero[i].gameObject.SetActive(true);

		for (int i = 0; i < GenerationOne.Length; i++)
			GenerationOne[i].gameObject.SetActive(false);

		for (int i = 0; i < GenerationTwo.Length; i++)
			GenerationTwo[i].gameObject.SetActive(false);

		for(int i = 0; i < 128; i++)
        {
			yield return new WaitForSeconds(0.025f);
			StatusLight.GetComponent<MeshRenderer>().material.color += new Color32(0, 1, 0, 255);
		}
    }

	void FixedUpdate()
	{
		if (!_isSolved)
		{
			for (int i = 0; i < _isTileHighlighted.Length; i++)
			{
				if (_isTileHighlighted[i])
					_TileMeshes[i].material = ColourPalette[4];
				else
				{
					if (i == 0 || (3 <= i && 5 >= i) || (12 <= i && i <= 20))
						_TileMeshes[i].material = ColourPalette[0];
					else if (i == 1 || (6 <= i && 8 >= i) || (21 <= i && i <= 29))
						_TileMeshes[i].material = ColourPalette[1];
					else if (i == 2 || (9 <= i && 11 >= i) || (30 <= i && i <= 38))
						_TileMeshes[i].material = ColourPalette[2];
					else if (i == 39)
						_TileMeshes[i].material = ColourPalette[3];
				}
			}
		}
		_step += _increment;
		StatusLight.transform.localRotation = Quaternion.Euler((_step * Mathf.PI) / 2f, _step, (_step * Mathf.PI) / 4f);
	}
}
