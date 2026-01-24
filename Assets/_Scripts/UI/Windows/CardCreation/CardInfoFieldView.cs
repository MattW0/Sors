using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class CardInfoFieldView : MonoBehaviour
{
    [SerializeField] protected string fieldId;
    public string FieldId => fieldId;
    [SerializeField] protected TMP_Text _fieldName;
    [SerializeField] protected TMP_Text _descriptionText;
    [SerializeField] protected CardCreator _creator;
}

public static class CardFieldId
{
    public const string Type = "type";
    public const string Title = "title";
    public const string Cost = "cost";
    public const string Health = "health";
    public const string Attack = "attack";
    public const string Points = "points";
    public const string MoneyValue = "moneyValue";
    public const string Traits = "traits";
    public const string Abilities = "abilities";
    public const string Description = "description";
    public const string FlavourText = "flavourText";
    public const string CardSprite = "cardSpritePath";
    public const string EntitySprite = "entitySpritePath";
}