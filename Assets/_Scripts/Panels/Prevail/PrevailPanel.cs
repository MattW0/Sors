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
    private int _numberOptionsAvailable;
    public int _totalSelected;
    private PrevailUI _ui;
    public static event Action OnPrevailSelectionEnded;

    private void Awake()
    {
        if (!Instance) Instance = this;
        _ui = GetComponent<PrevailUI>();
    }

    [ClientRpc]
    public void RpcPreparePrevailPanel()
    {
        _player = PlayerManager.GetLocalPlayer();
    }

    [TargetRpc]
    public void TargetBeginPrevailPhase(NetworkConnection conn, int numberOptions)
    {
        _ui.Begin(numberOptions);
        _numberOptionsAvailable = numberOptions;
    }

    public bool Increment(PrevailOption option)
    {
        if (_totalSelected >= _numberOptionsAvailable) return false;

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

    public void ConfirmButonClicked()
    {
        var player = PlayerManager.GetLocalPlayer();
        player.CmdPrevailSelection(_selectedOptions);
        print($" - Prevail: Picked {_selectedOptions.Count} options");
    }

    [ClientRpc]
    public void RpcOptionsSelected()
    {
        _totalSelected = 0;
        OnPrevailSelectionEnded?.Invoke();
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
