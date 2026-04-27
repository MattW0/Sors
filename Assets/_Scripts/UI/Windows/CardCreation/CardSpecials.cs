using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class CardSpecials : MonoBehaviour
{
    [SerializeField] private Button _addAbility;
    [SerializeField] private Button _addTrait;
    [SerializeField] private GameObject _abilityItemPrefab;
    [SerializeField] private Transform _abilitiesListParent;
    [SerializeField] private GameObject _traitItemPrefab;
    [SerializeField] private Transform _traitsListParent;
    private readonly List<AbilityItem> _abilityItems = new();
    private readonly List<TraitItem> _traitItems = new();
    private CardCreator _creator;

    public void Configure(CardCreator creator)
    {
        _creator = creator;
        _addAbility.onClick.AddListener(AddAbility);
        _addTrait.onClick.AddListener(AddTrait);
    }

    private void AddAbility()
    {
        var item = Instantiate(_abilityItemPrefab, _abilitiesListParent).GetComponent<AbilityItem>();
        item.Initialize(this);

        _abilityItems.Add(item);
        SyncAbilities();
    }

    private void AddTrait()
    {
        var item = Instantiate(_traitItemPrefab, _traitsListParent).GetComponent<TraitItem>();

        var remainingOptions = Configuration.TraitOptions.Except(_creator.card.traits).ToList();
        item.Initialize(this, remainingOptions);

        _traitItems.Add(item);
        SyncTraits();
    }

    public void SyncTraits()
    {
        _creator.card.traits.Clear();

        foreach (var item in _traitItems)
            _creator.card.traits.Add(item.SelectedTrait);

        _creator.NotifyChanged();
    }

    public void SyncAbilities()
    {
        _creator.card.abilities.Clear();

        foreach (var item in _abilityItems)
            _creator.card.abilities.Add(item.ability);

        _creator.NotifyChanged();
    }

    public void RemoveTrait(TraitItem item)
    {
        _traitItems.Remove(item);
        SyncTraits();
    }

    public void RemoveAbility(AbilityItem item)
    {
        _abilityItems.Remove(item);
        SyncTraits();
    }
}
