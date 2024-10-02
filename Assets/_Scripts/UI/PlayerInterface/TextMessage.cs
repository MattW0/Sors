using System;
using UnityEngine;
using TMPro;

public class TextMessage : MonoBehaviour
{
    private TMP_Text _text;
    private void Awake() => _text = GetComponent<TMP_Text>();
    internal void SetMessage(string message) => _text.text = message;
}
