using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PrevailOptionUI : MonoBehaviour
{
    private PrevailPanel _prevailPanel;
    private PrevailOption _option;
    [SerializeField] private TMP_Text optionText;
    [SerializeField] private TMP_Text numberSelectedText;
    // [SerializeField] private Image outline;
    private int _timesSelected;


    private void Start()
    {
        // Only do this once
        if (_prevailPanel) return;

        _prevailPanel = PrevailPanel.Instance;
        _option = (PrevailOption) System.Enum.Parse(typeof(PrevailOption), gameObject.name);
    }

    public void OnClickDecrement()
    {
        if (_timesSelected <= 0) return;
        
        _timesSelected--;
        numberSelectedText.text = _timesSelected.ToString();
        _prevailPanel.Decrement(_option);
    }

    public void OnClickIncrement()
    {
        // can only increment if total _timesSelected < _nbOptionsToChose
        if(!_prevailPanel.Increment(_option)) return;
        
        _timesSelected++;
        numberSelectedText.text = _timesSelected.ToString();
    }

    public void Reset(){
        _timesSelected = 0;
    }
}