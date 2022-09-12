using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Kingdom : NetworkBehaviour
{
    public static Kingdom Instance { get; private set; }

    [SerializeField] private KingdomCard[] kingdomCards;
    [SerializeField] private GameObject cardGrid;

    public static event Action OnRecruitPhaseEnded;
    private CardInfo _selectedCard;
    
    // UI
    public Button confirm;
    [SerializeField] private GameObject minView, maxView;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        gameObject.transform.SetParent(GameObject.Find("UI").transform, false);
    }

    [ClientRpc]
    public void RpcSetKingdomCards(CardInfo[] kingdomCardsInfo)
    {   
        kingdomCards = new KingdomCard[kingdomCardsInfo.Length];
        kingdomCards = cardGrid.GetComponentsInChildren<KingdomCard>();

        for (int i = 0; i < kingdomCardsInfo.Length; i++)
        {
            kingdomCards[i].SetCard(kingdomCardsInfo[i]);
        }
    }
    
    [Server]
    public void ResetRecruit()
    {
        OnRecruitPhaseEnded?.Invoke();
    }

    public void CardToRecruitClicked(bool select, KingdomCard card){

        if (select) {
            confirm.interactable = true;
            _selectedCard = card.cardInfo;
        } else {
            confirm.interactable = false;
            _selectedCard.Destroy();
        }
    }

    public void ConfirmButtonPressed()
    {
        confirm.interactable = false;
        PlayerPressedButton();
        // if (p.Recruits == 0) MinButton();
    }

    public void SkipButtonPressed()
    {
        _selectedCard.Destroy();
        PlayerPressedButton();
    }

    private void PlayerPressedButton()
    {
        var networkIdentity = NetworkClient.connection.identity;
        var p = networkIdentity.GetComponent<PlayerManager>();
        p.CmdRecruitSelection(_selectedCard);
    }

    public void MinButton()
    {
        minView.SetActive(true);
        maxView.SetActive(false);
    }

    public void MaxButton()
    {
        minView.SetActive(false);
        maxView.SetActive(true);
    }
}
