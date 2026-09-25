using UnityEngine;
using TMPro;
using UnityEngine.UI;


[ExecuteAlways]
public class CardSpeicalInput : CardInfoFieldView
{
    public enum SpecialField { Trait, Ability }
    public override bool IsCommitable => true;   

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
