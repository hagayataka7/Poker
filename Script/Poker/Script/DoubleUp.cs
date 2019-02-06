using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DoubleUp : MonoBehaviour
{
    [SerializeField]
    GameObject[] CardFrame = new GameObject[2];
    GameObject[] PlayCard = new GameObject[2];
    CardScript[] PlayScript = new CardScript[2];
    [SerializeField]
    AxlsInput GetAxlsInput;
    [SerializeField]
    CardPool pool;
    [SerializeField]
    GameManager manager;
    [SerializeField]
    GameObject SeObj, cardPoolObj, coin, SelectObj;
    AudioSource[] SE = new AudioSource[2];
    [SerializeField]
    GameObject[] DownEffect = new GameObject[3], DoubleUpButton = new GameObject[2];

    private void OnEnable()
    {
        SE = SeObj.GetComponents<AudioSource>();
    }
    public IEnumerator DoubleUpSet()
    {
        for (int i = 0; i < 2; i++)
        {
            GameObject Card = pool.CardList[0];
            pool.CardList.Remove(Card);                                       //山札から分離
            PlayCard[i] = Card;                                      //カードオブジェクトをプレイヤーに登録
            Card.transform.SetParent(PlayCard[i].transform);         //親子関係をセットCard = pool.CardList[0];
            PlayScript[i] = Card.GetComponent<CardScript>();
            SE[0].PlayOneShot(SE[0].clip);
            PlayCard[i].transform.DOMove(CardFrame[i].transform.position, 0.5f);
            yield return new WaitForSeconds(0.5f);
        }
        StartCoroutine(DoubleUpInpot());
    }
    IEnumerator DoubleUpInpot()
    {
        PlayCard[0].transform.DORotate(new Vector3(0f, 0), 0.5f, RotateMode.FastBeyond360);
        int Select = 0;
        for (; ; )
        {
            SelectObj.transform.position = DoubleUpButton[Select].transform.position;
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            { Select++; if (Select == 2) Select = 0; }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            { Select--; if (Select == -1) Select = 1; }
            if (Input.GetKeyDown(KeyCode.Space))
            { break; }
            yield return null;
        }
        StartCoroutine(DoubleUpMain(Select));
    }
    IEnumerator DoubleUpMain(int Select)
    {
        PlayCard[1].transform.DORotate(new Vector3(0f, 0f), 0.5f, RotateMode.FastBeyond360);
        yield return new WaitForSeconds(0.5f);
        if (PlayScript[0].CardNum != PlayScript[1].CardNum)
        {
            manager.DoubleUpPlay--;
            if (Select == 0)
            {
                manager.GetPoint =
                DoubleUpCheck(PlayScript[0].CardNum > PlayScript[1].CardNum, manager.GetPoint);
            }
            else if (Select == 1)
            {
                manager.GetPoint =
                 DoubleUpCheck(PlayScript[0].CardNum < PlayScript[1].CardNum, manager.GetPoint);
            }
        }
        StartCoroutine(DoubleUpEnd());
    }
    IEnumerator DoubleUpEnd()
    {
        yield return new WaitForSeconds(1f);
        for (int i = 0; i < 2; i++)
        {
            PlayCard[i].transform.DORotate(new Vector3(0f, 180f), 0.5f);
            PlayCard[i].transform.DOMove(cardPoolObj.transform.position, 0.5f);
            PlayScript[i] = null;
            PlayCard[i] = null;
        }
        SE[0].PlayOneShot(SE[0].clip);
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(manager.DoubleUpSelect());
    }
    int DoubleUpCheck(bool Check, int GetPoint)
    {
        if (Check)
        { coin.SetActive(true); return 1; }
        else
        {
            for (int i = 0; i < 3; i++)
            { DownEffect[i].SetActive(true); }
            return 0;
        }
    }
}
