using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CipherWords;
using KModkit;
using KeepCoding;
using Rnd = UnityEngine.Random;

public class DistanceCipherScript : ModuleScript
{
    private KMBombModule _module;

    [SerializeField]
    internal KMSelectable[] _GenerationZero, _GenerationOne, _GenerationTwo;
    [SerializeField]
    internal KMSelectable _SubmitButton;
    [SerializeField]
    internal TextMesh[] _DisplayTexts;
    [SerializeField]
    private Material[] _ColourPalette;
    [SerializeField]
    private GameObject _StatusLight;
    [SerializeField]
    private AudioClip[] _Sounds;

    internal char[] _alphabet = { '#', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
    private char _keyLetter;
    private byte _keyDigit;

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

    private int[][] _keyTriangle = new int[7][]
    {
        new int[7],
        new int[6],
        new int[5],
        new int[4],
        new int[3],
        new int[2] { 0, 0 },
        new int[1]
    };

    private int[][] _clockwiseKeyTriangle = new int[7][]
    {
        new int[7],
        new int[6],
        new int[5],
        new int[4],
        new int[3],
        new int[2] { 0, 0 },
        new int[1]
    };

    private int[][]_counterKeyTriangle = new int[7][]
    {
        new int[7],
        new int[6],
        new int[5],
        new int[4],
        new int[3],
        new int[2] { 0, 0 },
        new int[1]
    };

    private float _step;
    private float _increment = 1f;
    private MeshRenderer[] _TileMeshes = new MeshRenderer[40];
    private int _seed;
    private System.Random _Rnd;
    internal string _decryptedWord, _keyword, _encryptedWord;
    private char _unusedLetter;
    internal bool _isSolved, _isSeedSet, _isInInputMode, _isVisitingFirst;
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
        _keyLetter = _alphabet[_Rnd.Next(1, _alphabet.Length)];
        _keyDigit = (byte)_Rnd.Next(0, 10);
        _DisplayTexts[2].text = _keyLetter + _keyDigit.ToString();
        _pages[0][1] = _keyword;
        while(_keyword.Length != 7 || _keyword == _decryptedWord)
            _keyword = Wordlist.wordlist[_Rnd.Next(0, Wordlist.wordlist.Length)];

        Log("The decrypted word is: " + _decryptedWord);
        Log("The keyword used is: " + _keyword);
        Log("The key is: " + _DisplayTexts[2].text);

        for (int i = 0; i < _GenerationOne.Length; i++)
            _GenerationOne[i].gameObject.SetActive(false);
        for (int i = 0; i < _GenerationTwo.Length; i++)
            _GenerationTwo[i].gameObject.SetActive(false);

        for (int i = 0; i < _GenerationZero.Length; i++)
        {
            var x = i;
            _TileMeshes[i] = _GenerationZero[i].GetComponentInChildren<MeshRenderer>();
            _GenerationZero[i].Assign(onHighlight: () => { _isTileHighlighted[x] = true; });
            _GenerationZero[i].Assign(onHighlightEnded: () => { _isTileHighlighted[x] = false; });
            _GenerationZero[i].Assign(onInteract: () => { PressTile(x); });
        }

        for (int i = 0; i < _GenerationOne.Length; i++)
        {
            var x = i;
            _TileMeshes[i + 3] = _GenerationOne[i].GetComponentInChildren<MeshRenderer>();
            _GenerationOne[i].Assign(onHighlight: () => { _isTileHighlighted[x + 3] = true; });
            _GenerationOne[i].Assign(onHighlightEnded: () => { _isTileHighlighted[x + 3] = false; });
            _GenerationOne[i].Assign(onInteract: () => { PressTile(x + 3); });
        }

        for (int i = 0; i < _GenerationTwo.Length; i++)
        {
            var x = i;
            _TileMeshes[i + 12] = _GenerationTwo[i].GetComponentInChildren<MeshRenderer>();
            _GenerationTwo[i].Assign(onHighlight: () => { _isTileHighlighted[x + 12] = true; });
            _GenerationTwo[i].Assign(onHighlightEnded: () => { _isTileHighlighted[x + 12] = false; });
            _GenerationTwo[i].Assign(onInteract: () => { PressTile(x + 12); });
        }

        _TileMeshes[39] = _SubmitButton.GetComponentInChildren<MeshRenderer>();
        _SubmitButton.Assign(onHighlight: () => { _isTileHighlighted[39] = true; });
        _SubmitButton.Assign(onHighlightEnded: () => { _isTileHighlighted[39] = false; });
        _SubmitButton.Assign(onInteract: () => { SubmitSequence(); });

        _isVisitingFirst = true;
        MakeKey();

        for (int i = 0; i < _clockwiseKeyTriangle.Length; i++)
            for (int j = 0; j < _clockwiseKeyTriangle[i].Length; j++)
                _clockwiseKeyTriangle[i][j] = _keyTriangle[_clockwiseKeyTriangle[i].Length - 1 - j][i];

        Log("Key triangle, turned clockwise");
        for (int i = 0; i < _clockwiseKeyTriangle.Length; i++)
        {
            string logMessage = "";
            for (int j = 0; j < _clockwiseKeyTriangle[i].Length; j++)
            {
                logMessage += _alphabet[_clockwiseKeyTriangle[i][j]];
                if (j != _clockwiseKeyTriangle[i].Length - 1)
                    logMessage += " | ";
            }
            Log(logMessage);
        }

        for (int i = 0; i < _counterKeyTriangle.Length; i++)
            for (int j = 0; j < _counterKeyTriangle[i].Length; j++)
                _counterKeyTriangle[i][j] = _clockwiseKeyTriangle[_clockwiseKeyTriangle[i].Length - 1 - j][i];

        Log("Key triangle, turned counter-clockwise");
        for (int i = 0; i < _counterKeyTriangle.Length; i++)
        {
            string logMessage = "";
            for (int j = 0; j < _counterKeyTriangle[i].Length; j++)
            {
                logMessage += _alphabet[_counterKeyTriangle[i][j]];
                if (j != _counterKeyTriangle[i].Length - 1)
                    logMessage += " | ";
            }
            Log(logMessage);
        }

        _encryptedWord = RotationalMonoalphabeticSubstitution(_decryptedWord);
        Log("After encrypting using step 2, encrypted word is: " + _encryptedWord);

        _encryptedWord = CaesareanRoleSwitchingCipher(_encryptedWord);
        Log("After encrypting using step 1, encrypted word is: " + _encryptedWord);

        _pages[0][0] = _encryptedWord;
        _pages[0][1] = _keyword;
        _DisplayTexts[0].text = _pages[0][0];
        _DisplayTexts[1].text = _pages[0][1];
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
        bool foundMissingLetter = false;
        int iterator = 1;
        while (!foundMissingLetter)
        {
            if (!_usedPositions.Contains(iterator))
            {
                foundMissingLetter = true;
                _unusedLetter = _alphabet[iterator];
                _keyTriangle[6][0] = iterator;
            }
            else
                iterator++;
        }
        for (int i = 0; i < _keyTriangle.Length; i++)
        {
            string logMessage = "";
            for(int j = 0; j < _keyTriangle[i].Length; j++)
            {
                logMessage += _alphabet[_keyTriangle[i][j]];
                if (j != _keyTriangle[i].Length - 1)
                    logMessage += " | ";
            }
            Log(logMessage);
        }
        Log("Therefore, the unused letter is: " + _unusedLetter.ToString());
    }

    private string RotationalMonoalphabeticSubstitution(string word)
    {
        string encryptedWord = "";
        if(_alphabet.IndexOf(_keyLetter) % 2 == _keyDigit % 2)
        {
            for(int i = 0; i < word.Length; i++)
            {
                int a = 0, b = 0;
                for(int j = 0; j < _counterKeyTriangle.Length; j++)
                {
                    for(int k = 0; k < _counterKeyTriangle[j].Length; k++)
                    {
                        if(_alphabet.IndexOf(word[i]) == _keyTriangle[j][k])
                        {
                            a = j;
                            b = k;
                        }
                    }
                }
                if(_counterKeyTriangle[a][b] == 0)
                    encryptedWord += _alphabet[_clockwiseKeyTriangle[a][b]].ToString();
                else
                    encryptedWord += _alphabet[_counterKeyTriangle[a][b]].ToString();
            }
        }
        else
        {
            for (int i = 0; i < word.Length; i++)
            {
                int a = 0, b = 0;
                for (int j = 0; j < _counterKeyTriangle.Length; j++)
                {
                    for (int k = 0; k < _counterKeyTriangle[j].Length; k++)
                    {
                        if (_alphabet.IndexOf(word[i]) == _keyTriangle[j][k])
                        {
                            a = j;
                            b = k;
                        }
                    }
                }
                if (_clockwiseKeyTriangle[a][b] == 0)
                    encryptedWord += _alphabet[_counterKeyTriangle[a][b]].ToString();
                else
                    encryptedWord += _alphabet[_clockwiseKeyTriangle[a][b]].ToString();
            }
        }
        return encryptedWord;
    }

    private string CaesareanRoleSwitchingCipher(string word)
    {
        string initialWord = _keyLetter + word;
        string finalWord = "";
        for(int i = 1; i < initialWord.Length; i++)
        {
            int shift = 0;
            string alphabet = "ZABCDEFGHIJKLMNOPQRSTUVWXY";
            while ((alphabet.IndexOf(initialWord[i]) + shift) % 26 != alphabet.IndexOf(initialWord[i - 1]))
                shift++;
            finalWord += alphabet[(alphabet.IndexOf(initialWord[i - 1]) + shift) % 26];
        }
        return finalWord;
    }

    private void PressTile(int x)
    {
        if (_isSolved)
            return;
        if (x / 3 == 0)
        {
            ButtonEffect(_module.GetComponent<KMSelectable>(), 1.5f, _Sounds[0]);
            for (int i = 0; i < _GenerationZero.Length; i++)
                _GenerationZero[i].gameObject.SetActive(true);

            _GenerationZero[x].gameObject.SetActive(false);

            for (int i = 0; i < _GenerationOne.Length; i++)
                _GenerationOne[i].gameObject.SetActive(false);

            for (int i = 0; i < _GenerationTwo.Length; i++)
                _GenerationTwo[i].gameObject.SetActive(false);

            for (int i = 3 * x; i < (3 * x) + 3; i++)
                _GenerationOne[i].gameObject.SetActive(true);
        }
        else if (x / 3 >= 1 && x / 3 <= 3)
        {
            ButtonEffect(_module.GetComponent<KMSelectable>(), 1f, _Sounds[0]);
            foreach (int idx in _generations[1][(x / 3) - 1])
                _GenerationOne[idx - 3].gameObject.SetActive(true);
            for (int i = 0; i < _GenerationTwo.Length; i++)
                _GenerationTwo[i].gameObject.SetActive(false);

            _GenerationOne[x - 3].gameObject.SetActive(false);

            foreach(int idx in _generations[2][x - 3])
                _GenerationTwo[idx - 12].gameObject.SetActive(true);
        }
        else if (x / 3 >= 4 && x / 3 <= 12)
        {
            ButtonEffect(_module.GetComponent<KMSelectable>(), 0.5f, _Sounds[0]);
            if (!_isInInputMode)
            {
                _isInInputMode = true;
                for (int i = 0; i < _DisplayTexts.Length; i++)
                    _DisplayTexts[i].text = "";
                if(x - 12 == 0)
                {
                    _isInInputMode = false;
                    _DisplayTexts[0].text = _encryptedWord;
                    _DisplayTexts[1].text = _keyword;
                    _DisplayTexts[2].text = _keyLetter + _keyDigit.ToString();
                }
                else
                    _DisplayTexts[0].text += _alphabet[x - 12];
            }
            else
            {
                if(x - 12 == 0)
                {
                    _DisplayTexts[0].text = _DisplayTexts[0].text.Remove(_DisplayTexts[0].text.Length - 1, 1);
                    if(_DisplayTexts[0].text.Length == 0)
                    {
                        _isInInputMode = false;
                        _DisplayTexts[0].text = _encryptedWord;
                        _DisplayTexts[1].text = _keyword;
                        _DisplayTexts[2].text = _keyLetter + _keyDigit.ToString();
                    }
                }
                else if(_DisplayTexts[0].text.Length < 10)
                    _DisplayTexts[0].text += _alphabet[x - 12];
            }
            for (int i = 0; i < _GenerationZero.Length; i++)
                _GenerationZero[i].gameObject.SetActive(true);

            for (int i = 0; i < _GenerationOne.Length; i++)
                _GenerationOne[i].gameObject.SetActive(false);

            for (int i = 0; i < _GenerationTwo.Length; i++)
                _GenerationTwo[i].gameObject.SetActive(false);
        }
    }

    private void SubmitSequence()
    {
        if (_isSolved)
            return;
        if(_isInInputMode)
        {
            if(_DisplayTexts[0].text == "IWANNATEST")
            {
                ButtonEffect(_SubmitButton, 1, _Sounds[3]);
                _DisplayTexts[0].text = _encryptedWord;
                _DisplayTexts[1].text = _keyword;
                _DisplayTexts[2].text = _keyLetter + _keyDigit.ToString();
            }
            else if(_DisplayTexts[0].text == _decryptedWord)
            {
                _isSolved = true;
                _DisplayTexts[0].text = "";
                Log("Submitted correct word. Module solved!");
                _module.HandlePass();
                ButtonEffect(_SubmitButton, 1, _Sounds[1]);
                StartCoroutine(PostSolve());
            }
            else
            {
                _isInInputMode = false;
                Log("Submitted incorrect word: " + _DisplayTexts[0].text + ". Striking and reverting to page 1.");
                _module.HandleStrike();
                _DisplayTexts[0].text = _encryptedWord;
                _DisplayTexts[1].text = _keyword;
                _DisplayTexts[2].text = _keyLetter + _keyDigit.ToString();
            }
        }
    }

    private IEnumerator PostSolve()
    {
        for (int i = 0; i < _GenerationZero.Length; i++)
            _GenerationZero[i].gameObject.SetActive(true);

        for (int i = 0; i < _GenerationOne.Length; i++)
            _GenerationOne[i].gameObject.SetActive(false);

        for (int i = 0; i < _GenerationTwo.Length; i++)
            _GenerationTwo[i].gameObject.SetActive(false);

        for(int i = 0; i < 128; i++)
        {
            yield return new WaitForSeconds(0.025f);
            _StatusLight.GetComponent<MeshRenderer>().material.color += new Color32(0, 1, 0, 255);
        }
    }

    void FixedUpdate()
    {
        if (!_isSolved)
        {
            for (int i = 0; i < _isTileHighlighted.Length; i++)
            {
                if (_isTileHighlighted[i])
                    _TileMeshes[i].material = _ColourPalette[4];
                else
                {
                    if (i == 0 || (3 <= i && 5 >= i) || (12 <= i && i <= 20))
                        _TileMeshes[i].material = _ColourPalette[0];
                    else if (i == 1 || (6 <= i && 8 >= i) || (21 <= i && i <= 29))
                        _TileMeshes[i].material = _ColourPalette[1];
                    else if (i == 2 || (9 <= i && 11 >= i) || (30 <= i && i <= 38))
                        _TileMeshes[i].material = _ColourPalette[2];
                    else if (i == 39)
                        _TileMeshes[i].material = _ColourPalette[3];
                }
            }
        }
        _step += _increment;
        _StatusLight.transform.localRotation = Quaternion.Euler((_step * Mathf.PI) / 2f, (_step * Mathf.PI) / 4f, (_step * Mathf.PI) / 6f);
    }
}
