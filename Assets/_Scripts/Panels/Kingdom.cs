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
    private GameManager _gameManager;

    [SerializeField] private KingdomUI _ui;
    [SerializeField] private KingdomCard[] kingdomCards;
    [SerializeField] private GameObject cardGrid;

    public static event Action OnRecruitPhaseEnded;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        _gameManager = GameManager.Instance;
    }

    [ClientRpc]
    public void RpcSetKingdomCards(CardInfo[] kingdomCardsInfo)
    {   
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
        _ui.BeginRecruit();        
    }

    public void PlayerPressedButton(CardInfo selectedCard)
    {
        PlayerManager.GetPlayerManager().PlayerRecruits(selectedCard);
    }

    [ClientRpc]
    public void RpcReplaceCard(string oldTitle, CardInfo cardInfo)
    {
        foreach (var kc in kingdomCards)
        {
            if (kc.cardInfo.title != oldTitle) continue;

            kc.SetCard(cardInfo);
            break;
        }
    }
    
    [TargetRpc]
    public void TargetResetRecruit(NetworkConnection target, int recruitsLeft)
    {
        if (recruitsLeft > 0) _ui.ResetRecruitButton();
    }
    
    [TargetRpc]
    public void TargetCheckRecruitability(NetworkConnection target, int playerCash){
        var skipCards = _ui.GetPreviouslySelectedKingdomCards();

        foreach (var kc in kingdomCards)
        {
            if (skipCards.Contains(kc)) continue;
            kc.Recruitable = playerCash >= kc.Cost;
        }
    }
    
    [ClientRpc]
    public void RpcEndRecruit()
    {
        OnRecruitPhaseEnded?.Invoke();
        _ui.EndRecruit();
    }

    public void MaxButton() => _ui.MaxButton();
}
