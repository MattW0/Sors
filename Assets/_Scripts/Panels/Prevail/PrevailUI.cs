using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PrevailUI : AnimatedPanel
{
    [SerializeField] private TMP_Text instructions;
    [SerializeField] private Button _confirm;
    private PrevailPanel _panel;
    private bool _isOpen;
    
    void Start()
    {
        _panel = PrevailPanel.Instance;

        _confirm.onClick.AddListener(OnClickConfirm);
        _confirm.interactable = false;

        PrevailPanel.OnPrevailSelectionEnded += Close;
        PlayerInterfaceButtons.OnOpenPrevailPanel += ToggleOpen;
    }

    public void Begin(int numberOptions)
    {
        Open();
        _confirm.interactable = true;
        instructions.text = "Choose up to " + numberOptions.ToString();
    }

    private void OnClickConfirm()
    {
        _confirm.interactable = false;
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
