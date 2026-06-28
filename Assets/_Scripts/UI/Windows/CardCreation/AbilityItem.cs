using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class AbilityItem : MonoBehaviour
{
    private Ability _ability = new();
    public Ability Ability {
        get => _ability;
    }

    [SerializeField] private TMP_Dropdown _triggerDropdown;
    [SerializeField] private TMP_Dropdown _effectDropdown;
    [SerializeField] private TMP_InputField _amountInput;
    [SerializeField] private TMP_Dropdown _targetDropdown;
    [SerializeField] private Button _deleteButton;
    private CardSpecials _creator;
    private Dictionary<TMP_Dropdown, IEnumerable<Enum>> _inputs;

    public void Initialize(CardSpecials creator)
    {
        _creator = creator;

        _triggerDropdown.AddOptions(Enum.GetValues(typeof(Trigger)).Cast<Trigger>().Select(Nicify).ToList());
        _triggerDropdown.onValueChanged.AddListener(_ => OnTriggerChanged());

        _effectDropdown.AddOptions(Enum.GetValues(typeof(Effect)).Cast<Effect>().Select(Nicify).ToList());
        _effectDropdown.onValueChanged.AddListener(_ => OnEffectChanged());

        _targetDropdown.AddOptions(Enum.GetValues(typeof(Target)).Cast<Target>().Select(Nicify).ToList());
        _targetDropdown.onValueChanged.AddListener(_ => OnTargetChanged());

        _amountInput.onValueChanged.AddListener(OnValueChanged);
        _deleteButton.onClick.AddListener(OnDelete);
    }

    private void OnTriggerChanged()
    {
        _ability.trigger = (Trigger) _triggerDropdown.value;
        _creator.SyncAbilities();
    }

    private void OnEffectChanged()
    {
        _ability.effect = (Effect) _effectDropdown.value;
        _creator.SyncAbilities();
    }

    private void OnTargetChanged()
    {
        _ability.target = (Target) _targetDropdown.value;
        _creator.SyncAbilities();
    }

    private void OnValueChanged(string value)
    {
        if (_amountInput != null) _amountInput.text = value;
        if (!int.TryParse(value, out var v)) return;

        _ability.amount = v;
        _creator.SyncAbilities();
    }

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
