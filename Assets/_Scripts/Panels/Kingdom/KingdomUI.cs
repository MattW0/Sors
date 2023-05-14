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
    [SerializeField] private DetailCard previewCard;

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

    public void BeginPhase(Phase phase){
        MaxButton();
        skip.interactable = true;
        if(phase == Phase.Recruit) ShowRecruitPanel();
        else ShowDevelopPanel();
    }

    #region Selection
    public void SelectDevelopTile(DevelopTile tile){ 
        _kingdom.PlayerSelectsDevelopTile(tile);
        ShowPreviewCard(tile.cardInfo);
    }

    public void DeselectDevelopTile(DevelopTile tile){
        _kingdom.PlayerDeselectsDevelopTile(tile);
        HidePreviewCard();
    }

    public void SelectRecruitTile(RecruitTile tile){ 
        _kingdom.PlayerSelectsRecruitTile(tile);
        ShowPreviewCard(tile.cardInfo);
    }

    public void DeselectRecruitTile(RecruitTile tile){
        _kingdom.PlayerDeselectsRecruitTile(tile);
        HidePreviewCard();
    }

    private void ShowPreviewCard(CardInfo cardInfo){
        previewCard.SetCardUI(cardInfo);
        previewCard.gameObject.SetActive(true);
        confirm.interactable = true;
    }

    private void HidePreviewCard(){
        previewCard.gameObject.SetActive(false);
        confirm.interactable = false;
    }
    #endregion

    #region Buttons and UI
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

    public void ResetInteractionButtons(){
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
        HidePreviewCard();
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
    #endregion
}
