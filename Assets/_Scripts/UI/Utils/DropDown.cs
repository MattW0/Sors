using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DropDown : MonoBehaviour
{
    [SerializeField] private GameOption option;
    [SerializeField] private TMP_Text numberText;
    [SerializeField] private string optionOne;
    [SerializeField] private string optionTwo;

    private void Start(){
        // get value from dropwdown component
        var startValue = GetComponent<TMP_Dropdown>().value;
        DropDownSelection(startValue);

        if(option == GameOption.NumberPlayers) SorsNetworkManager.SetNumberPlayers(startValue);
    }

    public void DropDownSelection(int index){
        switch (index){
            case 0:
                numberText.text = optionOne;
                break;
            case 1:
                numberText.text = optionTwo;
                break;
            default:
                Debug.Log("Unknown option");
                break;
        }
    }
}

public enum GameOption : byte
{
    NumberPlayers,
    NumberPhases,
    NetworkAddress,
}
