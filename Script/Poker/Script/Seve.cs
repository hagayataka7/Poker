using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class Seve : MonoBehaviour
{
    public void Write(int Point)
    {
        StreamWriter sw = new StreamWriter("Point.txt", false); //true=追記 false=上書き
        sw.WriteLine(Point);
        sw.Flush();
        sw.Close();
    }
}
