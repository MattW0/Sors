using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PrevailOptionUI : MonoBehaviour
{
    private PrevailPanel _prevailPanel;
    [SerializeField] private TMP_Text optionDescription;
    [SerializeField] private TMP_Text numberSelectedText;
    [Header("Settings")]
    [SerializeField] private PrevailOption _option;
    [SerializeField] private string _placeHolderText;
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
        if (_timesSelected == 0){
            optionDescription.text = _placeHolderText;
            return;
        } 
        
        optionDescription.text = _option switch
        {
            PrevailOption.Trash => $"Trash up to {_timesSelected} card(s)",
            PrevailOption.CardSelection => $"Put {_timesSelected} card(s) from your discard into your hand",
            PrevailOption.Score => $"Score {_timesSelected} point(s) until end of turn",
            _ => "Error"
        };
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