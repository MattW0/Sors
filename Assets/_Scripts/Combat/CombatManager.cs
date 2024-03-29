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
        // else StartCoroutine(Blocks());
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
            // In the same blocker group -> Do normal damage
            if (!prevA.Equals(a)) {
                CheckTrampleDamage(prevA, excessDamage);
                excessDamage = int.MinValue;
                _attackerTarget.Remove(prevA);
            }
            prevA = a;

            var dmg = a.Attack;
            if (excessDamage != int.MinValue) dmg = excessDamage; 
            excessDamage = EvaluateClashDamage(a, b, dmg);
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

        _damageSystem.SetClashes(_clashes);
    }

    // private IEnumerator Blocks()
    // {
    //     if (_blockerAttacker.Count == 0){
    //         StartCoroutine(Unblocked());
    //         yield break;
    //     }

    //     CreatureEntity prevA = _blockerAttacker.First().Value;
    //     int excessDamage = int.MinValue;
    //     foreach(var (b, a) in _blockerAttacker){
    //         // In the same blocker group -> Do normal damage
    //         if (!prevA.Equals(a)) {
    //             if(DealTrampleDamage(prevA, excessDamage)) yield return new WaitForSeconds(SorsTimings.combatClash);
    //             excessDamage = int.MinValue;
    //             _attackerTarget.Remove(prevA);
    //         }
    //         prevA = a;

    //         a.RpcSetCombatHighlight();
    //         b.RpcSetCombatHighlight();
    //         if (excessDamage == int.MinValue) excessDamage = CombatClash(a, b, a.Attack);
    //         else excessDamage = CombatClash(a, b, excessDamage);

    //         yield return new WaitForSeconds(SorsTimings.combatClash);
    //     }

    //     if (DealTrampleDamage(prevA, excessDamage))
    //         yield return new WaitForSeconds(SorsTimings.combatClash);
    //     _attackerTarget.Remove(prevA);

    //     StartCoroutine(Unblocked());
    // }

    // private IEnumerator Unblocked()
    // {
    //     print($"Unblocked creatures after blocks : {_attackerTarget.Count}");

    //     foreach(var (a, t) in _attackerTarget){

    //         a.RpcSetCombatHighlight();
    //         t.RpcSetCombatHighlight(); 

    //         EntityDealsDamage(a, t, a.Attack);

    //         yield return new WaitForSeconds(SorsTimings.combatClash+1f);
    //     }

    //     UpdateCombatState(CombatState.CleanUp);
    // }
    #endregion

    #region Dealing Damage

    // Calculate the outcome of a combat clash between two creature entities, considering their keywords and attack damage. Returns the remaining attack damage after the clash.
    private int CombatClash(CreatureEntity attacker, CreatureEntity blocker, int attackDamage)
    {
        print($"CombatClash: {attacker.Title} vs {blocker.Title} with {attackDamage} damage");
        var attackerKw = attacker.GetKeywords();
        var blockerKw = blocker.GetKeywords();
        var blockerHealth = blocker.Health;

        // XOR: Neither or both have first strike
        if( ! attackerKw.Contains(Keywords.First_Strike)
            ^ blockerKw.Contains(Keywords.First_Strike))
        {
            EntityDealsDamage(attacker, blocker, attackDamage);
            EntityDealsDamage(blocker, attacker, blocker.Attack);
        }

        // Only attacker has first strike, need to track trample
        if (attackerKw.Contains(Keywords.First_Strike))
        {
            EntityDealsDamage(attacker, blocker, attackDamage);
            if(blocker.Health > 0) EntityDealsDamage(blocker, attacker, blocker.Attack);
        }

        // Only blocker has first strike
        if (blockerKw.Contains(Keywords.First_Strike))
        {
            EntityDealsDamage(blocker, attacker, blocker.Attack);
            if(attacker.Health > 0) EntityDealsDamage(attacker, blocker, attackDamage);
        }

        print ($"Remaining damage: {attackDamage - blockerHealth}");
        return attackDamage - blockerHealth;
    }

    private int EvaluateClashDamage(CreatureEntity attacker, CreatureEntity blocker, int attackDamage)
    {
        print($"CombatClash: {attacker.Title} vs {blocker.Title} with {attackDamage} damage");
        var attackerKw = attacker.GetKeywords();
        var blockerKw = blocker.GetKeywords();

        // XOR: Neither or both have first strike
        if( ! attackerKw.Contains(Keywords.First_Strike)
            ^ blockerKw.Contains(Keywords.First_Strike))
        {
            _clashes.Add(new CombatClash(attacker, blocker, attackDamage));
            _clashes.Add(new CombatClash(blocker, attacker, blocker.Attack));
        }

        // Only attacker has first strike, need to track trample
        if (attackerKw.Contains(Keywords.First_Strike))
        {
            _clashes.Add(new CombatClash(attacker, blocker, attackDamage));
            if(blocker.Health - attackDamage > 0) _clashes.Add(new CombatClash(blocker, attacker, blocker.Attack));
        }

        // Only blocker has first strike
        if (blockerKw.Contains(Keywords.First_Strike))
        {
            _clashes.Add(new CombatClash(blocker, attacker, blocker.Attack));
            if(attacker.Health - blocker.Attack > 0) _clashes.Add(new CombatClash(attacker, blocker, attackDamage));
        }

        return blocker.Health - attackDamage;
    }

    private bool CheckTrampleDamage(CreatureEntity attacker, int excessDamage)
    {
        if (!attacker.GetKeywords().Contains(Keywords.Trample)) return false;

        print($"Trample of {attacker.Title} with {excessDamage} excess damage");
        var target = _attackerTarget[attacker];
        _clashes.Add(new CombatClash(attacker, target, excessDamage));

        return true;
    }

    private bool DealTrampleDamage(CreatureEntity attacker, int excessDamage)
    {
        if (!attacker.GetKeywords().Contains(Keywords.Trample)) return false;
        print($"Trample of {attacker.Title} with {excessDamage} excess damage");

        var target = _attackerTarget[attacker];

        target.RpcSetCombatHighlight();
        EntityDealsDamage(attacker, target, excessDamage);

        return true;
    }
    #endregion

    public void EntityDealsDamage(CreatureEntity a, BattleZoneEntity t, int damage){
        _playerInterfaceManager.RpcLog($"{a.Title} deals {damage - t.Health} damage to {t.Title}", LogType.CombatClash);
        t.EntityTakesDamage(damage, a.GetKeywords().Contains(Keywords.Deathtouch));
    }

    public IEnumerator CombatCleanUp(bool forced)
    {
        _readyPlayers.Clear();
        _attackerTarget.Clear();
        _blockerAttacker.Clear();
        
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
