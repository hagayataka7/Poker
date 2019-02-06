using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowSpace : MonoBehaviour
{
    public List<GameObject> ShowFrame = new List<GameObject>();
    public List<GameObject> ShowCard = new List<GameObject>();
    public List<CardScript> ShowcardScripts = new List<CardScript>();
    public GameManager manager;
    public void ShowSetUp()
    {
        bool[] CheckFlag = { false, false, false, false, false };
        int[] StraightPattern = { 1, 10, 11, 12, 13 };
        string[] str = new string[5];
        List<CardScript> CopyScripts = new List<CardScript>(ShowcardScripts);
        List<GameObject> CopyCard = new List<GameObject>(ShowCard);
        for (int i = 0; i < 5; i++)
        {
            str[i] = ShowCard[i].name;
        }
        Array.Sort(str);
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                if (ShowCard[j].name == str[i])
                {
                    CopyScripts[i] = ShowcardScripts[j];
                    CopyCard[i] = ShowCard[j];
                }
            }
        }
        ShowcardScripts = new List<CardScript>(CopyScripts);
        ShowCard = new List<GameObject>(CopyCard);
        for (int i = 0; i < 5; i++)
        { CheckFlag[i] = ShowcardScripts[i].CardNum == StraightPattern[i]; }
        if (CheckFlag[0] && CheckFlag[1] && CheckFlag[2] && CheckFlag[3] && CheckFlag[4])
        { manager. Straight = true; manager.RoyalStraight = true; }
        if (manager.Straight)
        {
            ShowCard[0] = CopyCard[1];
            ShowCard[1] = CopyCard[2];
            ShowCard[2] = CopyCard[3];
            ShowCard[3] = CopyCard[4];
            ShowCard[4] = CopyCard[0];

            ShowcardScripts[0] = CopyScripts[1];
            ShowcardScripts[1] = CopyScripts[2];
            ShowcardScripts[2] = CopyScripts[3];
            ShowcardScripts[3] = CopyScripts[4];
            ShowcardScripts[4] = CopyScripts[0];
        }
    }
}
