using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Kingdom : NetworkBehaviour
{
    public static Kingdom Instance { get; private set; }
    [SerializeField] private GameManager _gameManager;
    private PlayerManager _player;
    private Phase _currentPhase;

    [SerializeField] private KingdomUI _ui;
    [SerializeField] private KingdomTile[] moneyTiles;
    [SerializeField] private KingdomTile[] technologyTiles;
    [SerializeField] private KingdomTile[] creatureTiles;
    [SerializeField] private GameObject moneyGrid;
    [SerializeField] private GameObject developmentsGrid;
    [SerializeField] private GameObject creaturesGrid;
    private KingdomTile _selectedTile;

    public static event Action OnDevelopPhaseEnded;
    public static event Action OnRecruitPhaseEnded;

    private void Awake(){
        if (Instance == null) Instance = this;
    }

    private void Start(){
        moneyTiles = moneyGrid.GetComponentsInChildren<KingdomTile>();
        technologyTiles = developmentsGrid.GetComponentsInChildren<KingdomTile>();
        creatureTiles = creaturesGrid.GetComponentsInChildren<KingdomTile>();

        _gameManager.SetNumberOfKingdomTiles(moneyTiles.Length, technologyTiles.Length, creatureTiles.Length);
    }

    #region Setup

    [ClientRpc]
    public void RpcSetPlayer(){
        _player = PlayerManager.GetLocalPlayer();
    }
    [ClientRpc]
    public void RpcSetMoneyTiles(CardInfo[] moneyTilesInfo){
        for (var i = 0; i < moneyTilesInfo.Length; i++) 
            moneyTiles[i].SetTile(moneyTilesInfo[i]);
    }

    [ClientRpc]
    public void RpcSetTechnologyTiles(CardInfo[] technologyTilesInfo){
        for (var i = 0; i < technologyTilesInfo.Length; i++) 
            technologyTiles[i].SetTile(technologyTilesInfo[i]);
    }

    [ClientRpc]
    public void RpcSetRecruitTiles(CardInfo[] creatureTilesInfo){   
        for (var i = 0; i < creatureTilesInfo.Length; i++) 
            creatureTiles[i].SetTile(creatureTilesInfo[i]);
    }

    [ClientRpc]
    public void RpcBeginPhase(Phase phase){
        _currentPhase = phase;
        _ui.BeginPhase(phase);
    }
    #endregion

    #region Tile Cost
    [TargetRpc]
    public void TargetKingdomBonus(NetworkConnection target, int priceReduction){
        if (_currentPhase == Phase.Invent){
            foreach(var tile in moneyTiles) tile.SetBonus(priceReduction);
            foreach(var tile in technologyTiles) tile.SetBonus(priceReduction);
        } else if (_currentPhase == Phase.Recruit){
            foreach(var tile in creatureTiles) tile.SetBonus(priceReduction);
        }
    }

    [TargetRpc]
    public void TargetKingdomPriceReduction(NetworkConnection target, CardType type, int priceReduction){
        if (type == CardType.Money){
            foreach(var tile in moneyTiles) tile.SetBonus(priceReduction);
        } else if (type == CardType.Technology){
            foreach(var tile in technologyTiles) tile.SetBonus(priceReduction);
        } else if (_currentPhase == Phase.Recruit){
            foreach(var tile in creatureTiles) tile.SetBonus(priceReduction);
        }
    }

    [TargetRpc]
    public void TargetCheckPriceKingdomTile(NetworkConnection target, int playerCash){
        if (_currentPhase == Phase.Invent){
            foreach(var tile in moneyTiles) tile.Interactable = playerCash >= tile.Cost;
            foreach (var tile in technologyTiles) tile.Interactable = playerCash >= tile.Cost;
        } else if (_currentPhase == Phase.Recruit){
            foreach (var tile in creatureTiles) tile.Interactable = playerCash >= tile.Cost;
        }
    }
    #endregion
    
    public void PreviewCard(CardInfo cardInfo) => _ui.PreviewCard(cardInfo);
    public void PlayerSelectsTile(KingdomTile tile){
        PlayerDeselectsTile();
        
        _selectedTile = tile;
        _ui.SelectTile(tile.cardInfo);

        // Reset all other tiles -> single selection
        if (_currentPhase == Phase.Invent){
            foreach (var t in moneyTiles) if (t != tile) t.ResetSelected();
            foreach (var t in technologyTiles) if (t != tile) t.ResetSelected();
        } else if (_currentPhase == Phase.Recruit){
            foreach (var t in creatureTiles) if (t != tile) t.ResetSelected();
        }        
    }

    public void PlayerDeselectsTile(){
        _selectedTile = null;
        _ui.DeselectTile();
    }

    #region Reset and EoP
    [TargetRpc]
    public void TargetResetKingdom(NetworkConnection target, int actionsLeft){
        if (actionsLeft <= 0) return;

        _selectedTile.HasBeenChosen();
        PlayerDeselectsTile();
        _ui.ResetInteractionButtons();
    }

    [ClientRpc]
    public void RpcReplaceRecruitTile(string oldTitle, CardInfo newCardInfo){
        foreach (var tile in creatureTiles){
            if (tile.cardInfo.title != oldTitle) continue;

            tile.SetTile(newCardInfo);
            break;
        }
    }
    
    [ClientRpc]
    public void RpcEndKingdomPhase(){
        _ui.EndPhase();
        OnDevelopPhaseEnded?.Invoke();
    }

    [ClientRpc]
    public void RpcEndRecruit(){
        _ui.EndPhase();
        OnRecruitPhaseEnded?.Invoke();
    }
    #endregion

    public void PlayerPressedButton(bool skip){
        if(skip) {
            _player.CmdSkipKingdomBuy();
            return;
        }
        
        if(_selectedTile) _player.CmdSelectKingdomTile(_selectedTile.cardInfo, _selectedTile.Cost);
        else print("ERROR: No tile selected");
    }

    public void MaxButton() => _ui.MaxButton();
}
