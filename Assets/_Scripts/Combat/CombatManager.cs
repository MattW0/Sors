using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;
using UnityEditor;

public class CombatManager : NetworkBehaviour
{
    public static CombatManager Instance { get; private set; }
    public static event Action<CombatState> OnCombatStateChanged;

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

    public void UpdateCombatState(CombatState newState)
    {
        OnCombatStateChanged?.Invoke(newState);

        switch (newState)
        {
            case CombatState.Damage:
                ResolveDamage();
                break;
            case CombatState.CleanUp:
                CombatCleanUp(false);
                break;
        }
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
        foreach(var a in attackers) 
            _attackerTarget.Add(a, target);
    }

    public void PlayerDeclaredAttackers(PlayerManager player)
    {
        _readyPlayers.Add(player);
        if (_readyPlayers.Count != _gameManager.players.Count) return;
        _readyPlayers.Clear();

        foreach(var (a, t) in _attackerTarget)
        {
            a.RpcDeclaredAttack(t);
            _playerInterfaceManager.RpcLog($"  - {a.Title} attacks {t.Title}", LogType.CombatAttacker);
        }

        UpdateCombatState(CombatState.Blockers);
    }

    public void PlayerChoosesAttackerToBlock(CreatureEntity attacker, List<CreatureEntity> blockers)
    {
        foreach(var b in blockers) 
            _blockerAttacker.Add(b, attacker);
    }

    public void PlayerDeclaredBlockers(PlayerManager player)
    {
        _readyPlayers.Add(player);
        if (_readyPlayers.Count != _gameManager.players.Count) return;
        _readyPlayers.Clear();

        foreach (var (b, a) in _blockerAttacker)
        {
            b.RpcDeclaredBlock(a);
            _playerInterfaceManager.RpcLog($"  - {b.Title} blocks {a.Title}", LogType.CombatBlocker);
        }
        UpdateCombatState(CombatState.Damage);
    }

    private void ResolveDamage()
    {
        // Skip damage logic if there are no attackers 
        if (_attackerTarget.Count == 0) UpdateCombatState(CombatState.CleanUp);
        else _damageSystem.EvaluateBlocks(_attackerTarget, _blockerAttacker);
    }

    public void CombatCleanUp(bool forced)
    {
        _readyPlayers.Clear();
        _attackerTarget.Clear();
        _blockerAttacker.Clear();
        _clashes.Clear();

        UpdateCombatState(CombatState.Idle);
        if(!forced) _turnManager.CombatCleanUp().Forget();
    }

    public void PlayerPressedReadyButton(PlayerManager player) => _boardManager.PlayerPressedReadyButton(player);
    private void OnDestroy(){
        GameManager.OnGameStart -= Prepare;
    }
}

public enum CombatState : byte
{
    Idle,
    Attackers,
    Blockers,
    Damage,
    CleanUp
}