using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KingdomUI : MonoBehaviour
{

    public static KingdomUI Instance { get; private set; }
    [SerializeField] private Kingdom _kingdom;
    private KingdomCard _selectedCard;
    private List<KingdomCard> _previouslySelected = new ();
    public List<KingdomCard> GetPreviouslySelectedKingdomCards() => _previouslySelected;

    // UI
    public Button confirm;
    public Button skip;
    [SerializeField] private GameObject maxView;

    private void Awake()
    {
        if (!Instance) Instance = this;
    }

    public void BeginRecruit(){
        MaxButton();
        skip.interactable = true;
    }

    public void SelectCard(KingdomCard card){ 
        confirm.interactable = true;
        _selectedCard = card;
    }
    public void DeselectCard(KingdomCard card){
        confirm.interactable = false;
        _selectedCard = null;
    }

    public void ConfirmButtonPressed()
    {
        confirm.interactable = false;
        _previouslySelected.Add(_selectedCard);
        _kingdom.PlayerPressedButton(_selectedCard.cardInfo);
    }

    public void SkipButtonPressed()
    {
        skip.interactable = false;
        _selectedCard = null;
        _kingdom.PlayerPressedButton(new CardInfo());
    }

    public void EndRecruit(){
        MinButton();
        _selectedCard = null;
        _previouslySelected.Clear();

        skip.interactable = true;
        confirm.interactable = false;
    }

    public void ResetRecruitButton(){
        skip.interactable = true;
    }

    public void MaxButton() {
        if (maxView.activeSelf) MinButton();
        else maxView.SetActive(true);
    }
    private void MinButton() => maxView.SetActive(false);
}
