using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;


public class CardPool : MonoBehaviour
{
    // Use this for initialization
    [SerializeField]
    GameObject Pool;
    public List<GameObject> CardList = new List<GameObject>();
    [SerializeField]
    GameObject CardPlain;
    [SerializeField] List<Sprite> sprite = new List<Sprite>();
    string[] words = new string[2];
    SpriteRenderer spriteRenderer;
    CardScript cardScript;
    public bool CardCreate()
    {
        for (int i = 0; i < 52; i++)
        {
            GameObject Cardmata = Instantiate(CardPlain);
            Cardmata.transform.SetParent(transform);
            Cardmata.transform.position = Pool.transform.position;
            cardScript = Cardmata.GetComponent<CardScript>();
            words = sprite[i].name.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
            int num = int.Parse(words[1]);
            if (num <= 9)
                Cardmata.name = "0" + num;
            else
                Cardmata.name = num.ToString();

            switch (words[0])
            {
                case "s":
                    cardScript.cardMark = CardMark.Spade;
                    Cardmata.name += "S";
                    break;
                case "d":
                    cardScript.cardMark = CardMark.Dia;
                    Cardmata.name += "D";
                    break;
                case "c":
                    cardScript.cardMark = CardMark.clover;
                    Cardmata.name += "C";
                    break;
                case "h":
                    cardScript.cardMark = CardMark.heart;
                    Cardmata.name += "H";
                    break;
            }
            Cardmata.GetComponent<SpriteRenderer>().sprite = sprite[i];
            cardScript.CardNum = num;
            CardList.Add(Cardmata);
        }
        int rand = UnityEngine.Random.Range(5, 10);
        for (int j = 0; j < rand; j++)
        { 
            CardList = CardList.OrderBy(i => Guid.NewGuid()).ToList();
        }
        poolShuffle();
        return true;

    }
    public void poolShuffle()
    {
        int rand = UnityEngine.Random.Range(5, 10);
        for (int j = 0; j < rand; j++)
        {
            CardList = CardList.OrderBy(i => Guid.NewGuid()).ToList();
        }
    }
}
