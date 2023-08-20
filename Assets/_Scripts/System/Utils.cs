using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils : MonoBehaviour
{
    public static string Capitalize(string str)
    {
        return char.ToUpper(str[0]) + str.Substring(1);
    }
}
