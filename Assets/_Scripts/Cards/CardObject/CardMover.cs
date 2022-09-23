using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardMover : MonoBehaviour
{
    [Header("Playboard Transforms")]
    // [SerializeField] private GameObject phaseSelectionPanel;
    [SerializeField] private Transform playerHand;
    [SerializeField] private Transform playerPlayZone;
    [SerializeField] private Transform playerMoneyZone;
    [SerializeField] private Transform playerDrawPile;
    [SerializeField] private Transform playerDiscardPile;
    [SerializeField] private Transform opponentHand;
    [SerializeField] private Transform opponentPlayZone;
    [SerializeField] private Transform opponentMoneyZone;
    [SerializeField] private Transform opponentDrawPile;
    [SerializeField] private Transform opponentDiscardPile;

    private CardUI _cardUI;
    private Transform _transform;

    private void Awake(){
        playerDrawPile = GameObject.Find("PlayerDrawPile").transform.GetChild(0);
        opponentDrawPile = GameObject.Find("OpponentDrawPile").transform.GetChild(0);
        playerHand = GameObject.Find("PlayerHand").transform;
        opponentHand = GameObject.Find("OpponentHand").transform;
        playerPlayZone = GameObject.Find("PlayerPlayZone").transform;
        opponentPlayZone = GameObject.Find("OpponentPlayZone").transform;
        playerMoneyZone = GameObject.Find("PlayerMoneyZone").transform.GetChild(1);
        opponentMoneyZone = GameObject.Find("OpponentMoneyZone").transform.GetChild(1);
        playerDiscardPile = GameObject.Find("PlayerDiscardPile").transform.GetChild(0);
        opponentDiscardPile = GameObject.Find("OpponentDiscardPile").transform.GetChild(0);

        _cardUI = gameObject.GetComponent<CardUI>();
    }

    public void MoveToDestination(bool hasAuthority, CardLocations destination)
    {
        _transform = gameObject.transform;
        
        switch (destination){
        default:
            print("<color=orange> Unknown card destination </color>");
            break;
        case CardLocations.Deck:
            if (hasAuthority) _transform.SetParent(playerDrawPile, false);
            else _transform.SetParent(opponentDrawPile, false);
            _cardUI.CardBackUp();
            break;
        case CardLocations.Hand:
            if (hasAuthority) {
                _transform.SetParent(playerHand, false);
                _cardUI.CardFrontUp();
            }
            else _transform.SetParent(opponentHand, false);
            break;
        case CardLocations.PlayZone:
            if (hasAuthority) _transform.SetParent(playerPlayZone, false);
            else {
                _transform.SetParent(opponentPlayZone, false);
                _cardUI.CardFrontUp();
            }
            break;
        case CardLocations.MoneyZone:
            if (hasAuthority) _transform.SetParent(playerMoneyZone, false);
            else {
                _transform.SetParent(opponentMoneyZone, false);
                _cardUI.CardFrontUp();
            }
            break;
        case CardLocations.Discard:
            if (hasAuthority) _transform.SetParent(playerDiscardPile, false);
            else _transform.SetParent(opponentDiscardPile, false);
            _cardUI.CardFrontUp();
            _transform.localPosition = Vector3.zero;
            break;
        }

        _cardUI.HighlightReset();
    }
}
