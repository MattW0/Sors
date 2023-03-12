using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PrevailOptionUI : MonoBehaviour
{
    private PrevailPanel _prevailPanel;
    [SerializeField] private PrevailOption _option;
    [SerializeField] private TMP_Text optionTitle;
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
        PrevailPanel.OnPrevailEnded += Reset;
    }

    public void OnClickDecrement(){
        if (_timesSelected <= 0) return;
        
        _timesSelected--;
        _prevailPanel.Decrement(_option);

        numberSelectedText.text = _timesSelected.ToString();
        if (_timesSelected == 0) {
            optionDescription.enabled = false;
        }
    }

    public void OnClickIncrement(){
        // can only increment if total _timesSelected < _nbOptionsToChose
        if(!_prevailPanel.Increment(_option)) return;
        
        _timesSelected++;
        numberSelectedText.text = _timesSelected.ToString();
        optionDescription.enabled = true;
    }

    public void Reset(){
        _timesSelected = 0;
        numberSelectedText.text = "0";
    }

    private void OnDestroy(){
        PrevailPanel.OnPrevailEnded -= Reset;
    }
}