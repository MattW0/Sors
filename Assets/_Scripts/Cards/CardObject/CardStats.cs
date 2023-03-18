using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class CardStats : NetworkBehaviour
{
    public PlayerManager owner;
    public CardInfo cardInfo;
    private CardUI _cardUI;

    private bool _interactable;
    public bool IsInteractable { 
        get => _interactable;
        set {
            _interactable = value;
            _cardUI.Highlight(value, Color.green);
        }
    }

    private bool _discardable;
    public bool IsDiscardable { 
        get => _discardable;
        set {
            _discardable = value;
            _cardUI.Highlight(value, Color.red);
        }
    }

    private bool _trashable;
    public bool IsTrashable { 
        get => _trashable;
        set {
            _trashable = value;
            _cardUI.Highlight(value, Color.red);
        }
    }
    
    private bool _deployable;
    public bool IsDeployable
    {
        get => _deployable;
        set
        {
            if (!cardInfo.isCreature) return;
            
            _deployable = value;
            _cardUI.Highlight(value, Color.cyan);
        }
    }
    
    private void Awake()
    {
        var networkIdentity = NetworkClient.connection.identity;
        owner = networkIdentity.GetComponent<PlayerManager>();
        
        _cardUI = gameObject.GetComponent<CardUI>();
    }

    [ClientRpc]
    public void RpcSetCardStats(CardInfo card)
    {
        cardInfo = card;
        _cardUI.SetCardUI(card);
    }
}
