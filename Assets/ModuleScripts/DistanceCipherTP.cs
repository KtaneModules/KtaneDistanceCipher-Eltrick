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
        string[] combos = { "LLL", "LLT", "LLR", "LRL", "LRR", "LRT", "LTR", "LTL", "LTT", "TLL", "TLR", "TLT", "TTL", "TTT", "TTR", "TRT", "TRL", "TRR", "RTT", "RTR", "RTL", "RLT", "RLL", "RLR", "RRL", "RRT", "RRR" };
        if (Module._isInInputMode)
        {
            string input = Module._DisplayTexts[0].text;
            if (input.Length > Module._decryptedWord.Length)
            {
                for (int k = 0; k < input.Length - Module._decryptedWord.Length; k++)
                    yield return Process(combos[0]);
                input = Module._DisplayTexts[0].text;
            }
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] != Module._decryptedWord[i])
                {
                    for (int k = 0; k < input.Length - i; k++)
                        yield return Process(combos[0]);
                    break;
                }
            }
        }
        string ans = Module._decryptedWord;
        int start = Module._DisplayTexts[0].text.Length;
        if (!Module._isInInputMode)
            start = 0;
        for (int i = start; i < ans.Length; i++)
        {
            for (int j = 1; j < combos.Length; j++)
            {
                if (Module._alphabet[j] == ans[i])
                {
                    yield return Process(combos[j]);
                    break;
                }
            }
        }
        yield return Process("S");
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
            yield return new WaitForSeconds(.075f);
        }
    }
}
