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

    [ClientRpc]
    public void RpcSetDevelopTiles(CardInfo[] developTilesInfo)
    {   
        developTiles = new DevelopTile[developTilesInfo.Length];
        developTiles = developGrid.GetComponentsInChildren<DevelopTile>();

        for (var i = 0; i < developTilesInfo.Length; i++)
        {
            developTiles[i].SetTile(developTilesInfo[i]);
        }
    }

    [ClientRpc]
    public void RpcSetRecruitTiles(CardInfo[] recruitTilesInfo)
    {   
        recruitTiles = new RecruitTile[recruitTilesInfo.Length];
        recruitTiles = recruitGrid.GetComponentsInChildren<RecruitTile>();

        for (var i = 0; i < recruitTilesInfo.Length; i++)
        {
            recruitTiles[i].SetTile(recruitTilesInfo[i]);
        }
    }

    [ClientRpc]
    public void RpcBeginPhase(Phase phase)
    {
        _currentPhase = phase;
        _ui.BeginPhase(phase);        
    }

    public void PlayerPressedButton(bool skip)
    {
        var player = PlayerManager.GetPlayerManager();
        if(skip) {
            player.PlayerPressedReadyButton();
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

    [ClientRpc]
    public void RpcReplaceCard(string oldTitle, CardInfo cardInfo)
    {
        foreach (var kc in recruitTiles)
        {
            if (kc.cardInfo.title != oldTitle) continue;

            kc.SetTile(cardInfo);
            break;
        }
    }

    [TargetRpc]
    public void TargetDevelopBonus(NetworkConnection target, int priceReduction){
        foreach(var tile in developTiles) tile.SetDevelopBonus(priceReduction);
    }

    [TargetRpc]
    public void TargetCheckDevelopability(NetworkConnection target, int playerCash){
        foreach (var dt in developTiles) dt.Developable = playerCash >= dt.Cost;
    }
    
    [TargetRpc]
    public void TargetCheckRecruitability(NetworkConnection target, int playerCash){
        var skipCards = _ui.GetPreviouslySelectedRecruitTiles();

        foreach (var kc in recruitTiles)
        {
            if (skipCards.Contains(kc)) continue;
            kc.Recruitable = playerCash >= kc.Cost;
        }
    }

    [TargetRpc]
    public void TargetResetRecruit(NetworkConnection target, int recruitsLeft){
        if (recruitsLeft > 0) _ui.ResetRecruitButton();
    }

    [ClientRpc]
    public void RpcEndDevelop()
    {
        OnDevelopPhaseEnded?.Invoke();
        _ui.EndDevelop();
    }
    
    [ClientRpc]
    public void RpcEndRecruit()
    {
        OnRecruitPhaseEnded?.Invoke();
        _ui.EndRecruit();
    }

    public void MaxButton() => _ui.MaxButton();
}
