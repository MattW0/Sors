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

        _prevailPanel = PrevailPanel.Instance;
        PrevailPanel.OnPrevailSelectionEnded += Reset;
    }

    public void OnClickDecrement()
    {
        if(_timesSelected == 0 || !_prevailPanel.Decrement(_option)) return;
        _timesSelected--;
        numberSelectedText.text = _timesSelected.ToString();
        UpdateDescriptionText();
    }

    public void OnClickIncrement()
    {
        // can only increment if total _timesSelected < _nbOptionsToChose
        if(!_prevailPanel.Increment(_option)) return;
        _timesSelected++;
        numberSelectedText.text = _timesSelected.ToString();

        UpdateDescriptionText();
    }

    private void UpdateDescriptionText()
    {
        if (_option == PrevailOption.Trash){
            if (_timesSelected == 0) optionDescription.text = "Trash up to X card(s)";
            else optionDescription.text = $"Trash up to {_timesSelected} card(s)";
        } else if (_option == PrevailOption.CardSelection){
            if (_timesSelected == 0) optionDescription.text = "Put X card(s) from your discard into your hand";
            else optionDescription.text = $"Put {_timesSelected} card(s) from your discard into your hand";
        } else if (_option == PrevailOption.Score){
            if (_timesSelected == 0) optionDescription.text = "Score X point(s) until end of turn";
            else optionDescription.text = $"Score {_timesSelected} point(s) until end of turn";
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