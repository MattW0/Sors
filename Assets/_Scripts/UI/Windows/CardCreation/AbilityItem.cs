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
    public bool IsCommitable => _ability.IsCommitable;

    [SerializeField] private TMP_Dropdown _triggerDropdown;
    [SerializeField] private TMP_Dropdown _effectDropdown;
    [SerializeField] private TMP_InputField _amountInput;
    [SerializeField] private TMP_Dropdown _targetDropdown;
    [SerializeField] private Button _deleteButton;
    private CardSpecials _creator;
    private List<Trigger> _triggers;
    private List<Effect> _effects;
    private List<Target> _targets;

    public void Initialize(CardSpecials creator)
    {
        _creator = creator;

        _triggers = Enum.GetValues(typeof(Trigger)).Cast<Trigger>().ToList();
        _effects = Enum.GetValues(typeof(Effect)).Cast<Effect>().ToList();
        _targets = Enum.GetValues(typeof(Target)).Cast<Target>().ToList();

        _triggerDropdown.AddOptions(_triggers.Select(Nicify).ToList());
        _triggerDropdown.onValueChanged.AddListener(OnTriggerChanged);

        _effectDropdown.AddOptions(_effects.Select(Nicify).ToList());
        _effectDropdown.onValueChanged.AddListener(OnEffectChanged);

        _targetDropdown.AddOptions(_targets.Select(Nicify).ToList());
        _targetDropdown.onValueChanged.AddListener(OnTargetChanged);

        _amountInput.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnTriggerChanged(int index)
    {
        _ability.trigger = _triggers[index];
        _creator.SyncAbilities();
    }

    private void OnEffectChanged(int index)
    {
        _ability.effect = _effects[index];
        _creator.SyncAbilities();
    }

    private void OnTargetChanged(int index)
    {
        _ability.target = _targets[index];
        _creator.SyncAbilities();
    }

    private void OnValueChanged(string value)
    {
        if (int.TryParse(value, out var v))  _ability.amount = v;
        _creator.SyncAbilities();
    }

    public void OnDelete()
    {
        _creator.RemoveAbility(this);
        Destroy(gameObject);
    }

    private static string Nicify<T>(T value) where T : Enum
    {
        return ObjectNames.NicifyVariableName(value.ToString());
    }
}
