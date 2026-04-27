using TMPro;
using UnityEngine;

[ExecuteAlways]
public class TextInput : CardInfoFieldView
{
    public enum TextField { Title, FlavourText, Description }
    [SerializeField] private TextField textField;
    [SerializeField] private TMP_InputField _input;

    private void Awake()
    {
        UpdateFieldName();

        if (_input != null)
        _input.onValueChanged.AddListener(OnChanged);
    }

    private void UpdateFieldName()
    {
        if (_fieldName != null)
        _fieldName.text = textField.ToString();
    }

    private void OnChanged(string value)
    {
        if (_descriptionText != null) _descriptionText.text = value;
        if (_creator == null) return;

        switch (textField)
        {
            case TextField.Title: _creator.card.title = value; break;
            case TextField.FlavourText: _creator.card.flavourText = value; break;
            case TextField.Description: _creator.card.description = value; break;
        }
        _creator.NotifyChanged();
    }

    private void OnDestroy()
    {
        if (_input != null) _input.onValueChanged.RemoveListener(OnChanged);
    }
    private void OnValidate() => UpdateFieldName();
}
