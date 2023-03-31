using System;
using UnityEngine;
using TMPro;

public class InputField : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Text selection;
    public static event Action<string> OnInputFieldChanged;
    
    private void Start(){
        inputField.text = "";
        inputField.onEndEdit.AddListener(delegate { UpdateTextBox(inputField); });
    }
 
    public void UpdateTextBox(TMP_InputField textbox)
    {
        if (textbox.text.Length < 1) return;
        
        var text = textbox.text.ToString();
        textbox.text = text;
        selection.text = text;
        OnInputFieldChanged?.Invoke(text);
    }
}
