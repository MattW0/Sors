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

    private void Awake(){
        playerDrawPile = GameObject.Find("PlayerDrawPile").transform.GetChild(0);
        opponentDrawPile = GameObject.Find("OpponentDrawPile").transform.GetChild(0);
        playerHand = GameObject.Find("PlayerHand").transform;
        opponentHand = GameObject.Find("OpponentHand").transform;
        playerPlayZone = GameObject.Find("PlayerPlayZone").transform;
        opponentPlayZone = GameObject.Find("OpponentPlayZone").transform;
        playerMoneyZone = GameObject.Find("PlayerMoneyZone").transform;
        opponentMoneyZone = GameObject.Find("OpponentMoneyZone").transform;
        playerDiscardPile = GameObject.Find("PlayerDiscardPile").transform.GetChild(0);
        opponentDiscardPile = GameObject.Find("OpponentDiscardPile").transform.GetChild(0);

        _cardUI = gameObject.GetComponent<CardUI>();
    }

    public void MoveToDestination(bool hasAuthority, CardLocations destination){
        switch (destination){
        default:
            print("<color=orange> Unknown card destination </color>");
            break;
        case CardLocations.Deck:
            if (hasAuthority) gameObject.transform.SetParent(playerDrawPile, false);
            else gameObject.transform.SetParent(opponentDrawPile, false);

            gameObject.GetComponent<CardUI>().CardBackUp();
            break;
        case CardLocations.Hand:
            if (hasAuthority) {
                gameObject.transform.SetParent(playerHand, false);
                gameObject.GetComponent<CardUI>().CardFrontUp();
                gameObject.GetComponent<DragDrop>().ChangeDragPermission(true);
            }
            else gameObject.transform.SetParent(opponentHand, false);
            break;
        case CardLocations.PlayZone:
            if (hasAuthority) gameObject.transform.SetParent(playerPlayZone, false);
            else {
                gameObject.transform.SetParent(opponentPlayZone, false);
                gameObject.GetComponent<CardUI>().CardFrontUp();
            }
            break;
        case CardLocations.MoneyZone:
            if (hasAuthority) gameObject.transform.SetParent(playerMoneyZone, false);
            else {
                gameObject.transform.SetParent(opponentMoneyZone, false);
                gameObject.GetComponent<CardUI>().CardFrontUp();
            }
            break;
        case CardLocations.Discard:
            if (hasAuthority) gameObject.transform.SetParent(playerDiscardPile, false);
            else gameObject.transform.SetParent(opponentDiscardPile, false);
            gameObject.GetComponent<CardUI>().CardFrontUp();
            gameObject.transform.localPosition = Vector3.zero;
            break;
        }

        _cardUI.Highlight(false);
    }
}
