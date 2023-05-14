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
    private List<RecruitTile> _previouslySelected = new ();
    public List<RecruitTile> GetPreviouslySelectedRecruitTiles() => _previouslySelected;

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

    #region Bonus
    [TargetRpc]
    public void TargetDevelopBonus(NetworkConnection target, int priceReduction){
        foreach(var tile in developTiles) tile.SetDevelopBonus(priceReduction);
    }

    [TargetRpc]
    public void TargetRecruitBonus(NetworkConnection target, int priceReduction){
        foreach(var tile in recruitTiles) tile.SetRecruitBonus(priceReduction);
    }

    [TargetRpc]
    public void TargetCheckDevelopability(NetworkConnection target, int playerCash){
        foreach (var dt in developTiles) dt.Developable = playerCash >= dt.Cost;
    }

    [TargetRpc]
    public void TargetCheckRecruitability(NetworkConnection target, int playerCash){
        foreach (var tile in recruitTiles){
            if (_previouslySelected.Contains(tile)) continue;
            tile.Recruitable = playerCash >= tile.Cost;
        }
    }
    #endregion
    
    #region Selection
    public void PlayerSelectsDevelopTile(DevelopTile tile) => selection = tile.cardInfo;
    public void PlayerDeselectsDevelopTile(DevelopTile tile) => selection.title = null;

    public void PlayerSelectsRecruitTile(RecruitTile tile){
        selection = tile.cardInfo;
        _previouslySelected.Add(tile);
    }

    public void PlayerDeselectsRecruitTile(RecruitTile tile){
        selection.title = null; // hack-around for check in TurnManager
        _previouslySelected.Remove(tile);
    }
    #endregion

    #region Reset and EoP
    [TargetRpc]
    public void TargetResetDevelop(NetworkConnection target, int developsLeft){        
        if (developsLeft <= 0) return;
        _ui.ResetInteractionButtons();
    }

    [TargetRpc]
    public void TargetResetRecruit(NetworkConnection target, int recruitsLeft){        
        foreach (var tile in _previouslySelected) tile.ShowAsRecruited();

        if (recruitsLeft <= 0) return;
        _ui.ResetInteractionButtons();
    }

    [ClientRpc]
    public void RpcReplaceRecruitTile(string oldTitle, CardInfo cardInfo){
        foreach (var tile in recruitTiles){
            if (tile.cardInfo.title != oldTitle) continue;

            tile.SetTile(cardInfo);
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
        _previouslySelected.Clear();
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
