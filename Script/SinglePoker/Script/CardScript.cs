using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardScript : MonoBehaviour
{
    public CardMark cardMark;
    public int CardNum = 0;
    [SerializeField]
    Sprite mainSprite;
    [SerializeField]
    Sprite reverseSprite;
    SpriteRenderer spriteRenderer;
    public bool ChangeFlag=false;
    [SerializeField] bool ReverseCheck = true;
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainSprite = spriteRenderer.sprite;
        ReverseChange(ReverseCheck);
        StartCoroutine(RotationCheck());
    }
    IEnumerator RotationCheck()
    {
        Vector3 Vec3 = Vector3.zero;
        int oldRote_y = 0, Rote_y = 0;
        for (;;)
        {
            Vec3 = transform.rotation.eulerAngles;
            Rote_y = Mathf.FloorToInt(Vec3.y / 10);
            if (Rote_y == 9 || Rote_y == 27)
            {
                if (oldRote_y != Rote_y)
                {
                    oldRote_y = Rote_y;
                    ReverseCheck = !ReverseCheck;
                    ReverseChange(ReverseCheck);
                }

            }
            if (Rote_y == 0 || Rote_y == 18)
                oldRote_y = 0;
            yield return null;
        }
    }
    void ReverseChange(bool check)
    {
        if (check)
            spriteRenderer.sprite = reverseSprite;
        else
            spriteRenderer.sprite = mainSprite;
    }
    public IEnumerator ReverseMove()
    {
        for (int i = 0; i < 180; i+=5)
        {
            transform.Rotate(0, 5, 0);
            yield return null;
        }
    }
}
