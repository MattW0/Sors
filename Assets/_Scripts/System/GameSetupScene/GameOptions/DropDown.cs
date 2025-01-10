using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DropDown : MonoBehaviour
{
    [SerializeField] private GameOption option;
    private GameOptionsMenu _options;
    private TMP_Dropdown _dropdown;

    private void Start()
    {
        _options = GetComponentInParent<GameOptionsMenu>();

        _dropdown = GetComponent<TMP_Dropdown>();
        _dropdown.onValueChanged.AddListener(delegate { SetOption(); });

        // To get pre-set values from design time 
        SetOption();
    }

    // Value + 1 because indexed from 0
    public void SetOption() => _options.SetOption(option, _dropdown.value + 1);
}
