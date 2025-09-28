using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror.BouncyCastle.Crypto.Generators;

public class PrevailUI : AnimatedPanel
{
    [SerializeField] private TMP_Text instructions;
    [SerializeField] private Button confirm;
    private PrevailPanel _panel;
    private bool _isOpen;
    
    void Start()
    {
        confirm.onClick.AddListener(OnClickConfirm);
        _panel = PrevailPanel.Instance;

        PrevailPanel.OnPrevailSelectionEnded += Close;
        PlayerInterfaceButtons.OnOpenPrevailPanel += ToggleOpen;
    }

    public void Begin(int numberOptions)
    {
        Open();
        confirm.interactable = true;
        instructions.text = "Choose up to " + numberOptions.ToString();
    }

    private void OnClickConfirm()
    {
        confirm.interactable = false;
        _panel.ConfirmButonClicked();
    }

    private void ToggleOpen()
    {
        _isOpen = !_isOpen;
        if(_isOpen) PanelIn();
        else PanelOut();
    }

    private void Open()
    {
        PanelIn();
        _isOpen = true;
    }

    private void Close()
    {
        PanelOut();
        _isOpen = false;
    }

    private void OnDestroy()
    {
        PrevailPanel.OnPrevailSelectionEnded -= Close;
        PlayerInterfaceButtons.OnOpenPrevailPanel -= ToggleOpen;
    }
}
