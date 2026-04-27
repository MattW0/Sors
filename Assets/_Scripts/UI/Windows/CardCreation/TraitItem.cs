using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class TraitItem : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown _dropdown;
    [SerializeField] private Button _deleteButton;
    private CardSpecials _creator;
    private List<Trait> _options;
    public Trait SelectedTrait => _options[_dropdown.value];

    public void Initialize(CardSpecials creator, List<Trait> remainingOptions)
    {
        _creator = creator;
        _options = remainingOptions;

        _dropdown.ClearOptions();
        _dropdown.AddOptions(_options.Select(Nicify).ToList());

        _dropdown.onValueChanged.AddListener(_ => OnChanged());
        _deleteButton.onClick.AddListener(OnDelete);
    }
    private void OnChanged() => _creator.SyncTraits();
    private void OnDelete()
    {
        _creator.RemoveTrait(this);
        Destroy(gameObject);
    }

    private static string Nicify<T>(T value) where T : Enum
    {
        return ObjectNames.NicifyVariableName(value.ToString());
    }
}
