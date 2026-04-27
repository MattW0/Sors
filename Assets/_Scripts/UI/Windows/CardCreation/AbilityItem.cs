using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class AbilityItem : MonoBehaviour
{
    public Ability ability {
        get => new Ability();
    }
    [SerializeField] private TMP_Dropdown _triggerDropdown;
    [SerializeField] private TMP_Dropdown _effectDropdown;
    [SerializeField] private TMP_Dropdown _targetDropdown;
    [SerializeField] private Button _deleteButton;
    private CardSpecials _creator;
    private int _amount;
    private List<Trigger> _triggerOptions;
    private List<Effect> _effectOptions;
    private List<Target> _targetOptions;
    private Dictionary<TMP_Dropdown, List<Enum>> _inputs;

    public void Initialize(CardSpecials creator)
    {
        _creator = creator;

        foreach (var (dropdown, options) in _inputs) {
            dropdown.ClearOptions();
            dropdown.AddOptions(options.Select(Nicify).ToList());
            dropdown.onValueChanged.AddListener(_ => OnChanged());
        }

        _deleteButton.onClick.AddListener(OnDelete);
    }
    private void OnChanged() => _creator.SyncAbilities();
    private void OnDelete()
    {
        _creator.RemoveAbility(this);
        Destroy(gameObject);
    }

    private static string Nicify<T>(T value) where T : Enum
    {
        return ObjectNames.NicifyVariableName(value.ToString());
    }
}
