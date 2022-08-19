using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardMover : MonoBehaviour
{
    [Header("Playboard Transforms")]
    // [SerializeField] private GameObject phaseSelectionPanel;
    [SerializeField] private Transform playerHand;
    [SerializeField] private Transform playerDropZone;
    [SerializeField] private Transform playerDrawPile;
    [SerializeField] private Transform playerDiscardPile;
    [SerializeField] private Transform opponentHand;
    [SerializeField] private Transform opponentDropZone;
    [SerializeField] private Transform opponentDrawPile;
    [SerializeField] private Transform opponentDiscardPile;

    private Image _highlight;

    private void Awake(){
        playerDrawPile = GameObject.Find("PlayerDrawPile").transform.GetChild(0);
        opponentDrawPile = GameObject.Find("OpponentDrawPile").transform.GetChild(0);
        playerHand = GameObject.Find("PlayerHand").transform;
        opponentHand = GameObject.Find("OpponentHand").transform;
        playerDropZone = GameObject.Find("PlayerDropZone").transform;
        opponentDropZone = GameObject.Find("OpponentDropZone").transform;
        playerDiscardPile = GameObject.Find("PlayerDiscardPile").transform.GetChild(0);
        opponentDiscardPile = GameObject.Find("OpponentDiscardPile").transform.GetChild(0);

        _highlight = gameObject.transform.GetChild(0).GetComponent<Image>();
    }

    public void MoveToDestination(bool hasAuthority, string destination){
        switch (destination){
        default:
            print("<color=orange> Unknown card destination </color>");
            break;
        case "DrawPile":
            if (hasAuthority) gameObject.transform.SetParent(playerDrawPile, false);
            else gameObject.transform.SetParent(opponentDrawPile, false);

            gameObject.GetComponent<CardUI>().CardBackUp();
            break;
        case "Hand":
            if (hasAuthority) {
                gameObject.transform.SetParent(playerHand, false);
                gameObject.GetComponent<CardUI>().CardFrontUp();
                // card.GetComponent<DragDrop>().ChangeDragPermission(true);
            }
            else gameObject.transform.SetParent(opponentHand, false);
            break;
        case "PlayZone":
            if (hasAuthority) gameObject.transform.SetParent(playerDropZone, false);
            else {
                gameObject.transform.SetParent(opponentDropZone, false);
                gameObject.GetComponent<CardUI>().CardFrontUp();
            }
            break;
        case "DiscardPile":
            if (hasAuthority) gameObject.transform.SetParent(playerDiscardPile, false);
            else {
                gameObject.transform.SetParent(opponentDiscardPile, false);
                gameObject.GetComponent<CardUI>().CardFrontUp();
            }
            gameObject.transform.localPosition = Vector3.zero;
            break;
        }

        _highlight.enabled = false;
    }
}
