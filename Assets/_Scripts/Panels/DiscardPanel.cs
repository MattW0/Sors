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

    [SerializeField] private List<GameObject> _selectedCardsList;
    
    private void Awake() {
        
        Instance = this;

        _nbSelected = 0;
        _selectedCardsList = new List<GameObject>();
        _nbCardsToDiscard = GameManager.Instance.nbDiscard;

        gameObject.transform.SetParent(GameObject.Find("UI").transform, false);
        displayText.text = $"Discard 0/{_nbCardsToDiscard} cards";
        OnDiscardPhaseStarted?.Invoke();
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

    public void OnDestroy() {
        Instance = null;
        _selectedCardsList.Clear();
    }
}