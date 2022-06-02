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
    private GameManager _gameManager;

    public static event Action OnDiscardPhaseStarted;

    private int _nbSelected;
    private int _nbCardsToDiscard;
    public Button confirm;
    public TMP_Text displayText;
    public GameObject waitingText;

    private List<GameObject> _selectedCardsList;
    
    private void Awake() {
        if (Instance == null) Instance = this;

        gameObject.transform.SetParent(GameObject.Find("UI").transform, false);
        OnDiscardPhaseStarted?.Invoke();

        _gameManager = GameManager.Instance;

        _selectedCardsList = new List<GameObject>();
        _nbCardsToDiscard = _gameManager.nbDiscard;
        displayText.text = $"Discard 0/{_nbCardsToDiscard} cards";
    }

    public void CardToDiscardSelected(GameObject _card, bool selected){
        if (selected) {
            _nbSelected++;
            _selectedCardsList.Add(_card);
        } else {
            _nbSelected--;
            _selectedCardsList.Remove(_card);
        }

        displayText.text = $"Discard {_nbSelected}/{_nbCardsToDiscard} cards";

        if (_nbSelected == _nbCardsToDiscard) confirm.interactable = true;
        else confirm.interactable = false;
    }

    public void ConfirmButtonPressed(){
        confirm.gameObject.SetActive(false);
        waitingText.SetActive(true);

        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        PlayerManager p = networkIdentity.GetComponent<PlayerManager>();
        p.CmdDiscardSelection(_selectedCardsList);
    }
}