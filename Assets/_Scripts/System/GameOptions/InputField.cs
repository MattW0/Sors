using System;
using UnityEngine;
using TMPro;

public class InputField : MonoBehaviour
{
    [SerializeField] private TMP_Text selection;
    [SerializeField] private GameOption option;
    private GameOptionsMenu _options;
    private TMP_InputField _inputField;
    
    private void Start()
    {
        _options = GetComponentInParent<GameOptionsMenu>();

        _inputField = GetComponent<TMP_InputField>();
        _inputField.onEndEdit.AddListener(delegate { SetOption(); });
        SetOption();
    }
 
    public void SetOption()
    {        
        var text = _inputField.text.ToString();
        selection.text = text;

        _options.SetOption(option, text);
    }
}
