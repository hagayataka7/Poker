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
public enum GamePhase
{ First, SetUp, Change, Select, RoleCheck, GameEnd, }
enum Rank
{ Bad = 0, Bronze = 1, Silver = 2, Gold = 3 }
public enum CardRole
{
    Non = 0, OnePair = 1, TwoPair = 2, ThreeCard = 3, Straight = 5,
    Flash = 6, FullHouse = 10, FourCard = 15, StraightFlash = 30,
    RoyalStraightFlash = 100,
}

public class GameManager : MonoBehaviour
{
    DoubleUp getDoubleUp;
    Player player;
    CardPool pool;
    ShowSpace showSpace;
    Enemy enemy;
    [SerializeField]
    SSDataControl seve;
    [SerializeField]
    GameObject playerObj, enemyObj;//プレイヤーの管理用のオブジェクト
    [SerializeField]
    GameObject cardPoolObj;//山札の管理用のオブジェクト
    [SerializeField]
    GameObject showZone;//役の場の管理用のオブジェクト
    [SerializeField]
    GameObject textBox;//説明用のテキスト用のUIオブジェクト
    [SerializeField]
    GameObject setPanel;//賭けポイントの用UIオブジェクト
    [SerializeField]
    GameObject UISelectFrame;//賭けポイントUIオブジェクトの選択用イメージ
    [SerializeField]
    GameObject EndMessageBox;//終了時用オブジェクト
    [SerializeField]
    GameObject ShowButton, NextButton, SelectFrame, SEobj, StartPanel;
    [SerializeField]
    GameObject[] Bar = new GameObject[2], coin = new GameObject[3];
    [SerializeField]
    GameObject[] Yes_No = new GameObject[2];
    [SerializeField]
    GameObject[] RolePanel = new GameObject[3];
    [SerializeField]
    Text[] RoleText = new Text[3];
    [SerializeField]
    Text textMessage, SetText, EndText, NextPointText, ranktextbox;
    [SerializeField]
    AxlsInput GetAxlsInput;
    [SerializeField]
    GameObject[] ChangeButton = new GameObject[2];
    [HideInInspector]
    public bool Straight = false, RoyalStraight = false;
    [HideInInspector]
    public int GetPoint = 0, DoubleUpPlay = 0;
    AudioSource[] SystemSound = new AudioSource[2];
    GamePhase gamePhase = GamePhase.First;
    CardRole FinishCardRole = CardRole.Non;
    Coroutine coroutine;
    Vector3 DefaultScale = Vector3.zero;
    Vector3 defupos = Vector3.zero;
    int UesPoint = 10, PlayCount = 3;
    int EndFlagNum = 0, CardHandNum = 0;
    bool AllChangeFlag = false;
    bool Flash = false, StraighFlash = false;
    void Start()
    {

        SystemSound = SEobj.GetComponents<AudioSource>();
        Screen.SetResolution(1920, 1080, true);
        player = playerObj.GetComponent<Player>();
        CardHandNum = player.PlayerZone.Count;
        pool = cardPoolObj.GetComponent<CardPool>();
        enemy = enemyObj.GetComponent<Enemy>();
        DefaultScale = SelectFrame.transform.localScale;
        defupos = new Vector3(-3, 1, 0);
        showSpace = showZone.GetComponent<ShowSpace>();
        showSpace.manager = this;

        StartCoroutine(GameStart());

    }
    IEnumerator GameStart()
    {
        while (!Input.GetKeyDown(KeyCode.Space) && !Input.GetKeyDown(KeyCode.JoystickButton1))
        { yield return null; }
        StartPanel.SetActive(false);
        if (pool.CardCreate())
        { StartCoroutine(PointSet()); }
    }
    /// <summary>かけるポイントの設定 </summary>
    IEnumerator PointSet()
    {
        GetPoint = 0; UesPoint = 0;
        setPanel.SetActive(true);
        yield return new WaitForSeconds(0.01f);
        SetText.text =
            "現在の勝ち点は" + player.PlayerPoint + "点です\n" +
            "残りプレイ回数は" + PlayCount + "回です\n";
        ranktextbox.text = "現在のランクは" + Valuation(player.PlayerPoint) + "です";
        NextPointText.text = "勝利すると獲得出来る勝ち点は" + (UesPoint * 2 + 1) + "点です";
        for (; ; )
        {
            UISelectFrame.transform.position = NextButton.transform.position;
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton1))
            {
                setPanel.SetActive(false);
                player.PlayerPoint -= UesPoint;
                MainGame();
                yield break;
            }
            yield return null;
        }

    }

    /// <summary>進行管理関数</summary>
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
                StartCoroutine(RoleCheck());
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
        List<GameObject>[] PlayerList = { player.PlayerCard, enemy.EnemyCard };
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < CardHandNum; j++)
            {
                if (PlayerList[i][j] == null)                                     //カード交換時にも使用するためプレイヤーの持ち札の空いているところに配る
                {
                    GameObject Card = pool.CardList[0];
                    pool.CardList.Remove(Card);                                       //山札から分離
                    PlayerList[i][j] = Card;                                      //カードオブジェクトをプレイヤーに登録
                    switch (i)
                    {
                        case 0: Card.transform.SetParent(player.PlayerZone[j].transform); break;        //親子関係をセット
                        case 1: Card.transform.SetParent(enemy.EnemyZone[j].transform); break;        //親子関係をセット
                    }
                }
            }
        }
        StartCoroutine(CardMove());                                               //配るための関数を開始
    }
    /// <summary> カードを配るアニメーションための関数</summary>
    IEnumerator CardMove()
    {
        List<CardScript>[] ScriptList = { player.PlayerCardScript, enemy.EnemyCardScript };
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < CardHandNum; j++)
            {
                if (ScriptList[i][j] == null)
                {
                    switch (i)
                    {
                        case 0:
                            ScriptList[i][j] = player.PlayerCard[j].GetComponent<CardScript>();//カードScriptをプレイヤーに登録
                            player.PlayerCard[j].transform.DOMove(player.PlayerZone[j].transform.position, 0.5f);
                            break;
                        case 1:
                            enemy.EnemyCard[j].transform.Rotate(0, 0, 180);
                            ScriptList[i][j] = enemy.EnemyCard[j].GetComponent<CardScript>();
                            enemy.EnemyCard[j].transform.DOMove(player.PlayerZone[j].transform.position, 0.5f);
                            break;
                    }
                    SystemSound[0].PlayOneShot(SystemSound[0].clip);

                    ScriptList[i][j].transform.DOMove(ScriptList[i][j].transform.parent.transform.position, 0.5f);
                    yield return new WaitForSeconds(0.25f);
                }
            }
        }

        yield return new WaitForSeconds(0.25f);
        for (int i = 0; i < CardHandNum; i++)
        { player.PlayerCard[i].transform.DORotate(new Vector3(0f, 0f), 0.5f, RotateMode.FastBeyond360); }
        SystemSound[1].PlayOneShot(SystemSound[1].clip);
        yield return new WaitForSeconds(0.5f);
        if (!AllChangeFlag)
        {
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
        else
        {
            StartCoroutine(player.CardSetUp());
            yield return new WaitForSeconds(1.5f);
            StartCoroutine(CardChoice());
        }

    }
    IEnumerator SetUpWait()
    {
        StartCoroutine(player.CardSetUp());
        StartCoroutine(enemy.CardSetUp());
        while (player.SetUp != null && enemy.SetUp != null)
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
        int ObjIndex = 0, ButtonIndex = 0;
        bool EndFlag = false, Select = true;
        Debug.Log(ButtonIndex);
        List<int> ChangeCardIndex = new List<int>();
        if (gamePhase == GamePhase.Change)
        {
            if (!AllChangeFlag)
            { ChangeButton[1].SetActive(true); }
            if (AllChangeFlag)
            { ChangeButton[0].transform.position = ShowButton.transform.position; }
            else
            { ChangeButton[0].transform.position = defupos; }

            ChangeButton[0].SetActive(true);
            SelectFrame.SetActive(true);
        }
        if (gamePhase == GamePhase.Select)
        { ShowButton.SetActive(true); SelectFrame.SetActive(true); }
        SelectFrame.transform.localScale = DefaultScale;
        while (!EndFlag)
        {

            if (Select)
            {
                SelectFrame.transform.position = player.PlayerZone[ObjIndex].transform.position;
                SelectFrame.transform.localScale = DefaultScale;
            }
            else
            {
                if (AllChangeFlag) { ButtonIndex = 0; }
                if (gamePhase == GamePhase.Change)
                {
                    SelectFrame.transform.position = ChangeButton[ButtonIndex].transform.position;
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
            {
                if (Select)
                { ObjIndex--; if (ObjIndex < 0) ObjIndex = 6; }
                else
                { ButtonIndex--; if (ButtonIndex < 0) ButtonIndex = 1; }
            }
            if (Input.GetKeyDown(KeyCode.RightArrow) ||
                GetAxlsInput.X_Plus)
            {
                if (Select)
                { ObjIndex++; if (ObjIndex > 6) ObjIndex = 0; }
                else
                { ButtonIndex++; if (ButtonIndex > 1) ButtonIndex = 0; }
            }
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
                        SystemSound[0].PlayOneShot(SystemSound[0].clip);
                        ChangeCardIndex.Remove(ObjIndex);
                        player.PlayerCard[ObjIndex].transform.DOMove(player.PlayerZone[ObjIndex].transform.position, 0.5f);
                    }
                }
                else
                {
                    if (AllChangeFlag)
                        AllChangeFlag = false;
                    switch (gamePhase)
                    {
                        case GamePhase.Change:
                            if (ButtonIndex == 0)
                            { EndFlag = true; }
                            else
                            {
                                AllChangeFlag = true;
                                player.CardReset();
                                ChangeButton[0].SetActive(false);
                                ChangeButton[1].SetActive(false);
                                SelectFrame.SetActive(false);
                                StartSet();
                                yield break;
                            }
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
        SelectFrame.transform.localScale = DefaultScale;
        if (gamePhase == GamePhase.Change)
        { ChangeButton[0].SetActive(false); ChangeButton[1].SetActive(false); SelectFrame.SetActive(false); }
        if (gamePhase == GamePhase.Select)
        { ShowButton.SetActive(false); SelectFrame.SetActive(false); }
        ChangeCard(ChangeCardIndex);
    }
    void CardUp(List<int> ChangeCardIndex, int ObjIndex)
    {
        SystemSound[0].PlayOneShot(SystemSound[0].clip);
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
                        pool.CardList.Add(player.PlayerCard[i]);
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
        enemy.RoleCheck(gamePhase);
        SystemSound[0].PlayOneShot(SystemSound[0].clip);
        switch (gamePhase)
        {
            case GamePhase.Change:
                StartSet();
                break;
            case GamePhase.Select:
                showSpace.ShowSetUp(playerObj);
                for (int i = 0; i < 5; i++)
                { showSpace.ShowCard[i].transform.DOMove(showSpace.ShowFrame[i].transform.position, 0.5f); }
                gamePhase = GamePhase.RoleCheck; MainGame();
                break;
        }
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
        return CardRole.Non;
    }

    IEnumerator RoleCheck()
    {
        FinishCardRole = Straight_FlashCheck();
        RoleText[0].text = FinishCardRole.ToString();
        RolePanel[0].SetActive(true);

        while (!enemy.showEnd)
        { yield return null; }

        if (FinishCardRole > enemy.Role)
        { EndFlagNum = 1; player.PlayerPoint += 1; }
        else if (FinishCardRole == enemy.Role)
        { EndFlagNum = 0; }
        else
        { EndFlagNum = -1; }

        switch (EndFlagNum)
        {
            case 1:
                RoleText[2].text = "Your Winner";
                RoleText[2].color = Color.red;
                break;
            case 0:
                RoleText[2].text = "Draw";
                RoleText[2].color = Color.black;
                break;
            case -1:
                RoleText[2].text = "Your Loser";
                RoleText[2].color = Color.blue;
                break;
        }
        RoleText[1].text = enemy.Role.ToString();
        RolePanel[1].SetActive(true);

        yield return new WaitForSeconds(1);

        RolePanel[2].SetActive(true);

        if (EndFlagNum != 0)
        { PlayCount--; }

        StartCoroutine(NextGame());
    }

    IEnumerator NextGame()
    {
        Straight = false; Flash = false;
        StraighFlash = false; RoyalStraight = false;
        while (!Input.GetKeyDown(KeyCode.Space) && !Input.GetKeyDown(KeyCode.JoystickButton1))
        { yield return null; }
        for (int i = 0; i < 3; i++)
        { RolePanel[i].SetActive(false); }
        showZone.SetActive(false);
        player.CardReset();
        enemy.CardReset();
        pool.poolShuffle();
        showSpace.ShowCard.Clear();
        showSpace.ShowcardScripts.Clear();
        enemy.ShowSpaceReset();
        gamePhase = GamePhase.First;
        yield return new WaitForSeconds(0.5f);

        if (EndFlagNum == 1)
        {
            DoubleUpPlay = 1;
            StartCoroutine(DoubleUpSelect());
        }
        else if (PlayCount <= 0)
        {
            player.PlayerPoint += GetPoint;
            StartCoroutine(GameEnd());
        }
        else
        { StartCoroutine(PointSet()); }
    }
    public IEnumerator DoubleUpSelect()
    {
        player.PlayerPoint += GetPoint;
        playerObj.SetActive(true);
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

    Rank Valuation(int point)
    {
        Rank rank = Rank.Bad;

        if (point >= 3) { rank = Rank.Gold; }
        else if (point >= 2) { rank = Rank.Silver; }
        else if (point >= 1) { rank = Rank.Bronze; }
        Debug.Log(rank);
        return rank;
    }
    IEnumerator GameEnd()
    {
        Rank rank = Valuation(player.PlayerPoint);
        seve.SaveData(0,1,(int)rank,4);
        yield return new WaitForSeconds(0.01f);
        EndMessageBox.SetActive(true);
        EndText.text = "最終勝ち点は" + player.PlayerPoint + "です\n" +
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