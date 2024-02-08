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
    private PlayerManager _player;
    private List<PrevailOption> _selectedOptions = new();
    private int _nbOptionsToChose;  // Set once from game manager setting
    private int _nbBonusOptions;
    private int _nbOptionsThisTurn;
    public int _totalSelected;
    [SerializeField] private GameObject maxView;
    [SerializeField] private TMP_Text instructions;
    [SerializeField] private Button confirm;
    public static event Action OnPrevailSelectionEnded;

    private void Awake()
    {
        if (!Instance) Instance = this;
    }

    [ClientRpc]
    public void RpcPreparePrevailPanel()
    {
        _player = PlayerManager.GetLocalPlayer();
        instructions.text = "Choose up to " + _nbOptionsThisTurn.ToString();
        maxView.SetActive(false);
    }

    [TargetRpc]
    public void TargetBeginPrevailPhase(NetworkConnection conn, bool bonus)
    {
        maxView.SetActive(true);
        confirm.interactable = true;

        instructions.text = "Choose up to " + _player.Prevails.ToString();
    }

    public bool Increment(PrevailOption option)
    {
        if (_totalSelected >= _nbOptionsThisTurn) return false;

        _totalSelected++;
        _selectedOptions.Add(option);
        return true;
    }

    public bool Decrement(PrevailOption option)
    {
        if (_totalSelected <= 0) return false;

        _totalSelected--;
        _selectedOptions.Remove(option);
        return true;
    }

    public void OnClickConfirm()
    {
        confirm.interactable = false;

        var player = PlayerManager.GetLocalPlayer();
        player.CmdPrevailSelection(_selectedOptions);
    }

    [ClientRpc]
    public void RpcOptionsSelected()
    {
        _totalSelected = 0;
        
        OnPrevailSelectionEnded?.Invoke();
        maxView.SetActive(false);
    }

    [ClientRpc]
    public void RpcReset() => _selectedOptions.Clear();
}

public enum PrevailOption : byte
{
    None = 0,
    CardSelection = 1,
    Trash = 2,
    Score = 3,
}
