using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Player : MonoBehaviour
{
    [SerializeField]
    GameObject SeObj, cardPoolObj;
    AudioSource[] SE = new AudioSource[2];
    public int PlayerPoint = 10000;
    public List<GameObject> PlayerZone = new List<GameObject>();
    public List<GameObject> PlayerCard = new List<GameObject>();
    public List<CardScript> PlayerCardScript = new List<CardScript>();
    public IEnumerator SetUp;



    private void Start()
    { SE = SeObj.GetComponents<AudioSource>(); }
    /// <summary>カードを並び替えるアニメーション関数</summary>
    public IEnumerator CardSetUp()
    {
        SetUp = CardSetUp();
        for (int i = 0; i < PlayerZone.Count; i++)
        { PlayerCard[i].transform.DORotate(new Vector3(0f, 180f), 0.5f, RotateMode.FastBeyond360); }
        SE[1].PlayOneShot(SE[1].clip);
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < PlayerZone.Count; i++)
        { PlayerCard[i].transform.DOMove(gameObject.transform.position, 0.5f); }
        SE[0].PlayOneShot(SE[0].clip);
        yield return new WaitForSeconds(0.5f);
        PlayerSetUp();                                                                                            //カードを並び替える関数
        for (int i = 0; i < PlayerZone.Count; i++)
        { PlayerCard[i].transform.DOMove(PlayerZone[i].transform.position, 0.5f); }
        SE[0].PlayOneShot(SE[0].clip);
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < PlayerZone.Count; i++)
        { PlayerCardScript[i].transform.DORotate(new Vector3(0f, 0f), 0.5f, RotateMode.FastBeyond360); }
        SE[1].PlayOneShot(SE[1].clip);
        yield return new WaitForSeconds(0.5f);
        SetUp = null;
    }
    /// <summary>カードを並び替える関数</summary>
    void PlayerSetUp()
    {
        string[] str = new string[7];
        List<CardScript> CopyCardScripts = new List<CardScript>(PlayerCardScript); //手札のコピーを作成
        List<GameObject> CopyPlayerCard = new List<GameObject>(PlayerCard);        //手札の内容のコピーを作成
        for (int i = 0; i < CopyPlayerCard.Count; i++)
        { str[i] = PlayerCardScript[i].name; }
        Array.Sort(str);　　　                                                            //登録した名前をソート
        for (int i = 0; i < PlayerZone.Count; i++)
        {
            for (int j = 0; j < PlayerZone.Count; j++)
            {
                if (PlayerCard[j].name == str[i])                                  //コピー上でソートとした名前のインデックス順に手札を並び替える
                {
                    CopyCardScripts[i] = PlayerCardScript[j];
                    CopyPlayerCard[i] = PlayerCard[j];
                    PlayerCard[j].transform.SetParent(PlayerZone[i].transform);
                }
            }
        }
        PlayerCardScript = CopyCardScripts;　　　　　　　　　　　　　　　　　　　　//コピーをコピー元に移す
        PlayerCard = CopyPlayerCard;
    }
    public void CardReset()
    {
        for (int i = 0; i < PlayerZone.Count; i++)
        {
            PlayerCard[i].transform.DORotate(new Vector3(0f, 180f), 0.5f);
            PlayerCard[i].transform.SetParent(cardPoolObj.transform);
            PlayerCard[i].transform.DOMove(cardPoolObj.transform.position, 0.5f);
            PlayerCard[i] = null;
            PlayerCardScript[i] = null;
        }
    }
}
