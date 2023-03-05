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
        MinButton();
    }

    #region Recruit
    public void BeginPhase(Phase phase){
        MaxButton();
        if(phase == Phase.Recruit) ShowRecruitPanel();
        else ShowDevelopPanel();
        skip.interactable = true;
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

    #region Develop

    public void SelectDevelopCard(DevelopTile tile){ 
        confirm.interactable = true;
        _kingdom.selection = tile.cardInfo;
    }
    public void DeselectDevelopCard(DevelopTile tile){
        confirm.interactable = false;
        _kingdom.selection.title = null; // hack-around for check in TurnManager
    }
    public void EndDevelop(){
        CloseWindow();
    }
    #endregion

    public void SwitchButtonPressed(){
        if(_developPanel.activeSelf) ShowRecruitPanel();
        else ShowDevelopPanel();
    }
    
    public void ConfirmButtonPressed(){
        confirm.interactable = false;
        _kingdom.PlayerPressedButton(false);
    }

    public void SkipButtonPressed(){
        skip.interactable = false;
        _kingdom.PlayerPressedButton(true);
    }

    public void ResetRecruitButton(){
        skip.interactable = true;
    }

    private void ShowDevelopPanel(){
        _developPanel.SetActive(true);
        _recruitPanel.SetActive(false);
        _switchBtnText.text = "Develop";
    }

    private void ShowRecruitPanel(){
        _developPanel.SetActive(false);
        _recruitPanel.SetActive(true);
        _switchBtnText.text = "Recruit";
    }

    public void CloseWindow(){
        MinButton();
        skip.interactable = true;
        confirm.interactable = false;
    }

    public void MaxButton() {
        if (maxView.activeSelf) MinButton();
        else maxView.SetActive(true);
    }
    public void MinButton() => maxView.SetActive(false);
}
