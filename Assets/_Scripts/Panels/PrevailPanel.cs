using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;
using System;

public class PrevailPanel : NetworkBehaviour
{
    public static PrevailPanel Instance { get; private set; }
    private TurnManager _turnManager;
    private List<PrevailOption> _selectedOptions = new();
    private int _nbOptionsToChose;  // Set once from game manager setting
    private int _nbOptionsThisTurn;
    public int _totalSelected;
    [SerializeField] private GameObject maxView;
    [SerializeField] private TMP_Text instructions;
    [SerializeField] private Button confirm;
    public static event Action OnPrevailEnded;

    private void Awake() {
        if (!Instance) Instance = this;
    }

    [ClientRpc]
    public void RpcPreparePrevailPanel(int nbOptionsToChose){
        _turnManager = TurnManager.Instance;

        _nbOptionsToChose = nbOptionsToChose;
        _nbOptionsThisTurn = nbOptionsToChose;
        instructions.text = "Choose up to " + _nbOptionsThisTurn.ToString();

        maxView.SetActive(false);
    }

    [TargetRpc]
    public void TargetBeginPrevailPhase(NetworkConnection conn, bool bonus){
        maxView.SetActive(true);

        if(!bonus) return;
        _nbOptionsThisTurn++;
        instructions.text = "Choose up to " + _nbOptionsThisTurn.ToString();
    }

    public bool Increment(PrevailOption option){
        if (_totalSelected >= _nbOptionsThisTurn) return false;

        _selectedOptions.Add(option);
        _totalSelected++;
        return true;
    }

    public void Decrement(PrevailOption option){
        _totalSelected--;
        _selectedOptions.Remove(option);
    }

    public void OnClickConfirm(){
        confirm.interactable = false;

        var player = PlayerManager.GetLocalPlayer();
        player.CmdPrevailSelection(_selectedOptions);
    }

    [ClientRpc]
    public void RpcOptionsSelected(){
        _totalSelected = 0;
        _nbOptionsThisTurn = _nbOptionsToChose;
        
        OnPrevailEnded?.Invoke();
        maxView.SetActive(false);
    }
}

public enum PrevailOption{
    Trash,
    Score,
    TopDeck
}
