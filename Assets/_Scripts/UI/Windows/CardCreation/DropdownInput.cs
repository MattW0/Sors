using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class DropdownInput : CardInfoFieldView
{
    public enum DropdownField { Type }
    [SerializeField] private DropdownField dropdownField;
    [SerializeField] private TMP_Dropdown _dropdown;

    private void Awake()
    {
        UpdateOptions();
        _dropdown.onValueChanged.AddListener(OnChanged);
        _creator = GetComponentInParent<CardCreator>();
    }

    private void UpdateOptions()
    {
        if (_dropdown == null) return;
        if (_fieldName != null) _fieldName.text = dropdownField.ToString();

        _dropdown.ClearOptions();
        
        switch (dropdownField)
        {
            case DropdownField.Type:
            _dropdown.AddOptions(Configuration.CardTypeOptions.Select(Nicify).ToList());
            break;
        }
    }

    private void OnChanged(int index)
    {
        if (_creator == null) return;

        var value = Configuration.CardTypeOptions[index];
        switch (dropdownField)
        {
            case DropdownField.Type: 
                _creator.SetCardType(value);
                if (_descriptionText != null) _descriptionText.text = string.Empty;
                break;
        }
        _creator.NotifyChanged();
    }

    private void OnDestroy()
    {
        if (_dropdown != null) _dropdown.onValueChanged.RemoveListener(OnChanged);
    }
    private void OnValidate() => UpdateOptions();

    private static string Nicify<T>(T value) where T : Enum
    {
        return ObjectNames.NicifyVariableName(value.ToString());
    }
}
