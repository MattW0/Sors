using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Kingdom : NetworkBehaviour
{
    public static Kingdom Instance { get; private set; }
    [SerializeField] private GameManager _gameManager;
    private Phase _currentPhase;
    // private List<RecruitTile> _previouslyRecruited = new ();
    // public List<RecruitTile> GetPreviouslySelectedRecruitTiles() => _previouslyRecruited;

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

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        moneyTiles = moneyGrid.GetComponentsInChildren<KingdomTile>();
        technologyTiles = developmentsGrid.GetComponentsInChildren<KingdomTile>();
        creatureTiles = creaturesGrid.GetComponentsInChildren<KingdomTile>();

        _gameManager.SetNumberOfKingdomTiles(moneyTiles.Length, technologyTiles.Length, creatureTiles.Length);
    }

    #region Setup
    [ClientRpc]
    public void RpcSetMoneyTiles(CardInfo[] moneyTilesInfo){
        for (var i = 0; i < moneyTilesInfo.Length; i++) 
            moneyTiles[i].SetTile(moneyTilesInfo[i]);
    }

    [ClientRpc]
    public void RpcSetDevelopmentTiles(CardInfo[] technologyTilesInfo){
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
        } else if (type == CardType.Development){
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
            foreach (var tile in creatureTiles){
                // if (_previouslyRecruited.Contains(tile)) continue;
                tile.Interactable = playerCash >= tile.Cost;
            }
        }
    }
    #endregion
    
    public void PlayerSelectsTile(KingdomTile tile){
        _selectedTile = tile;
        _ui.SelectTile(tile.cardInfo);
    }

    public void PlayerDeselectsTile(){
        _selectedTile = null;
        _ui.DeselectTile();
    }

    #region Reset and EoP
    [TargetRpc]
    public void TargetResetKingdom(NetworkConnection target, int actionsLeft){        
        if (actionsLeft <= 0) return;
        _ui.ResetInteractionButtons();
    }

    // [TargetRpc]
    // public void TargetResetRecruit(NetworkConnection target, int recruitsLeft){        
    //     // _previouslyRecruited.Add(tile);
    //     // foreach (var tile in _previouslySelected) tile.ShowAsRecruited();

    //     if (recruitsLeft <= 0) return;
    //     _ui.ResetInteractionButtons();
    // }

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
        // _previouslySelected.Clear();
        _ui.EndPhase();
        OnRecruitPhaseEnded?.Invoke();
    }
    #endregion

    public void PlayerPressedButton(bool skip)
    {
        var player = PlayerManager.GetLocalPlayer();
        if(skip) {

            if (_currentPhase == Phase.Invent){
                player.Invents = 0;
            } else if (_currentPhase == Phase.Recruit){
                player.Recruits = 0;
            }

            player.PlayerSkips();
            return;
        }

        if (_currentPhase == Phase.Invent){
            player.Invents--;
        } else if (_currentPhase == Phase.Recruit){
            player.Recruits--;
        }
        
        player.PlayerSelectsKingdomTile(_selectedTile.cardInfo, _selectedTile.Cost);
        _selectedTile = null;
    }

    public void MaxButton() => _ui.MaxButton();
}
