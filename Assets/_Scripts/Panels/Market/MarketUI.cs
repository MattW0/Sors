using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MarketUI : MonoBehaviour
{
    [SerializeField] private Market _market;
    [SerializeField] private GameObject maxView;
    [SerializeField] private Image _bg;
    [SerializeField] private GameObject _developPanel;
    [SerializeField] private GameObject _recruitPanel;
    [SerializeField] private GameObject _interactionButtons;
    [SerializeField] private GameObject _waitingText;
    [SerializeField] private TMP_Text _switchBtnText;
    [SerializeField] private Button confirm;
    [SerializeField] private Button skip;
    [SerializeField] private GameObject _creatureDetailCard;
    [SerializeField] private GameObject _technologyDetailCard;
    [SerializeField] private GameObject _moneyDetailCard;
    
    private void Awake()
    {
        maxView.SetActive(false);
        _creatureDetailCard.SetActive(false);
        _technologyDetailCard.SetActive(false);
        _moneyDetailCard.SetActive(false);
    }

    private void Start()
    {
        _market = Market.Instance;
        skip.interactable = false;
        MinButton();
    }

    public void BeginPhase(Phase phase)
    {
        MaxButton();
        skip.interactable = true;
        if(phase == Phase.Recruit) ShowCreaturePanel();
        else ShowTechnologyPanel();
    }

    #region Selection

    public void SelectTile(CardInfo cardInfo){

        var previewCardObject = cardInfo.type switch{
            CardType.Creature => _creatureDetailCard,
            CardType.Technology => _technologyDetailCard,
            CardType.Money => _moneyDetailCard,
            _ => null
        };

        var detailCard = previewCardObject.GetComponent<DetailCard>();
        detailCard.SetCardUI(cardInfo);
        previewCardObject.SetActive(true);

        confirm.interactable = true;
    }

    public void DeselectTile(){
        _creatureDetailCard.SetActive(false);
        _technologyDetailCard.SetActive(false);
        _moneyDetailCard.SetActive(false);

        confirm.interactable = false;
    }
    #endregion

    #region Buttons and UI
    public void SwitchButtonPressed(){
        if(_developPanel.activeSelf) ShowCreaturePanel();
        else ShowTechnologyPanel();
    }
    
    public void ConfirmButtonPressed(){
        _interactionButtons.SetActive(false);
        _waitingText.SetActive(true);
        _market.PlayerPressedButton(false);
    }

    public void SkipButtonPressed(){
        _interactionButtons.SetActive(false);
        _waitingText.SetActive(true);
        _market.PlayerPressedButton(true);
    }

    public void ResetInteractionButtons(){
        _interactionButtons.SetActive(true);
        confirm.interactable = false;
        _waitingText.SetActive(false);
    }

    private void ShowTechnologyPanel(){
        _developPanel.SetActive(true);
        _recruitPanel.SetActive(false);
        _bg.color = SorsColors.technology;
        _switchBtnText.text = "Creatures";
    }

    private void ShowCreaturePanel(){
        _developPanel.SetActive(false);
        _recruitPanel.SetActive(true);
        _bg.color = SorsColors.creature;
        _switchBtnText.text = "Techs";
    }

    public void EndPhase(){
        MinButton();
        
        _creatureDetailCard.SetActive(false);
        _technologyDetailCard.SetActive(false);
        _moneyDetailCard.SetActive(false);

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