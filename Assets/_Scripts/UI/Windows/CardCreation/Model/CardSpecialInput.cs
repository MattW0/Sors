using UnityEngine;
using TMPro;
using UnityEngine.UI;


[ExecuteAlways]
public class CardSpeicalInput : CardInfoFieldView
{
    public enum SpecialField { Trait, Ability }

    private void Awake()
    {
        UpdateOptions();
        _creator = GetComponentInParent<CardCreator>();
    }

    private void UpdateOptions()
    {
        // print("Special Field, update options");
    }

    private void OnValidate() => UpdateOptions();
}
