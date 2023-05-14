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
    private GameManager _gameManager;
    private Phase _currentPhase;
    // private List<RecruitTile> _previouslyRecruited = new ();
    // public List<RecruitTile> GetPreviouslySelectedRecruitTiles() => _previouslyRecruited;

    [SerializeField] private KingdomUI _ui;
    [SerializeField] private DevelopTile[] developTiles;
    [SerializeField] private RecruitTile[] recruitTiles;
    [SerializeField] private GameObject developGrid;
    [SerializeField] private GameObject recruitGrid;
    public CardInfo selection;

    public static event Action OnDevelopPhaseEnded;
    public static event Action OnRecruitPhaseEnded;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        _gameManager = GameManager.Instance;
    }

    #region Setup
    [ClientRpc]
    public void RpcSetDevelopTiles(CardInfo[] developTilesInfo){
        developTiles = new DevelopTile[developTilesInfo.Length];
        developTiles = developGrid.GetComponentsInChildren<DevelopTile>();

        for (var i = 0; i < developTilesInfo.Length; i++) developTiles[i].SetTile(developTilesInfo[i]);
    }

    [ClientRpc]
    public void RpcSetRecruitTiles(CardInfo[] recruitTilesInfo){   
        recruitTiles = new RecruitTile[recruitTilesInfo.Length];
        recruitTiles = recruitGrid.GetComponentsInChildren<RecruitTile>();

        for (var i = 0; i < recruitTilesInfo.Length; i++) recruitTiles[i].SetTile(recruitTilesInfo[i]);
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
        if (_currentPhase == Phase.Develop)
            foreach(var tile in developTiles) tile.SetDevelopBonus(priceReduction);
        else if (_currentPhase == Phase.Recruit)
            foreach(var tile in recruitTiles) tile.SetRecruitBonus(priceReduction);
    }

    [TargetRpc]
    public void TargetCheckPriceKingdomTile(NetworkConnection target, int playerCash){
        if (_currentPhase == Phase.Develop)
            foreach (var tile in developTiles) tile.Developable = playerCash >= tile.Cost;
        else if (_currentPhase == Phase.Recruit){
            foreach (var tile in recruitTiles){
                // if (_previouslyRecruited.Contains(tile)) continue;
                tile.Recruitable = playerCash >= tile.Cost;
            }
        }
    }
    #endregion
    
    public void PlayerSelectsTile(CardInfo cardInfo){
        selection = cardInfo;
        _ui.SelectTile(cardInfo);
    }

    public void PlayerDeselectsTile(CardInfo cardInfo){
        selection.title = null; // hack-around for check in TurnManager
        _ui.DeselectTile(cardInfo);
    }

    #region Reset and EoP
    [TargetRpc]
    public void TargetResetDevelop(NetworkConnection target, int developsLeft){        
        if (developsLeft <= 0) return;
        _ui.ResetInteractionButtons();
    }

    [TargetRpc]
    public void TargetResetRecruit(NetworkConnection target, int recruitsLeft){        
        // _previouslyRecruited.Add(tile);
        // foreach (var tile in _previouslySelected) tile.ShowAsRecruited();

        if (recruitsLeft <= 0) return;
        _ui.ResetInteractionButtons();
    }

    [ClientRpc]
    public void RpcReplaceRecruitTile(string oldTitle, CardInfo newCardInfo){
        foreach (var tile in recruitTiles){
            if (tile.cardInfo.title != oldTitle) continue;

            tile.SetTile(newCardInfo);
            break;
        }
    }
    
    [ClientRpc]
    public void RpcEndDevelop(){
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
            player.PlayerSkips();
            return;
        }

        switch (_currentPhase)
        {
            case Phase.Develop:
                player.PlayerDevelops(selection);
                break;
            case Phase.Recruit:
                player.PlayerRecruits(selection);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        selection.Destroy();
    }

    public void MaxButton() => _ui.MaxButton();
}
