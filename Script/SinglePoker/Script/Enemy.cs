using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Enemy : MonoBehaviour
{

    [SerializeField]
    GameObject SeObj, cardPoolObj, EnemyShowSpace;
    ShowSpace showSpace;
    [SerializeField]
    CardPool cardPool;
    AudioSource[] SE = new AudioSource[2];
    public List<GameObject> EnemyZone = new List<GameObject>();
    public List<GameObject> EnemyCard = new List<GameObject>();
    public List<CardScript> EnemyCardScript = new List<CardScript>();
    public IEnumerator SetUp;
    public bool SelectEnd = false, showEnd = false;
    public CardRole Role = CardRole.Non;
    List<int> CardIndex = new List<int>();

    private void Start()
    {
        SE = SeObj.GetComponents<AudioSource>();
        showSpace = EnemyShowSpace.GetComponent<ShowSpace>();
    }
    /// <summary>カードを並び替えるアニメーション関数</summary>
    public IEnumerator CardSetUp()
    {
        showEnd = false;
        SetUp = CardSetUp();
        //for (int i = 0; i < EnemyZone.Count; i++)
        //{ EnemyCard[i].transform.DORotate(new Vector3(0f, 0f), 0.5f, RotateMode.FastBeyond360); }
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < EnemyZone.Count; i++)
        { EnemyCard[i].transform.DOMove(gameObject.transform.position, 0.5f); }
        SE[0].PlayOneShot(SE[0].clip);
        yield return new WaitForSeconds(0.5f);
        PlayerSetUp();                                                                                            //カードを並び替える関数
        for (int i = 0; i < EnemyZone.Count; i++)
        { EnemyCard[i].transform.DOMove(EnemyZone[i].transform.position,0.5f); }
        SE[0].PlayOneShot(SE[0].clip);
        yield return new WaitForSeconds(1f);
        SetUp = null;
    }
    /// <summary>カードを並び替える関数</summary>
    void PlayerSetUp()
    {
        string[] str = new string[Const_int.MaxCard];
        List<CardScript> CopyCardScripts = new List<CardScript>(EnemyCardScript); //手札のコピーを作成
        List<GameObject> CopyEnemyCard = new List<GameObject>(EnemyCard);        //手札の内容のコピーを作成
        for (int i = 0; i < CopyEnemyCard.Count; i++)
        { str[i] = EnemyCardScript[i].name; }
        Array.Sort(str);　　                                                           //登録した名前をソート
        for (int i = 0; i < EnemyZone.Count; i++)
        {
            for (int j = 0; j < EnemyZone.Count; j++)
            {
                if (EnemyCard[j].name == str[i])                                  //コピー上でソートとした名前のインデックス順に手札を並び替える
                {
                    CopyCardScripts[i] = EnemyCardScript[j];
                    CopyEnemyCard[i] = EnemyCard[j];
                    EnemyCard[j].transform.SetParent(EnemyZone[i].transform);
                    EnemyCardScript[j].CardIndex = i;
                    EnemyCardScript[j].Select = false;
                }
            }
        }
        EnemyCardScript = CopyCardScripts;　　　　　　　　　　　　　　　　　　　　//コピーをコピー元に移す
        EnemyCard = CopyEnemyCard;
    }
    public void RoleCheck(GamePhase gamePhase)
    {
        SelectEnd = false;
        Role = CardRole.Non;
        if (PairCheck(EnemyCardScript, CardRole.FourCard))
        { DebugCardNum(); }
        else if (FullHouse_TwoPairCheck(EnemyCardScript, CardRole.FullHouse))
        { DebugCardNum(); }
        else if (FlashCheck(EnemyCardScript))
        { DebugCardNum(); }
        else if (PairCheck(EnemyCardScript, CardRole.ThreeCard))
        { DebugCardNum(); }
        else if (FullHouse_TwoPairCheck(EnemyCardScript, CardRole.TwoPair))
        { DebugCardNum(); }
        else if (PairCheck(EnemyCardScript, CardRole.OnePair))
        { DebugCardNum(); }

        if (Role == CardRole.Non)
        { CardIndex.Clear(); }

        for (int i = 0; i < CardIndex.Count; i++)
        { EnemyCardScript[CardIndex[i]].Select = true; }

        switch (gamePhase)
        {
            case GamePhase.Change: CardChange(); break;
            case GamePhase.Select: StartCoroutine(CardShow()); break;
        }

        Debug.Log(Role);

    }
    void DebugCardNum()
    {
        for (int i = 0; i < CardIndex.Count; i++)
        {
            Debug.Log(EnemyCardScript[CardIndex[i]].CardNum);
        }
    }

    bool FlashCheck(List<CardScript> cardScript)
    {
        for (int i = 0; i < Const_int.MaxCard; i++)
        {
            CardIndex.Clear();
            for (int j = 0; j < Const_int.MaxCard; j++)
            {
                if (cardScript[i].cardMark == cardScript[j].cardMark)
                {
                    CardIndex.Add(cardScript[j].CardIndex);
                    if (CardIndex.Count == 5)
                    {
                        Role = CardRole.Flash;
                        return true;
                    }
                }
            }
        }
        return false;
    }
    bool PairCheck(List<CardScript> cardScript, CardRole role)
    {
        for (int i = 0; i < Const_int.MaxCard; i++)
        {
            CardIndex.Clear();
            for (int j = 0; j < Const_int.MaxCard; j++)
            {
                if (cardScript[i].CardNum == cardScript[j].CardNum)
                { CardIndex.Add(cardScript[j].CardIndex); }
            }
            if (CardIndex.Count == 4 && role == CardRole.FourCard)
            { Role = CardRole.FourCard; return true; }
            if (CardIndex.Count == 3 && role == CardRole.ThreeCard)
            { Role = CardRole.ThreeCard; return true; }
            if (CardIndex.Count == 2 && role == CardRole.OnePair)
            { Role = CardRole.OnePair; return true; }
        }
        return false;
    }
    bool FullHouse_TwoPairCheck(List<CardScript> cardScript, CardRole role)
    {
        for (int i = 0; i < Const_int.MaxCard; i++)
        {
            CardIndex.Clear();

            switch (role)
            {
                case CardRole.FullHouse:
                    if (PairCheck(cardScript, CardRole.ThreeCard) || PairCheck(cardScript, CardRole.ThreeCard))
                    {
                        for (int j = 0; j < Const_int.MaxCard; j++)
                        {
                            if (cardScript[i].CardNum == cardScript[j].CardNum &&
                                cardScript[CardIndex[0]].CardNum != cardScript[j].CardNum)
                            { CardIndex.Add(cardScript[j].CardIndex); }
                            if (CardIndex.Count == 5)
                            { Role = CardRole.FullHouse; return true; }

                        }
                    }
                    break;

                case CardRole.TwoPair:
                    if (PairCheck(cardScript, CardRole.OnePair))
                    {
                        for (int j = 0; j < Const_int.MaxCard; j++)
                        {
                            if (cardScript[i].CardNum == cardScript[j].CardNum &&
                                cardScript[CardIndex[0]].CardNum != cardScript[j].CardNum)
                            { CardIndex.Add(cardScript[j].CardIndex); }
                            if (CardIndex.Count == 4)
                            { Role = CardRole.TwoPair; return true; }
                        }
                    }
                    break;
            }
        }
        return false;
    }
    void CardChange()
    {
        for (int i = 0; i < Const_int.MaxCard; i++)
        {
            if (!EnemyCardScript[i].Select)
            {
                EnemyCard[i].transform.DORotate(new Vector3(0f, 180f), 0.5f);
                EnemyCard[i].transform.SetParent(cardPoolObj.transform);
                EnemyCard[i].transform.DOMove(cardPoolObj.transform.position, 0.5f);
                cardPool.CardList.Add(EnemyCard[i]);
                EnemyCard[i] = null;
                EnemyCardScript[i] = null;
            }
        }
        SelectEnd = true;
    }
    IEnumerator CardShow()
    {
        for (int i = 0; i < Const_int.MaxCard; i++)
        {
            if (EnemyCardScript[i].Select)
            {
                showSpace.ShowCard.Add(EnemyCard[i]);
                showSpace.ShowcardScripts.Add(EnemyCardScript[i]);
            }
        }
        for (int i = 0; ; i++)
        {
            if (showSpace.ShowCard.Count == 5)
            { break; }
            if (!EnemyCardScript[i].Select)
            {
                showSpace.ShowCard.Add(EnemyCard[i]);
                showSpace.ShowcardScripts.Add(EnemyCardScript[i]);
            }
          
        }
        EnemyShowSpace.SetActive(true);
        showSpace.ShowSetUp(gameObject);
        for (int i = 0; i < Const_int.ShowCard; i++)
        { showSpace.ShowCard[i].transform.DOMove(showSpace.ShowFrame[i].transform.position, 0.5f); }

        yield return new WaitForSeconds(1f);

        for (int i = 0; i < 5; i++)
        { showSpace.ShowCard[i].transform.DORotate(new Vector3(0f, 0f), 0.5f, RotateMode.FastBeyond360); }
        SE[1].PlayOneShot(SE[1].clip);
        
        showEnd = true;
    }
    public void ShowSpaceReset()
    {
        EnemyShowSpace.SetActive(false);
        showSpace.ShowCard.Clear();
        showSpace.ShowcardScripts.Clear();
    }
    public void CardReset()
    {
        for (int i = 0; i < EnemyZone.Count; i++)
        {
            EnemyCard[i].transform.DORotate(new Vector3(0f, 180f), 0.5f);
            EnemyCard[i].transform.SetParent(cardPoolObj.transform);
            EnemyCard[i].transform.DOMove(cardPoolObj.transform.position, 0.5f);
            cardPool.CardList.Add(EnemyCard[i]);
            EnemyCard[i] = null;
            EnemyCardScript[i] = null;
        }
    }
}
