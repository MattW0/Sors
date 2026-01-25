using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class CardCreator : MonoBehaviour
{
    public CardInfo card = new();
    [SerializeField] private DetailCardPreview _cardPreview;
    [SerializeField] private GameObject _options;
    [SerializeField] private Button _addAbility;
    [SerializeField] private Button _addTrait;
    [SerializeField] private GameObject _traitSelectorPrefab;
    [SerializeField] private Transform _traitsListParent;
    private readonly List<TraitItem> _traitItems = new();
    private CardInfoFieldView[] _inputs;

    void Awake()
    {
        _inputs = _options.GetComponentsInChildren<CardInfoFieldView>();
        _addAbility.onClick.AddListener(AddAbility);
        _addTrait.onClick.AddListener(AddTrait);

        card.type = CardType.Creature;
        ApplyCardTypeRules();
        NotifyChanged();
    }

    private void AddAbility()
    {
        throw new NotImplementedException();
    }

    private void AddTrait()
    {
        var item = Instantiate(_traitSelectorPrefab, _traitsListParent).GetComponent<TraitItem>();

        var remainingOptions = Configuration.TraitOptions.Except(card.traits).ToList();
        item.Initialize(this, remainingOptions);

        _traitItems.Add(item);
        SyncTraitsFromUI();
    }

    public void RemoveTrait(TraitItem item)
    {
        _traitItems.Remove(item);
        SyncTraitsFromUI();
    }

    private void SyncTraitsFromUI()
    {
        card.traits.Clear();

        foreach (var item in _traitItems)
            card.traits.Add(item.SelectedTrait);

        NotifyChanged();
    }

    public void OnTraitChanged()
    {
        SyncTraitsFromUI();
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