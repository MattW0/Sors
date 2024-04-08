using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Market : NetworkBehaviour
{
    public static Market Instance { get; private set; }
    [SerializeField] private GameManager _gameManager;
    private PlayerManager _player;
    private Phase _currentPhase;

    [SerializeField] private MarketUI _ui;
    [SerializeField] private MarketTile[] moneyTiles;
    [SerializeField] private MarketTile[] technologyTiles;
    [SerializeField] private MarketTile[] creatureTiles;
    [SerializeField] private GameObject moneyGrid;
    [SerializeField] private GameObject developmentsGrid;
    [SerializeField] private GameObject creaturesGrid;
    private MarketTile _selectedTile;

    public static event Action OnMarketPhaseEnded;

    private void Awake(){
        if (Instance == null) Instance = this;
    }

    private void Start(){
        moneyTiles = moneyGrid.GetComponentsInChildren<MarketTile>();
        technologyTiles = developmentsGrid.GetComponentsInChildren<MarketTile>();
        creatureTiles = creaturesGrid.GetComponentsInChildren<MarketTile>();

        _gameManager.SetNumberOfMarketTiles(moneyTiles.Length, technologyTiles.Length, creatureTiles.Length);
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
    public void RpcSetCreatureTiles(CardInfo[] creatureTilesInfo){   
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

    // Removed because changed phase bonus
    // [TargetRpc]
    // public void TargetMarketPhaseBonus(NetworkConnection target, int priceReduction){
    //     if (_currentPhase == Phase.Invent){
    //         foreach(var tile in moneyTiles) tile.SetBonus(priceReduction);
    //         foreach(var tile in technologyTiles) tile.SetBonus(priceReduction);
    //     } else if (_currentPhase == Phase.Recruit){
    //         foreach(var tile in creatureTiles) tile.SetBonus(priceReduction);
    //     }
    // }

    [TargetRpc]
    public void TargetMarketPriceReduction(NetworkConnection target, CardType type, int priceReduction){
        if (type == CardType.Money){
            foreach(var tile in moneyTiles) tile.SetBonus(priceReduction);
        } else if (type == CardType.Technology){
            foreach(var tile in technologyTiles) tile.SetBonus(priceReduction);
        } else if (type == CardType.Creature){
            foreach(var tile in creatureTiles) tile.SetBonus(priceReduction);
        }
    }

    [TargetRpc]
    public void TargetCheckMarketPrices(NetworkConnection target, int playerCash){
        if (_currentPhase == Phase.Invent){
            foreach(var tile in moneyTiles) tile.Interactable = playerCash >= tile.Cost;
            foreach (var tile in technologyTiles) tile.Interactable = playerCash >= tile.Cost;
        } else if (_currentPhase == Phase.Recruit){
            foreach (var tile in creatureTiles) tile.Interactable = playerCash >= tile.Cost;
        }
    }
    #endregion
    
    public void PlayerSelectsTile(MarketTile tile)
    {
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
    public void TargetResetMarket(NetworkConnection target, int actionsLeft)
    {
        _selectedTile.HasBeenChosen();
        PlayerDeselectsTile();
        _ui.ResetInteractionButtons();
    }

    [ClientRpc]
    public void RpcReplaceRecruitTile(string oldTitle, CardInfo newCardInfo)
    {
        // TODO: Implement
        MaxButton();
        var nextTile = _gameManager.GetNewCreatureFromDb();

        // TODO: Update
        // foreach (var tile in creatureTiles){
        //     if (tile.cardInfo.title != oldTitle) continue;

        //     tile.SetTile(newCardInfo);
        //     break;
        // }
    }
    
    [ClientRpc]
    public void RpcEndMarketPhase(){
        _ui.EndPhase();
        OnMarketPhaseEnded?.Invoke();
    }
    #endregion

    public void PlayerPressedConfirmButton()
    {        
        // Need the cost here as market bonus are not reflected in cardInfo itself
        if(_selectedTile) _player.CmdConfirmBuy(_selectedTile.cardInfo, _selectedTile.Cost);
        else print("ERROR: No tile selected");
    }

    public void PlayerPressedSkipButton() => _player.CmdSkipBuy();

    [ClientRpc]
    public void RpcMinButton() => _ui.MinButton();

    [ClientRpc]
    public void RpcMaxButton() => _ui.MaxButton();
    public void MaxButton() => _ui.MaxButton();

    public List<CardInfo>[] GetTileInfos(){

        var scriptableTiles = new List<CardInfo>[3];
        scriptableTiles[0] = new List<CardInfo>();
        scriptableTiles[1] = new List<CardInfo>();
        scriptableTiles[2] = new List<CardInfo>();

        foreach (var tile in moneyTiles) scriptableTiles[0].Add(tile.cardInfo);
        foreach (var tile in technologyTiles) scriptableTiles[1].Add(tile.cardInfo);
        foreach (var tile in creatureTiles) scriptableTiles[2].Add(tile.cardInfo);

        return scriptableTiles;
    }
}
