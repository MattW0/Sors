using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KingdomUI : MonoBehaviour
{

    public static KingdomUI Instance { get; private set; }
    [SerializeField] private Kingdom _kingdom;
    
    // UI
    [SerializeField] private GameObject maxView;
    [SerializeField] private GameObject _developPanel;
    [SerializeField] private GameObject _recruitPanel;
    [SerializeField] private GameObject _interactionButtons;
    [SerializeField] private GameObject _waitingText;
    [SerializeField] private TMP_Text _switchBtnText;
    [SerializeField] private Button confirm;
    [SerializeField] private Button skip;

    private void Awake()
    {
        if (!Instance) Instance = this;
    }

    private void Start()
    {
        _kingdom = Kingdom.Instance;
        skip.interactable = false;
        MinButton();
    }

    #region Develop
    public void SelectDevelopCard(DevelopTile tile){ 
        confirm.interactable = true;
        _kingdom.developSelection.Add(tile.cardInfo);
    }
    public void DeselectDevelopCard(DevelopTile tile){
        _kingdom.developSelection.Remove(tile.cardInfo);
        if(_kingdom.developSelection.Count == 0) confirm.interactable = false;
    }
    #endregion

    #region Recruit
    public void BeginPhase(Phase phase){
        MaxButton();
        skip.interactable = true;
        if(phase == Phase.Recruit) ShowRecruitPanel();
        else ShowDevelopPanel();
    }

    public void SelectRecruitCard(RecruitTile tile){ 
        confirm.interactable = true;
        _kingdom.PlayerSelectsRecruitTile(tile);
    }
    public void DeselectRecruitCard(RecruitTile tile){
        confirm.interactable = false;
        _kingdom.PlayerDeselectsRecruitTile(tile);
    }
    #endregion

    public void SwitchButtonPressed(){
        if(_developPanel.activeSelf) ShowRecruitPanel();
        else ShowDevelopPanel();
    }
    
    public void ConfirmButtonPressed(){
        _interactionButtons.SetActive(false);
        _waitingText.SetActive(true);
        _kingdom.PlayerPressedButton(false);
    }

    public void SkipButtonPressed(){
        _interactionButtons.SetActive(false);
        _waitingText.SetActive(true);
        _kingdom.PlayerPressedButton(true);
    }

    public void ResetRecruitButton(){
        _interactionButtons.SetActive(true);
        confirm.interactable = false;
        _waitingText.SetActive(false);
    }

    private void ShowDevelopPanel(){
        _developPanel.SetActive(true);
        _recruitPanel.SetActive(false);
        _switchBtnText.text = "Recruit";
    }

    private void ShowRecruitPanel(){
        _developPanel.SetActive(false);
        _recruitPanel.SetActive(true);
        _switchBtnText.text = "Develop";
    }

    public void EndPhase(){
        MinButton();
        _interactionButtons.SetActive(true);
        _waitingText.SetActive(false);
        confirm.interactable = false;
        skip.interactable = false;
    }

    public void MaxButton() {
        if (maxView.activeSelf) MinButton();
        else maxView.SetActive(true);
    }
    public void MinButton() => maxView.SetActive(false);
}
