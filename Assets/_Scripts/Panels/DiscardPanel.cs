using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DiscardPanel : NetworkBehaviour
{   
    public static DiscardPanel Instance { get; private set; }

    public static event Action OnDiscardPhaseStarted;

    private int _nbSelected;
    private int _nbCardsToDiscard;
    public Button confirm;
    public TMP_Text displayText;
    public GameObject waitingText;

    [SerializeField] private List<GameObject> selectedCardsList;
    
    private void Awake() {
        
        if (!Instance) Instance = this;

        _nbSelected = 0;
        selectedCardsList = new List<GameObject>();
        _nbCardsToDiscard = GameManager.Instance.nbDiscard;

        gameObject.transform.SetParent(GameObject.Find("UI").transform, false);
        displayText.text = $"Discard 0/{_nbCardsToDiscard} cards";
        OnDiscardPhaseStarted?.Invoke();
    }

    public void CardToDiscardSelected(GameObject card, bool selected){
        if (selected) {
            _nbSelected++;
            selectedCardsList.Add(card);
        } else {
            _nbSelected--;
            selectedCardsList.Remove(card);
        }

        displayText.text = $"Discard {_nbSelected}/{_nbCardsToDiscard} cards";

        if (_nbSelected == _nbCardsToDiscard) confirm.interactable = true;
        else confirm.interactable = false;
    }

    public void ConfirmButtonPressed(){
        confirm.gameObject.SetActive(false);
        waitingText.SetActive(true);

        var networkIdentity = NetworkClient.connection.identity;
        var p = networkIdentity.GetComponent<PlayerManager>();
        p.CmdDiscardSelection(selectedCardsList);
    }

    public void OnDestroy() {
        Instance = null;
        selectedCardsList.Clear();
    }
}