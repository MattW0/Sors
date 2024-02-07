using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Logger : MonoBehaviour
{
    [SerializeField] private TMP_Text logText;
    [SerializeField] private ScrollRect scrollRect; 

    public void Log(string message){
        logText.text += message + "\n";
        scrollRect.ScrollToBottom();
    }
}
