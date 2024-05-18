using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MarketUI : MonoBehaviour
{
    private Market _market;
    private InteractionPanel _interactionPanel;
    [SerializeField] private GameObject maxView;
    [SerializeField] private GameObject _developPanel;
    [SerializeField] private GameObject _recruitPanel;
    [SerializeField] private TMP_Text _switchBtnText;
    
    private void Awake()
    {
        maxView.SetActive(false);
    }

    private void Start()
    {
        _market = Market.Instance;
        MinButton();
    }

    public void BeginPhase(Phase phase)
    {
        MaxButton();
        if(phase == Phase.Recruit) ShowCreaturePanel();
        else ShowTechnologyPanel();
    }

    #region Buttons and UI

    public void SwitchButtonPressed(){
        if(_developPanel.activeSelf) ShowCreaturePanel();
        else ShowTechnologyPanel();
    }

    private void ShowTechnologyPanel(){
        _developPanel.SetActive(true);
        _recruitPanel.SetActive(false);
        _switchBtnText.text = "Creatures";
    }

    private void ShowCreaturePanel(){
        _developPanel.SetActive(false);
        _recruitPanel.SetActive(true);
        _switchBtnText.text = "Technologies";
    }

    public void MaxButton() {
        if (maxView.activeSelf) MinButton();
        else maxView.SetActive(true);
    }
    public void MinButton() => maxView.SetActive(false);
    #endregion
}
