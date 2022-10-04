using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LogChatManager : MonoBehaviour
{
    [SerializeField] private GameObject logPanel;
    [SerializeField] private GameObject chatPanel;

    public void DisplayLog()
    {
        logPanel.SetActive(true);
        chatPanel.SetActive(false);
    }
    
    public void DisplayChat()
    {
        logPanel.SetActive(false);
        chatPanel.SetActive(true);
    }
}
