using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CardSpecials))]
public class CardCreator : MonoBehaviour
{
    public CardInfo card = new();
    [SerializeField] private DetailCardPreview _cardPreview;
    [SerializeField] private GameObject _options;
    private CardInfoFieldView[] _inputs;
    private CardSpecials _specials;

    void Awake()
    {
        _inputs = _options.GetComponentsInChildren<CardInfoFieldView>();
        _specials = GetComponent<CardSpecials>();
        _specials.Configure(this);

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
        var visible = Configuration.VisibleFields[card.type];
        foreach (var input in _inputs)
            input.gameObject.SetActive(visible.Contains(input.FieldId));
    }
}