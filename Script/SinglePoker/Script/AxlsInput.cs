using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxlsInput : MonoBehaviour
{
    [HideInInspector]
    public bool X_Plus = false, X_Minus = false;
    [HideInInspector]
    public bool Y_Plus = false, Y_Minus = false;
    int X_Count = 0, Y_Count = 0;
    [SerializeField]
    int Frame = 60;
    private void Start()
    { StartCoroutine(InputStart()); }
    IEnumerator InputStart()
    {
        for (;;)
        {
            int x = (int)Input.GetAxisRaw(InputString.CrossButton_X);
            int y = (int)Input.GetAxisRaw(InputString.CrossButton_Y);

            switch (x)
            {
                case 1:
                    if (X_Count >= Frame) { X_Count = 0; }
                    X_Plus = X_Count == 0;
                    X_Count++; break;
                case -1:
                    if (X_Count <= -Frame) { X_Count = 0; }
                    X_Minus = X_Count == 0;
                    X_Count--; break;
                case 0:
                    X_Count = 0;
                    X_Plus = false; X_Minus = false;
                    break;
            }
            switch (y)
            {
                case 1:
                    if (Y_Count >= Frame) { Y_Count = 0; }
                    Y_Plus = Y_Count == 0;
                    Y_Count++; break;
                case -1:
                    if (Y_Count <= -Frame) { Y_Count = 0; }
                    Y_Minus = Y_Count == 0;
                    Y_Count--; break;
                case 0:
                    Y_Count = 0;
                    Y_Plus = false; Y_Minus = false;
                    break;
            }
            
            yield return null;
        }
    }
}
