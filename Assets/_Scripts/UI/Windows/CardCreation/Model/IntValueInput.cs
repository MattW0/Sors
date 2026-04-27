using TMPro;
using UnityEngine;

[ExecuteAlways]
public class IntValueInput : CardInfoFieldView
{
    public enum IntField { Cost, Attack, Health, Points, MoneyValue }
    [SerializeField] private IntField numberField;
    [SerializeField] private TMP_InputField _input;

    private void Awake()
    {
        UpdateFieldName();
        _input.contentType = TMP_InputField.ContentType.IntegerNumber;

        if (_input != null)
        _input.onValueChanged.AddListener(OnChanged);
    }

    private void UpdateFieldName()
    {
        if (_fieldName != null)
        _fieldName.text = numberField.ToString();
    }

    [ExecuteInEditMode]
    private void OnChanged(string value)
    {
        if (_descriptionText != null) _descriptionText.text = value;
        if (!int.TryParse(value, out var v)) return;
        if (_creator == null) return;

        switch (numberField)
        {
            case IntField.Cost: _creator.card.cost = v; break;
            case IntField.Attack: _creator.card.attack = v; break;
            case IntField.Health: _creator.card.health = v; break;
            case IntField.Points: _creator.card.points = v; break;
            case IntField.MoneyValue: _creator.card.moneyValue = v; break;
        }
        _creator.NotifyChanged();
    }

    private void OnDestroy()
    {
        if (_input != null)
        _input.onValueChanged.RemoveListener(OnChanged);
    }
    private void OnValidate() => UpdateFieldName();
}