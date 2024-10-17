using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PrevailUI : AnimatedPanel
{
    [SerializeField] private TMP_Text instructions;
    [SerializeField] private Button confirm;
    private PrevailPanel _panel;
    
    void Start()
    {
        confirm.onClick.AddListener(OnClickConfirm);
        _panel = PrevailPanel.Instance;

        PrevailPanel.OnPrevailSelectionEnded += PanelOut;
    }

    public void Begin(int numberOptions)
    {
        PanelIn();
        confirm.interactable = true;
        instructions.text = "Choose up to " + numberOptions.ToString();
    }

    private void OnClickConfirm()
    {
        confirm.interactable = false;
        _panel.ConfirmButonClicked();
    }

    private void OnDestroy()
    {
        PrevailPanel.OnPrevailSelectionEnded -= PanelOut;
    }
}
