using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Mirror;
using UnityEditor;

public class CombatManager : NetworkBehaviour
{
    public static CombatManager Instance { get; private set; }
    private static CombatState state { get; set; }

    public static event Action<CombatState> OnCombatStateChanged;

    private Dictionary<CreatureEntity, BattleZoneEntity> _attackerTarget = new ();
    private Dictionary<CreatureEntity, CreatureEntity> _blockerAttacker = new ();
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

    public void UpdateCombatState(CombatState newState){
        state = newState;
        
        OnCombatStateChanged?.Invoke(state);

        switch(state){
            case CombatState.Idle:
                break;
            case CombatState.Attackers:
                // _playerInterfaceManager.RpcLog(" --- Starting Combat --- ", LogType.Combat);
                break;
            case CombatState.Blockers:
                _playerInterfaceManager.RpcLog(" - Attackers declared", LogType.Standard);
                break;
            case CombatState.Damage:
                _playerInterfaceManager.RpcLog(" - Blockers declared", LogType.Standard);
                ResolveDamage();
                break;
            case CombatState.CleanUp:
                _playerInterfaceManager.RpcLog(" - Combat ends - ", LogType.Standard);
                StartCoroutine(CombatCleanUp(false));
                break;
            default:
                print("<color=red>Invalid turn state</color>");
                throw new System.ArgumentOutOfRangeException(nameof(newState), newState, null);
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
        foreach(var a in attackers) _attackerTarget.Add(a, target);

        // print($"Player {attackers[0].Owner.PlayerName} declared attack on {target.Title}");
        var attackingPlayerConn = attackers[0].Owner.connectionToClient;

        foreach (var attacker in attackers){
            // Locally shows attackers (freezes arrows to target)
            attacker.TargetDeclaredAttack(attackingPlayerConn, target);
        }
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
        foreach(var b in blockers) _blockerAttacker.Add(b, attacker);

        var blockingPlayerConn = blockers[0].Owner.connectionToClient;
        
        foreach (var blocker in blockers)
        {
            // Only shows local blockers
            blocker.TargetDeclaredBlock(blockingPlayerConn, attacker);
        }
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

    #region Combat Logic
    private void ResolveDamage()
    {
        // Skip damage logic if there are no attackers 
        if (_attackerTarget.Count == 0) UpdateCombatState(CombatState.CleanUp);
        else EvaluateBlocks();
    }

    private void EvaluateBlocks(){
        if (_blockerAttacker.Count == 0){
            EvaluateUnblocked();
            return;
        }

        int excessDamage = int.MinValue;
        var prevA = _blockerAttacker.First().Value;
        foreach(var (b, a) in _blockerAttacker)
        {
            var dmg = a.Attack;
            if (excessDamage != int.MinValue) dmg = excessDamage; 
            excessDamage = EvaluateClashDamage(a, b, dmg);

            // In the same blocker group -> Do normal damage
            if (!prevA.Equals(a)) {
                CheckTrampleDamage(prevA, excessDamage);
                excessDamage = int.MinValue;
                _attackerTarget.Remove(prevA);
            }
            prevA = a;
        }

        CheckTrampleDamage(prevA, excessDamage);
        _attackerTarget.Remove(prevA);

        EvaluateUnblocked();
    }

    private void EvaluateUnblocked()
    {
        print($"Unblocked creatures after blocks : {_attackerTarget.Count}");

        foreach(var (a, t) in _attackerTarget)
            _clashes.Add(new CombatClash(a, t, a.Attack));

        _damageSystem.ExecuteClashes(_clashes);
    }
    #endregion

    // Calculate the outcome of a combat clash between two creature entities, considering their traits and attack damage. Returns the remaining attack damage after the clash.
    private int EvaluateClashDamage(CreatureEntity attacker, CreatureEntity blocker, int attackDamage)
    {
        print($"CombatClash: {attacker.Title} vs {blocker.Title} with {attackDamage} damage");
        _clashes.Add(new CombatClash(attacker, blocker, attackDamage, blocker.Attack));

        return attackDamage - blocker.Health;
    }

    private void CheckTrampleDamage(CreatureEntity attacker, int excessDamage)
    {
        if (!attacker.GetTraits().Contains(Traits.Trample)) return;

        print($"Trample of {attacker.Title} with {excessDamage} excess damage");
        var target = _attackerTarget[attacker];
        _clashes.Add(new CombatClash(attacker, target, excessDamage));
    }

    public void EntityDealsDamage(CreatureEntity a, BattleZoneEntity t, int damage)
    {
        t.EntityTakesDamage(damage, a.GetTraits().Contains(Traits.Deathtouch));
    }

    public IEnumerator CombatCleanUp(bool forced)
    {
        _readyPlayers.Clear();
        _attackerTarget.Clear();
        _blockerAttacker.Clear();
        _clashes.Clear();
        
        yield return new WaitForSeconds(SorsTimings.combatCleanUp);

        UpdateCombatState(CombatState.Idle);
        if(!forced) _turnManager.FinishCombat();
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