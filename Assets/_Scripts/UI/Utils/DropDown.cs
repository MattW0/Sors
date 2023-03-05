using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DropDown : MonoBehaviour
{
    [SerializeField] private TMP_Text numberText;
    [SerializeField] private string optionOne;
    [SerializeField] private string optionTwo;


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
