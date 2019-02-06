using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeIn : MonoBehaviour
{
    public Image Fade;
    private void OnEnable()
    {
        StartCoroutine(FadeController());
    }
    private void OnDisable()
    {
        Fade.color = new Color(Fade.color.r, Fade.color.g, Fade.color.b, 1);
    }
    IEnumerator FadeController()
    {
        float alpha = 1;
        for (; ; )
        {
            alpha -= 0.05f;
            Fade.color = new Color(Fade.color.r, Fade.color.g, Fade.color.b, alpha);
            if (alpha == 0)
                yield break;
            yield return null;
        }
    }


}
