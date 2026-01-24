using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class DropdownInput : CardInfoFieldView
{
    public enum DropdownField { Type, Trait }
    [SerializeField] private DropdownField dropdownField;
    [SerializeField] private TMP_Dropdown _dropdown;

    [Header("CardType Subset")]
    [SerializeField] private List<CardType> cardTypeOptions = new() {
        CardType.Creature,
        CardType.Technology,
        CardType.Money 
    };

    [Header("Trait Subset")]
    [SerializeField] private List<Trait> traitOptions = new() {
        Trait.Trample, 
        Trait.Deathtouch,
        Trait.Lifelink 
    };

    private List<Enum> _activeValues = new();

    private void Awake()
    {
        UpdateOptions();
        _dropdown.onValueChanged.AddListener(OnChanged);
    }

    private void UpdateOptions()
    {
        if (_dropdown == null) return;
        if (_fieldName != null) _fieldName.text = dropdownField.ToString();

        _dropdown.ClearOptions();
        _activeValues.Clear();
        
        switch (dropdownField)
        {
            case DropdownField.Type:
            _activeValues.AddRange(cardTypeOptions.Cast<Enum>());
            _dropdown.AddOptions(cardTypeOptions.Select(Nicify).ToList());
            break;


            case DropdownField.Trait:
            _activeValues.AddRange(traitOptions.Cast<Enum>());
            _dropdown.AddOptions(traitOptions.Select(Nicify).ToList());
            break;
        }
    }

    private void OnChanged(int index)
    {
        if (_creator == null) return;

        var value = _activeValues[index];
        switch (dropdownField)
        {
            case DropdownField.Type: 
                _creator.SetCardType((CardType)value);
                if (_descriptionText != null) _descriptionText.text = string.Empty;
                break;
            case DropdownField.Trait: 
                var trait = (Trait)value;
                _creator.card.traits = new List<Trait> { trait };
                if (_descriptionText != null) _descriptionText.text = trait.GetDescription();
                break;
        }
        _creator.NotifyChanged();
    }

    private static string Nicify<T>(T value) where T : Enum
    {
        return ObjectNames.NicifyVariableName(value.ToString());
    }

    private void OnDestroy()
    {
        if (_dropdown != null) _dropdown.onValueChanged.RemoveListener(OnChanged);
    }
    private void OnValidate() => UpdateOptions();
}
