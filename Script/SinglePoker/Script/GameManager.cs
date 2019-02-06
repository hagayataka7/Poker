using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif


public enum CardMark
{ Spade, Dia, clover, heart, joker, }
enum GamePhase
{ First, SetUp, Change, Select, RoleCheck, GameEnd, }
enum Rank
{ Not = 0, Bad = 1, Normal = 2, Steel = 3, Bronze = 4, Silver = 5, Gold = 6 }
enum CardRole
{
    NoPair = 0, OnePair = 1, TwoPair = 2, ThreeCard = 3, Straight = 5,
    Flash = 6, FullHouse = 10, FourCard = 15, StraightFlash = 30,
    RoyalStraightFlash = 100,
}

public class GameManager : MonoBehaviour
{
    
    DoubleUp getDoubleUp;
    Player player;
    CardPool pool;
    ShowSpace showSpace;
    [SerializeField]
    SSDataControl seve;
    [SerializeField]
    GameObject playerObj;//プレイヤーの管理用のオブジェクト
    [SerializeField]
    GameObject cardPoolObj;//山札の管理用のオブジェクト
    [SerializeField]
    GameObject showZone;//役の場の管理用のオブジェクト
    [SerializeField]
    GameObject doubleUpobj;//ダブルアップの管理用のオブジェクト
    [SerializeField]
    GameObject textBox;//説明用のテキスト用のUIオブジェクト
    [SerializeField]
    GameObject setPanel;//賭けポイントの用UIオブジェクト
    [SerializeField]
    GameObject UISelectFrame;//賭けポイントUIオブジェクトの選択用イメージ
    [SerializeField]
    GameObject EndMessageBox;//終了時用オブジェクト
    [SerializeField]
    GameObject DoubleUpBox;//ダブルアップ用のUIオブジェクト
    [SerializeField]
    GameObject DoubleUpSelectBox;//ダブルアップ用のUIオブジェクトの選択用イメージ
    [SerializeField]
    GameObject GameEndBood;
    [SerializeField]
    GameObject ChangeButton, ShowButton, NextButton, SelectFrame, SEobj, StartPanel;
    [SerializeField]
    GameObject[] Bar = new GameObject[5], coin = new GameObject[3];
    [SerializeField]
    GameObject[] Yes_No = new GameObject[2];
    [SerializeField]
    Text textMessage, SetText, Point, EndText, DoubleUpText, GameEndMessage;
    [SerializeField]
    AxlsInput GetAxlsInput;
    AudioSource[] SE = new AudioSource[2];
    [HideInInspector]
    public bool Straight = false, RoyalStraight = false;
    int[] NextRankPoint = { 200, 1000, 2500, 7500, 10000, 25000 };
    GamePhase gamePhase = GamePhase.First;
    CardRole FinishCardRole = CardRole.NoPair;
    Coroutine coroutine;
    int UesPoint = 10;
    int PlayCount = 3;
    public int GetPoint = 0, DoubleUpPlay = 0;
    int CardHandNum = 0;
    bool Flash = false, StraighFlash = false;
    void Start()
    {
        SE = SEobj.GetComponents<AudioSource>();
        Screen.SetResolution(1920, 1080, true);
        player = playerObj.GetComponent<Player>();
        CardHandNum = player.PlayerZone.Count;
        pool = cardPoolObj.GetComponent<CardPool>();

        getDoubleUp = doubleUpobj.GetComponent<DoubleUp>();

        showSpace = showZone.GetComponent<ShowSpace>();
        showSpace.manager = this;

        StartCoroutine(GameStart());

    }
    IEnumerator GameStart()
    {
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton1))
            {
                StartPanel.SetActive(false);
                if (pool.CardCreate())
                {
                    StartCoroutine(PointSet());
                    yield break;
                }
            }
            yield return null;
        }

    }
    int PointIndex = 0;
    void NextPointSet()
    {
        int point = player.PlayerPoint;

        if (PointIndex != 6 && NextRankPoint[PointIndex] <= point)
        { PointIndex++; NextPointSet(); return; }

        if (PointIndex != 0 && NextRankPoint[PointIndex - 1] > point)
        { PointIndex--; NextPointSet(); return; }

        SetText.text += "現在のランクは" + Valuation(point).ToString() + "です\n";

        if (PointIndex != 6)
        { SetText.text += "次のランクまで残り" + (NextRankPoint[PointIndex] - point) + "ポイントです"; }
    }
    /// <summary>かけるポイントの設定 </summary>
    IEnumerator PointSet()
    {
        GetPoint = 0;
        if (player.PlayerPoint < 10) { UesPoint = player.PlayerPoint; }
        else { UesPoint = 10; }
        setPanel.SetActive(true);
        yield return new WaitForSeconds(0.01f);
        bool Select = false, NumSet = false;
        int SelectNum = 0;
        SetText.text =
            "現在のポイントは" + player.PlayerPoint + "ポイントです\n" +
            "何ポイント使いますか?\n" +
            "残りプレイ回数は" + PlayCount + "回です\n";
        NextPointSet();
        Point.text = string.Format("{0:D5}", UesPoint);
        for (; ; )
        {
            if (!NumSet)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) ||
                    GetAxlsInput.Y_Minus || GetAxlsInput.Y_Plus)
                { Select = !Select; }
            }
            if (Select)
            {
                UISelectFrame.transform.position = NextButton.transform.position;
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton1))
                {
                    setPanel.SetActive(false);
                    player.PlayerPoint -= UesPoint;
                    MainGame();
                    yield break;
                }
            }
            else
            {
                UISelectFrame.transform.position = Point.transform.position;
                if (NumSet)
                {
                    if (Input.GetKeyDown(KeyCode.JoystickButton0))
                    {
                        NumSet = false;
                        UISelectFrame.SetActive(true); Bar[SelectNum].SetActive(false);
                    }
                }
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton1))
                {
                    NumSet = !NumSet;
                    if (NumSet)
                    { UISelectFrame.SetActive(false); Bar[SelectNum].SetActive(true); }
                    else
                    { UISelectFrame.SetActive(true); Bar[SelectNum].SetActive(false); }
                }
                if (NumSet)
                {
                    if (Input.GetKeyDown(KeyCode.LeftArrow) || GetAxlsInput.X_Minus)
                    {
                        if (SelectNum < 4)
                        {
                            SelectNum++;
                            Bar[SelectNum].SetActive(true);
                            Bar[SelectNum - 1].SetActive(false);
                        }
                    }
                    if (Input.GetKeyDown(KeyCode.RightArrow) || GetAxlsInput.X_Plus)
                    {
                        if (SelectNum > 0)
                        {
                            SelectNum--;
                            Bar[SelectNum].SetActive(true);
                            Bar[SelectNum + 1].SetActive(false);
                        }
                    }
                    if (Input.GetKeyDown(KeyCode.UpArrow) || GetAxlsInput.Y_Plus)
                    {
                        switch (SelectNum)
                        {
                            case 0: UesPoint += 1; break;
                            case 1: UesPoint += 10; break;
                            case 2: UesPoint += 100; break;
                            case 3: UesPoint += 1000; break;
                            case 4: UesPoint += 10000; break;
                        }
                        if (player.PlayerPoint < UesPoint)
                        { UesPoint = player.PlayerPoint; }
                        if (99999 <= UesPoint)
                        { UesPoint = 99999; }
                    }
                    if (Input.GetKeyDown(KeyCode.DownArrow) || GetAxlsInput.Y_Minus)
                    {
                        switch (SelectNum)
                        {
                            case 0: UesPoint -= 1; break;
                            case 1: UesPoint -= 10; break;
                            case 2: UesPoint -= 100; break;
                            case 3: UesPoint -= 1000; break;
                            case 4: UesPoint -= 10000; break;
                        }
                        if (1 > UesPoint)
                        { UesPoint = 1; }
                    }
                    Point.text = string.Format("{0:D5}", UesPoint);
                }
            }
            yield return null;
        }

    }
    /// <summary>進行管理関数 </summary>
    void MainGame()
    {
        switch (gamePhase)
        {
            case GamePhase.First:                       //最初にカードを配る
                StartSet();
                break;
            case GamePhase.SetUp:                       //配ったカードを番号順に並べる
                StartCoroutine(SetUpWait());
                break;
            case GamePhase.Change:                      //交換するカードを選択する
                coroutine = StartCoroutine(TextPou("交換するカードを選択してください"));
                StartCoroutine(CardChoice());
                break;
            case GamePhase.Select:                      //役を作るカードを選択する
                coroutine = StartCoroutine(TextPou("役を作るカードを5選択してください"));
                StartCoroutine(CardChoice());
                break;
            case GamePhase.RoleCheck:                   //役を確認する
                RoleCheck();
                break;
            case GamePhase.GameEnd:                     //再スタートまたは終了
                RoleCheck();
                break;
            default: break;
        }
    }

    IEnumerator TextPou(string Message)
    {
        int TimeLimit = 0;
        textBox.SetActive(true);
        textMessage.text = Message;
        while (TimeLimit <= 2)
        {
            TimeLimit++;
            yield return new WaitForSeconds(1);
        }
        textBox.SetActive(false);
        coroutine = null;
    }
    void TextFalse()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            textBox.SetActive(false);
        }
    }
    /// <summary>カード配布</summary>
    void StartSet()
    {
        for (int i = 0; i < CardHandNum; i++)
        {
            if (player.PlayerCard[i] == null)                                     //カード交換時にも使用するためプレイヤーの持ち札の空いているところに配る
            {
                GameObject Card = pool.CardList[0];
                pool.CardList.Remove(Card);                                       //山札から分離
                player.PlayerCard[i] = Card;                                      //カードオブジェクトをプレイヤーに登録
                Card.transform.SetParent(player.PlayerZone[i].transform);         //親子関係をセット
            }
        }
        StartCoroutine(CardMove());                                               //配るための関数を開始
    }
    /// <summary> カードを配るアニメーションための関数</summary>
    IEnumerator CardMove()
    {
        for (int i = 0; i < CardHandNum; i++)
        {
            if (player.PlayerCardScript[i] == null)
            {
                player.PlayerCardScript[i] = player.PlayerCard[i].GetComponent<CardScript>();     //カードScriptをプレイヤーに登録
                SE[0].PlayOneShot(SE[0].clip);
                player.PlayerCard[i].transform.DOMove(player.PlayerZone[i].transform.position, 0.5f);
                yield return new WaitForSeconds(0.25f);
            }
        }
        yield return new WaitForSeconds(0.25f);
        for (int i = 0; i < CardHandNum; i++)
        { player.PlayerCard[i].transform.DORotate(new Vector3(0f, 0f), 0.5f, RotateMode.FastBeyond360); }
        SE[1].PlayOneShot(SE[1].clip);
        yield return new WaitForSeconds(0.5f);
        switch (gamePhase)
        {
            case GamePhase.First:
                gamePhase = GamePhase.SetUp;
                MainGame();
                break;
            case GamePhase.Change:
                StartCoroutine(SetUpWait());
                break;
        }
    }
    IEnumerator SetUpWait()
    {
        StartCoroutine(player.CardSetUp());
        while (player.SetUp != null)
        { yield return null; }
        switch (gamePhase)
        {
            case GamePhase.SetUp: gamePhase = GamePhase.Change; break;
            case GamePhase.Change: gamePhase = GamePhase.Select; break;
        }
        MainGame();
    }

    /// <summary>カードを選択する</summary>
    IEnumerator CardChoice()
    {
        int ObjIndex = 0;
        bool EndFlag = false, Select = true;
        List<int> ChangeCardIndex = new List<int>();
        if (gamePhase == GamePhase.Change)
        { ChangeButton.SetActive(true); SelectFrame.SetActive(true); }
        if (gamePhase == GamePhase.Select)
        { ShowButton.SetActive(true); SelectFrame.SetActive(true); }
        Vector3 FrameScale = SelectFrame.transform.localScale;
        while (!EndFlag)
        {
            if (Select)
            {
                SelectFrame.transform.position = player.PlayerZone[ObjIndex].transform.position;
                SelectFrame.transform.localScale = FrameScale;
            }
            else
            {
                if (gamePhase == GamePhase.Change)
                {
                    SelectFrame.transform.position = ChangeButton.transform.position;
                    SelectFrame.transform.localScale = new Vector3(50, 30, 1);
                }
                if (gamePhase == GamePhase.Select)
                {
                    SelectFrame.transform.position = ShowButton.transform.position;
                    SelectFrame.transform.localScale = new Vector3(50, 30, 1);
                }
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow) ||
                GetAxlsInput.X_Minus)
            { ObjIndex--; if (ObjIndex < 0) ObjIndex = 6; }
            if (Input.GetKeyDown(KeyCode.RightArrow) ||
                GetAxlsInput.X_Plus)
            { ObjIndex++; if (ObjIndex > 6) ObjIndex = 0; }
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) ||
                GetAxlsInput.Y_Minus || GetAxlsInput.Y_Plus)
            { Select = !Select; }
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton1))
            {
                if (Select)
                {
                    int Index = ChangeCardIndex.IndexOf(ObjIndex);
                    if (Index == -1)
                    {
                        if (gamePhase == GamePhase.Select)
                        {
                            if (ChangeCardIndex.Count < 5)
                            { CardUp(ChangeCardIndex, ObjIndex); }
                        }
                        else
                        { CardUp(ChangeCardIndex, ObjIndex); }
                    }
                    else
                    {
                        SE[0].PlayOneShot(SE[0].clip);
                        ChangeCardIndex.Remove(ObjIndex);
                        player.PlayerCard[ObjIndex].transform.DOMove(player.PlayerZone[ObjIndex].transform.position, 0.5f);
                    }
                }
                else
                {
                    switch (gamePhase)
                    {
                        case GamePhase.Change:
                            EndFlag = true;
                            break;
                        case GamePhase.Select:
                            if (ChangeCardIndex.Count == 5)
                            { EndFlag = true; TextFalse(); }
                            else
                                if (coroutine == null)
                                coroutine = StartCoroutine(TextPou("5枚選択してください"));
                            break;
                    }
                }
            }
            yield return null;
        }
        SelectFrame.transform.localScale = FrameScale;
        if (gamePhase == GamePhase.Change)
        { ChangeButton.SetActive(false); SelectFrame.SetActive(false); }
        if (gamePhase == GamePhase.Select)
        { ShowButton.SetActive(false); SelectFrame.SetActive(false); }
        ChangeCard(ChangeCardIndex);
    }
    void CardUp(List<int> ChangeCardIndex, int ObjIndex)
    {
        SE[0].PlayOneShot(SE[0].clip);
        ChangeCardIndex.Add(ObjIndex);
        player.PlayerCard[ObjIndex].transform.DOMove(new Vector3(
          player.PlayerZone[ObjIndex].transform.position.x,
          player.PlayerZone[ObjIndex].transform.position.y + 1,
          player.PlayerZone[ObjIndex].transform.position.z), 0.5F);

    }
    void ChangeCard(List<int> ChangeCardIndex)
    {
        if (gamePhase == GamePhase.Select)
        { showZone.SetActive(true); }
        int num = ChangeCardIndex.Count;
        for (int i = 0; i < CardHandNum; i++)
        {
            for (int j = 0; j < num; j++)
            {
                if (gamePhase == GamePhase.Change)
                {
                    if (i == ChangeCardIndex[j])
                    {
                        player.PlayerCard[i].transform.DORotate(new Vector3(0f, 180f), 0.5f);
                        player.PlayerCard[i].transform.SetParent(cardPoolObj.transform);
                        player.PlayerCard[i].transform.DOMove(cardPoolObj.transform.position, 0.5f);
                        player.PlayerCard[i] = null;
                        player.PlayerCardScript[i] = null;
                    }
                }
                if (gamePhase == GamePhase.Select)
                {
                    if (i == ChangeCardIndex[j])
                    {
                        showSpace.ShowCard.Add(player.PlayerCard[i]);
                        showSpace.ShowcardScripts.Add(player.PlayerCardScript[i]);
                    }
                }
            }
        }
        SE[0].PlayOneShot(SE[0].clip);
        switch (gamePhase)
        {
            case GamePhase.Change:
                if (num != 0) { StartSet(); }
                else { gamePhase = GamePhase.Select; MainGame(); };
                break;
            case GamePhase.Select:
                showSpace.ShowSetUp();
                for (int i = 0; i < 5; i++)
                { showSpace.ShowCard[i].transform.DOMove(showSpace.ShowFrame[i].transform.position, 0.5f); }
                gamePhase = GamePhase.RoleCheck; MainGame();
                break;
        }
    }

    void RoleCheck()
    {
        FinishCardRole = Straight_FlashCheck();
        GetPoint = (int)FinishCardRole * UesPoint;
        textBox.SetActive(true);

        if (FinishCardRole > CardRole.OnePair && (GetPoint > 200 || FinishCardRole >= CardRole.Flash))
        { coin[2].SetActive(true); }
        else if (FinishCardRole >= CardRole.OnePair && (GetPoint > 50 || (GetPoint > 40 && FinishCardRole <= CardRole.ThreeCard)))
        { coin[1].SetActive(true); }
        else if (GetPoint > 1)
        { coin[0].SetActive(true); }
        textMessage.text = FinishCardRole.ToString();
        PlayCount--;
        StartCoroutine(NextGame());
    }
    CardRole Straight_FlashCheck()
    {
        bool[] CheckFlag = new bool[5];

        for (int i = 0; i < 5; i++)
        { CheckFlag[i] = showSpace.ShowcardScripts[0].cardMark == showSpace.ShowcardScripts[i].cardMark; }
        if (CheckFlag[0] && CheckFlag[1] && CheckFlag[2] && CheckFlag[3] && CheckFlag[4])
        { Flash = true; }

        if (RoyalStraight && Flash)
        { return CardRole.RoyalStraightFlash; }
        else
        { RoyalStraight = false; }

        if (!Straight)
        { CheckFlag = new bool[4]; }
        for (int i = 0; i < 4; i++)
        { CheckFlag[i] = showSpace.ShowcardScripts[i].CardNum + 1 == showSpace.ShowcardScripts[i + 1].CardNum; }
        if (CheckFlag[0] && CheckFlag[1] && CheckFlag[2] && CheckFlag[3])
        { Straight = true; }

        if (Straight && Flash)
        { return CardRole.StraightFlash; }
        else if (Straight)
        { return CardRole.Straight; }
        else if (Flash)
        { return CardRole.Flash; }
        else
        { return PairCheck(); }
    }
    CardRole PairCheck()
    {
        int[] PairCard = new int[5];
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                if (showSpace.ShowcardScripts[i].CardNum == showSpace.ShowcardScripts[j].CardNum)
                { PairCard[i]++; }
            }
        }
        for (int i = 0; i < 5; i++)
        {
            if (PairCard[i] == 4)
            { return CardRole.FourCard; }

            else if (PairCard[i] == 3)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (PairCard[j] == 2)
                    { return CardRole.FullHouse; }
                }
                return CardRole.ThreeCard;
            }
            else if (PairCard[i] == 2)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (PairCard[j] == 3)
                    { return CardRole.FullHouse; }
                    else if (PairCard[j] == 2 && showSpace.ShowcardScripts[i].CardNum != showSpace.ShowcardScripts[j].CardNum)
                    { return CardRole.TwoPair; }
                }
                return CardRole.OnePair;
            }
        }
        return CardRole.NoPair;
    }
    IEnumerator NextGame()
    {
        Straight = false; Flash = false;
        StraighFlash = false; RoyalStraight = false;
        if (PlayCount >= 0 && (player.PlayerPoint == 0 && GetPoint == 0))
        {
            GameEndBood.SetActive(true);
            yield return new WaitForSeconds(2);
            GameEndBood.SetActive(false);
        }
        else
        {
            for (; ; )
            {
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton1))
                    break;
                yield return null;
            }
        }
        coin[0].SetActive(false);
        showZone.SetActive(false);
        textBox.SetActive(false);
        player.CardReset();
        pool.poolShuffle();
        showSpace.ShowCard.Clear();
        showSpace.ShowcardScripts.Clear();
        gamePhase = GamePhase.First;
        yield return new WaitForSeconds(0.5f);
        if (player.PlayerPoint <= 0)
        { player.PlayerPoint = 0; }
        if (GetPoint >= 0)
        {
            DoubleUpPlay = 3;
            if (FinishCardRole == CardRole.OnePair)
            { DoubleUpPlay = 1; }
            StartCoroutine(DoubleUpSelect());
        }
        else if (PlayCount <= 0 || (player.PlayerPoint <= 0 && GetPoint <= 0))
        {
            player.PlayerPoint += GetPoint;
            StartCoroutine(GameEnd());
        }
        else
        { StartCoroutine(PointSet()); }
    }
    public IEnumerator DoubleUpSelect()
    {
        if (GetPoint <= 0 || DoubleUpPlay == 0)
        {
            playerObj.SetActive(true);
            doubleUpobj.SetActive(false);
            player.PlayerPoint += GetPoint;
            if (player.PlayerPoint == 0)
            { StartCoroutine(GameEnd()); }
            else
            {
                if (PlayCount <= 0)
                { StartCoroutine(GameEnd()); }
                else
                { StartCoroutine(PointSet()); }
            }
            yield break;
        }
        DoubleUpBox.SetActive(true);
        DoubleUpText.text =
            "ダブルアップを行いますか\n" +
            "挑戦できる回数は" + DoubleUpPlay + "回です\n" +
           "獲得のポイントは" + GetPoint + "ポイントです\n";
        int Select = 0;
        for (; ; )
        {
            DoubleUpSelectBox.transform.position = Yes_No[Select].transform.position;
            if (Input.GetKeyDown(KeyCode.LeftArrow) || GetAxlsInput.X_Minus)
            { Select++; if (Select == 2) Select = 0; }
            if (Input.GetKeyDown(KeyCode.RightArrow) || GetAxlsInput.X_Plus)
            { Select--; if (Select == -1) Select = 1; }
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton1))
            { break; }
            yield return null;
        }
        DoubleUpBox.SetActive(false);
        if (Select == 1)
        {
            playerObj.SetActive(false);
            doubleUpobj.SetActive(true);
            StartCoroutine(getDoubleUp.DoubleUpSet());
        }
        else
        {
            playerObj.SetActive(true);
            doubleUpobj.SetActive(false);
            player.PlayerPoint += GetPoint;
            if (PlayCount <= 0)
            { StartCoroutine(GameEnd()); }
            else
            { StartCoroutine(PointSet()); }
        }
    }

    Rank Valuation(int point)
    {
        Rank rank = Rank.Not;

        if (point >= 25000) { rank = Rank.Gold; }
        else if (point >= 10000) { rank = Rank.Silver; }
        else if (point >= 7500) { rank = Rank.Bronze; }
        else if (point >= 2500) { rank = Rank.Steel; }
        else if (point >= 1000) { rank = Rank.Normal; }
        else if (point >= 200) { rank = Rank.Bad; }
        Debug.Log(rank);
        return rank;
    }
    IEnumerator GameEnd()
    {
        Rank rank = Valuation(player.PlayerPoint);
        seve.SaveData(0, 1, (int)rank, 7);
        yield return new WaitForSeconds(0.01f);
        EndMessageBox.SetActive(true);
        EndText.text = "最終所持ポイントは" + player.PlayerPoint + "です\n" +
       "今回のランクは" + rank.ToString() + "です";
        for (; ; )
        {
            if (Input.GetKey(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton1))
            {
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
      Application.Quit();
#endif
            }
            yield return null;
        }
    }
}