using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;

public class CombatManager : NetworkBehaviour
{
    public static CombatManager Instance { get; private set; }
    public static event Action<TurnState> OnCombatStateChanged;

    private Dictionary<CreatureEntity, BattleZoneEntity> _attackerTarget = new();
    private Dictionary<CreatureEntity, CreatureEntity> _blockerAttacker = new();
    private List<CombatClash> _clashes = new();
    private GameManager _gameManager;
    private TurnManager _turnManager;
    private BoardManager _boardManager;
    [SerializeField] private DamageSystem _damageSystem;
    private PlayerInterfaceManager _playerInterfaceManager;
    private List<PlayerManager> _readyPlayers = new();

    private void Awake()
    {
        if (!Instance) Instance = this;
        
        GameManager.OnGameStart += Prepare;
    }

    public void UpdateCombatState(TurnState newState)
    {
        OnCombatStateChanged?.Invoke(newState);

        if (newState == TurnState.CombatDamage) ResolveDamage();
        else if (newState == TurnState.CombatCleanUp) CombatCleanUp(false);
    }

    private void Prepare(GameOptions options)
    {
        _gameManager = GameManager.Instance;
        _turnManager = TurnManager.Instance;
        _boardManager = BoardManager.Instance;
        _playerInterfaceManager = PlayerInterfaceManager.Instance;
    }

    public void PlayerChoosesTargetToAttack(BattleZoneEntity target, List<CreatureEntity> attackers)
    {
        foreach(var a in attackers) _attackerTarget.Add(a, target);
    }

    public void PlayerDeclaredAttackers(PlayerManager player)
    {
        if (! AllPlayersReady(player)) return;

        foreach (var (a, t) in _attackerTarget)
        {
            a.RpcDeclaredAttack(t);
            _playerInterfaceManager.RpcLog(a.Owner.ID, a.Title, t.Title, LogType.CombatAttacker);
        }

        UpdateCombatState(TurnState.Blockers);
    }
    public void PlayerChoosesAttackerToBlock(CreatureEntity attacker, List<CreatureEntity> blockers)
    {
        foreach(var b in blockers) _blockerAttacker.Add(b, attacker);
    }

    public void PlayerDeclaredBlockers(PlayerManager player)
    {
        if (! AllPlayersReady(player)) return;

        foreach (var (b, a) in _blockerAttacker)
        {
            b.RpcDeclaredBlock(a);
            _playerInterfaceManager.RpcLog(a.Owner.ID, b.Title, a.Title, LogType.CombatBlocker);
        }
        UpdateCombatState(TurnState.CombatDamage);
    }

    private bool AllPlayersReady(PlayerManager player)
    {
        _readyPlayers.Add(player);
        if (_readyPlayers.Count != _gameManager.players.Count) return false;

        _readyPlayers.Clear();
        return true;
    }

    private void ResolveDamage()
    {
        // Skip damage logic if there are no attackers 
        if (_attackerTarget.Count == 0) UpdateCombatState(TurnState.CombatCleanUp);
        else _damageSystem.EvaluateBlocks(_attackerTarget, _blockerAttacker);
    }

    public void CombatCleanUp(bool forced)
    {
        _readyPlayers.Clear();
        _attackerTarget.Clear();
        _blockerAttacker.Clear();
        _clashes.Clear();

        if(!forced) _turnManager.CombatCleanUp().Forget();
    }

    private void OnDestroy(){
        GameManager.OnGameStart -= Prepare;
    }
}