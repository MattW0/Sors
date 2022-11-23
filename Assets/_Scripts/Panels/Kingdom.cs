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
    public Button skip;
    [SerializeField] private GameObject maxView;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    [ClientRpc]
    public void RpcSetKingdomCards(CardInfo[] kingdomCardsInfo)
    {   
        maxView.SetActive(false);

        kingdomCards = new KingdomCard[kingdomCardsInfo.Length];
        kingdomCards = cardGrid.GetComponentsInChildren<KingdomCard>();

        for (var i = 0; i < kingdomCardsInfo.Length; i++)
        {
            kingdomCards[i].SetCard(kingdomCardsInfo[i]);
        }
    }

    [ClientRpc]
    public void RpcBeginRecruit()
    {
        MaxButton();
        skip.interactable = true;
    }
    
    [TargetRpc]
    public void TargetResetRecruit(NetworkConnection target, int recruitsLeft)
    {
        if (recruitsLeft > 0) skip.interactable = true;
    }
    
    [TargetRpc]
    public void TargetCheckRecruitability(NetworkConnection target, int currentCash){
        foreach (var kc in kingdomCards)
        {
            kc.Recruitable = currentCash >= kc.Cost;
        }
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
    }

    public void SkipButtonPressed()
    {
        skip.interactable = false;
        _selectedCard.Destroy();
        PlayerPressedButton();
    }

    private void PlayerPressedButton()
    {
        var networkIdentity = NetworkClient.connection.identity;
        var p = networkIdentity.GetComponent<PlayerManager>();
        
        p.CmdRecruitSelection(_selectedCard);
    }
    
    [Server]
    public void EndRecruit()
    {
        OnRecruitPhaseEnded?.Invoke();
        RpcMinButton();
    }

    public void MaxButton() {
        if (maxView.activeSelf) MinButton();
        else maxView.SetActive(true);
    }
    public void MinButton() => maxView.SetActive(false);

    [ClientRpc]
    private void RpcMinButton() => MinButton();
}
