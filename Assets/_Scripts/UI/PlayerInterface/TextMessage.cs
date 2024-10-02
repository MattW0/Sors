using System;
using UnityEngine;
using TMPro;

public class TextMessage : MonoBehaviour
{
    [SerializeField] private TMP_Text _originator;
    [SerializeField] private TMP_Text _time;
    [SerializeField] private TMP_Text _message;

    internal void SetMessage(Message m) 
    {
        _originator.text = m.originator;
        _time.text = m.time;
        _message.text = m.message;
    }
}

public struct Message
{
    public string originator;
    public string time;
    public string message;

    public Message(string o, string t, string m)
    {
        originator = o;
        time = t;
        message = m;
    }
}
