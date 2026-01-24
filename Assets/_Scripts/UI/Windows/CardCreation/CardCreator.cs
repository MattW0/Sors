using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil.Cil;
using UnityEngine;

public class CardCreator : MonoBehaviour
{
    public CardInfo card = new();
    [SerializeField] private DetailCardPreview _cardPreview;
    [SerializeField] private GameObject _options;
    private CardInfoFieldView[] _inputs;

    void Awake()
    {
        _inputs = _options.GetComponentsInChildren<CardInfoFieldView>();

        card.type = CardType.Creature;
        ApplyCardTypeRules();
        NotifyChanged();
    }
    public void SetCardType(CardType type)
    {
        if (card.type == type) return;

        card.type = type;
        ApplyCardTypeRules();
        NotifyChanged();
    }
    public void NotifyChanged()
    {
        _cardPreview.ShowPreview(card, false);
    }

    private void ApplyCardTypeRules()
    {
        var visible = VisibleFields[card.type];
        foreach (var input in _inputs)
            input.gameObject.SetActive(visible.Contains(input.FieldId));
    }

    private static readonly Dictionary<CardType, HashSet<string>> VisibleFields = new() {
        {
            CardType.Creature, new() {
                CardFieldId.Type,
                CardFieldId.Title,
                CardFieldId.Cost,
                CardFieldId.Health,
                CardFieldId.Attack,
                CardFieldId.Traits,
                CardFieldId.Description,
                CardFieldId.FlavourText
            }
        },
        {
            CardType.Technology, new() {
                CardFieldId.Type,
                CardFieldId.Title,
                CardFieldId.Cost,
                CardFieldId.Health,
                CardFieldId.Points,
                CardFieldId.Description
                }
        },
        {
            CardType.Money, new(){
                CardFieldId.Type,
                CardFieldId.Title,
                CardFieldId.Cost,
                CardFieldId.MoneyValue,
                CardFieldId.FlavourText
            }
        }
    };
}