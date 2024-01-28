using System;
using UnityEngine;
using TMPro;

public class InputField : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Text selection;
    [SerializeField] private GameOption option;
    public static event Action<string> OnNetworkAdressUpdate; 
    
    private void Start()
    {
        // inputField.text = (option == GameOption.NetworkAddress) ? "localhost" : "";
        inputField.onEndEdit.AddListener(delegate { UpdateTextBox(); });
        UpdateTextBox();
    }
 
    public void UpdateTextBox()
    {        
        var text = inputField.text.ToString();
        inputField.text = text;
        selection.text = text;

        if(option == GameOption.NetworkAddress) GameOptionsMenu.SetNetworkAddress(text);
        else if(option == GameOption.StateFile) GameOptionsMenu.SetStateFile(text);
    }
}
