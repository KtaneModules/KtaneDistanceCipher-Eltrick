using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KeepCoding;

public class DistanceCipherTP : TPScript<DistanceCipherScript>
{
#pragma warning disable 414
    private string TwitchHelpMessage = "'!{0} <[TLR]><[TLR]><[TLR]>' to press the three buttons needed to input a letter. '!{0} S' to press the submit button. All commands are chainable using spaces.";
#pragma warning restore 414

    public override IEnumerator ForceSolve()
    {
        Module._isInInputMode = true;
        Module._DisplayTexts[0].text = Module._decryptedWord;
        Module._DisplayTexts[1].text = "";
        Module._DisplayTexts[2].text = "";
        yield return new WaitForSeconds(.2f);
        yield return null;
        Module._SubmitButton.OnInteract();
    }

    public override IEnumerator Process(string command)
    {
        string[] split = command.ToUpperInvariant().Split(new[] { ' ', ';', ',' }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < split.Length; i++)
        {
            if (split[i].Length != 1 && split[i].Length != 3)
                yield break;
            else if (split[i].Length == 1 && split[i] != "S")
                yield break;
            else if (split[i].Length == 3)
                for (int j = 0; j < split[i].Length; j++)
                    if (!(split[i][j] == 'T' || split[i][j] == 'L' || split[i][j] == 'R'))
                        yield break;
        }

        List<KMSelectable> ButtonsToBePressed = new List<KMSelectable>();

        for (int i = 0; i < split.Length; i++)
        {
            if (split[i] == "S")
                ButtonsToBePressed.Add(Module._SubmitButton);
            else
            {
                int a = 0, b = 0, c = 0;
                switch(split[i][0])
                {
                    case 'L':
                        a = 0;
                        break;
                    case 'T':
                        a = 1;
                        break;
                    case 'R':
                        a = 2;
                        break;
                }
                ButtonsToBePressed.Add(Module._GenerationZero[a]);

                if(a == 0)
                {
                    switch (split[i][1])
                    {
                        case 'L':
                            b = 0;
                            break;
                        case 'R':
                            b = 1;
                            break;
                        case 'T':
                            b = 2;
                            break;
                    }
                }
                else if (a == 1)
                {
                    switch (split[i][1])
                    {
                        case 'L':
                            b = 0;
                            break;
                        case 'T':
                            b = 1;
                            break;
                        case 'R':
                            b = 2;
                            break;
                    }
                }
                else
                {
                    switch (split[i][1])
                    {
                        case 'T':
                            b = 0;
                            break;
                        case 'L':
                            b = 1;
                            break;
                        case 'R':
                            b = 2;
                            break;
                    }
                }
                ButtonsToBePressed.Add(Module._GenerationOne[3 * a + b]);

                if (a == b)
                {
                    switch (split[i][2])
                    {
                        case 'L':
                            c = 0;
                            break;
                        case 'T':
                            c = 1;
                            break;
                        case 'R':
                            c = 2;
                            break;
                    }
                }
                else if ((a == 0 && b == 1) || (a == 1 && b == 0))
                {
                    switch (split[i][2])
                    {
                        case 'L':
                            c = 0;
                            break;
                        case 'R':
                            c = 1;
                            break;
                        case 'T':
                            c = 2;
                            break;
                    }
                }
                else if (a == 0 && b == 2)
                {
                    switch (split[i][2])
                    {
                        case 'R':
                            c = 0;
                            break;
                        case 'L':
                            c = 1;
                            break;
                        case 'T':
                            c = 2;
                            break;
                    }
                }
                else if ((a == 1 && b == 2) || (a == 2 && b == 1))
                {
                    switch (split[i][2])
                    {
                        case 'T':
                            c = 0;
                            break;
                        case 'L':
                            c = 1;
                            break;
                        case 'R':
                            c = 2;
                            break;
                    }
                }
                else if (a == 2 && b == 0)
                {
                    switch (split[i][2])
                    {
                        case 'T':
                            c = 0;
                            break;
                        case 'R':
                            c = 1;
                            break;
                        case 'L':
                            c = 2;
                            break;
                    }
                }
                ButtonsToBePressed.Add(Module._GenerationTwo[9 * a + 3 * b + c]);
            }
        }
        foreach (KMSelectable button in ButtonsToBePressed)
        {
            yield return null;
            button.OnInteract();
            yield return new WaitForSeconds(.05f);
        }
    }
}
