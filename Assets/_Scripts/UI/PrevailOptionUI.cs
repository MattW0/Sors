using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PrevailOptionUI : MonoBehaviour
{
    private PrevailPanel _prevailPanel;
    [SerializeField] private PrevailOption _option;
    [SerializeField] private TMP_Text optionDescription;
    [SerializeField] private TMP_Text numberSelectedText;
    // [SerializeField] private Image outline;
    private int _timesSelected;

    private void Start()
    {
        // Only do this once
        if (_prevailPanel) return;

        optionDescription.enabled = false;

        _prevailPanel = PrevailPanel.Instance;
        PrevailPanel.OnPrevailSelectionEnded += Reset;
    }

    public void OnClickDecrement()
    {
        if(_timesSelected == 0 || !_prevailPanel.Decrement(_option)) return;
        _timesSelected--;
        numberSelectedText.text = _timesSelected.ToString();

        if (_timesSelected == 0) optionDescription.enabled = false;
        else UpdateDescriptionText();
    }

    public void OnClickIncrement()
    {
        // can only increment if total _timesSelected < _nbOptionsToChose
        if(!_prevailPanel.Increment(_option)) return;
        _timesSelected++;
        numberSelectedText.text = _timesSelected.ToString();

        optionDescription.enabled = true;
        UpdateDescriptionText();
    }

    private void UpdateDescriptionText()
    {
        if (_option == PrevailOption.Trash) 
            optionDescription.text = $"Trash up to {_timesSelected} card(s)";
        else if (_option == PrevailOption.CardSelection) 
            optionDescription.text = $"Put {_timesSelected} card(s) from your discard into your hand";
        else if (_option == PrevailOption.Score){
            // TODO:
            optionDescription.text = $"This does nothing {_timesSelected} time(s)";
        }
    }

    public void Reset()
    {
        _timesSelected = 0;
        numberSelectedText.text = "0";
    }

    private void OnDestroy()
    {
        PrevailPanel.OnPrevailSelectionEnded -= Reset;
    }
}