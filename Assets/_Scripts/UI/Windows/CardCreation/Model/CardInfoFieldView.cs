using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class CardInfoFieldView : MonoBehaviour
{
    [SerializeField] protected string fieldId;
    public string FieldId => fieldId;
    [SerializeField] protected TMP_Text _fieldName;
    [SerializeField] protected TMP_Text _descriptionText;
    [SerializeField] protected CardCreator _creator;
    public abstract bool IsCommitable { get; }
}