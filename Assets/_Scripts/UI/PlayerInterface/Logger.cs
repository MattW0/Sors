using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Logger : MonoBehaviour
{
    [SerializeField] private TMP_Text logText;

    public void Log(string message){
        logText.text += message + "\n";
    }
}
