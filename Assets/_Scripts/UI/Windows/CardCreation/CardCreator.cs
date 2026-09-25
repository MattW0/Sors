using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(CardSpecials))]
public class CardCreator : MonoBehaviour
{
    public CardInfo card = new();
    public event Action<bool> CanCreateCardChanged;
    [SerializeField] private DetailCardPreview _cardPreview;
    [SerializeField] private GameObject _options;
    private CardInfoFieldView[] _inputs;
    private CardSpecials _specials;
    private HashSet<string> required = new();
    private Dictionary<string, CardInfoFieldView> _inputsById;

    void Awake()
    {
        _inputs = _options.GetComponentsInChildren<CardInfoFieldView>();
        _inputsById = _inputs.ToDictionary(x => x.FieldId);

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
        CanCreateCardChanged?.Invoke(HasRequiredFields());
    }

    private bool HasRequiredFields()
    {
        foreach (var fieldId in required)
        {
            if (!_inputsById.TryGetValue(fieldId, out var input))
            {
                Debug.LogWarning($"No input found for field '{fieldId}'.");
                return false;
            }

            if (!input.IsCommitable)
                return false;
        }

        Debug.Log($"Abilities valid: {_specials.AreAbilitiesValid}");

        return _specials.AreAbilitiesValid;
    }

    private void ApplyCardTypeRules()
    {
        required = Configuration.RequiredFields[card.type];

        var visible = Configuration.VisibleFields[card.type];
        foreach (var input in _inputs)
            input.gameObject.SetActive(visible.Contains(input.FieldId));
    }
}