using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;

public class PrevailPanel : NetworkBehaviour
{
    public static PrevailPanel Instance { get; private set; }
    private TurnManager _turnManager;
    private List<PrevailOption> _selectedOptions = new();
    private int _nbOptionsToChose;
    public int _totalSelected;
    [SerializeField] private GameObject maxView;
    [SerializeField] private TMP_Text instructions;
    [SerializeField] private Button confirm;

    private void Awake() {
        if (!Instance) Instance = this;
    }

    [ClientRpc]
    public void RpcPreparePrevailPanel(int nbOptionsToChose){
        _turnManager = TurnManager.Instance;

        _nbOptionsToChose = nbOptionsToChose;
        instructions.text = "Choose up to " + _nbOptionsToChose.ToString();

        maxView.SetActive(false);
    }

    [TargetRpc]
    public void TargetBeginPrevailPhase(NetworkConnection conn, bool bonus){
        maxView.SetActive(true);

        if(!bonus) return;
        _nbOptionsToChose++;
        instructions.text = "Choose up to " + _nbOptionsToChose.ToString();
    }

    public bool Increment(PrevailOption option){
        if (_totalSelected >= _nbOptionsToChose) return false;

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
}

public enum PrevailOption{
    Trash,
    Score,
    TopDeck
}
