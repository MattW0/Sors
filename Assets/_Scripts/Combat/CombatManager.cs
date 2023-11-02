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

    [SerializeField] private float combatDamageWaitTime = 0.8f;
    public static event Action<CombatState> OnCombatStateChanged;

    private Dictionary<BattleZoneEntity, List<CreatureEntity>> _targetsAttackers = new ();
    private Dictionary<CreatureEntity, List<CreatureEntity>> _attackersBlockers = new ();
    
    private GameManager _gameManager;
    private TurnManager _turnManager;
    private BoardManager _boardManager;
    private PlayerInterfaceManager _playerInterfaceManager;
    private List<PlayerManager> _readyPlayers = new();

    private void Awake()
    {
        if (!Instance) Instance = this;
        
        GameManager.OnGameStart += Prepare;
        BoardManager.OnAttackersDeclared += PlayerDeclaredAttackers;
        BoardManager.OnBlockersDeclared += PlayerDeclaredBlockers;
    }

    public void UpdateCombatState(CombatState newState){
        state = newState;
        
        OnCombatStateChanged?.Invoke(state);

        switch(state){
            case CombatState.Idle:
                break;
            case CombatState.Attackers:
                _playerInterfaceManager.RpcLog(" --- Starting Combat --- ", LogType.Combat);
                _boardManager.StartCombatPhase(CombatState.Attackers);
                break;
            case CombatState.Blockers:
                _playerInterfaceManager.RpcLog("   - Attackers declared", LogType.Combat);
                _boardManager.StartCombatPhase(CombatState.Blockers);
                break;
            case CombatState.Damage:
                _playerInterfaceManager.RpcLog("   - Blockers declared", LogType.Combat);
                ResolveDamage();
                break;
            case CombatState.CleanUp:
                ResolveCombat(false);
                break;
            default:
                print("<color=red>Invalid turn state</color>");
                throw new System.ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }

    private void Prepare(int nbPlayers)
    {
        _gameManager = GameManager.Instance;
        _turnManager = TurnManager.Instance;
        _boardManager = BoardManager.Instance;
        _playerInterfaceManager = PlayerInterfaceManager.Instance;
    }

    private void StartCombat(){
        // TODO: Make only player and technologies targetable
        
    }

    [Server]
    public void PlayerChoosesTargetToAttack(BattleZoneEntity target, List<CreatureEntity> attackers)
    {
        if (_targetsAttackers.Keys.Contains(target)) 
            _targetsAttackers[target].AddRange(attackers);
        else 
            _targetsAttackers.Add(target, attackers);

        var attackingPlayerConn = attackers[0].Owner.connectionToClient;

        // For later tracking which entity blocks which attackers
        foreach (var attacker in attackers){
            _attackersBlockers.Add(attacker, new List<CreatureEntity>());

            // Locally shows attackers (freezes arrows to target)
            attacker.TargetDeclaredAttack(attackingPlayerConn, target);
        }
    }

    [Server]
    private void PlayerDeclaredAttackers(PlayerManager player)
    {
        // print($"Player {player.PlayerName} declared attackers");
        _readyPlayers.Add(player);
        if (_readyPlayers.Count != _gameManager.players.Count) return;
        _readyPlayers.Clear();

        _playerInterfaceManager.RpcLog(" --- Attackers Declared ---", LogType.Standard);
        foreach(var (target, attackers) in _targetsAttackers)
        {
            foreach(var a in attackers){
                a.RpcDeclaredAttack(target);
                _playerInterfaceManager.RpcLog($"  - {a.Title} attacks {target.Title}", LogType.Standard);
            }
        }

        UpdateCombatState(CombatState.Blockers);
    }

    public void PlayerChoosesAttackerToBlock(CreatureEntity attacker, List<CreatureEntity> blockers)
    {
        if (_attackersBlockers.Keys.Contains(attacker)) 
            _attackersBlockers[attacker].AddRange(blockers);
        else 
            _attackersBlockers.Add(attacker, blockers);

        var blockingPlayerConn = blockers[0].Owner.connectionToClient;
        
        foreach (var blocker in blockers)
        {
            // TODO: Add UI stuff like this? 
            // blocker.IsBlocking = true;

            // Only shows local blockers
            blocker.TargetDeclaredBlock(blockingPlayerConn, attacker);
        }
    }

    private void PlayerDeclaredBlockers(PlayerManager player)
    {
        _readyPlayers.Add(player);
        if (_readyPlayers.Count != _gameManager.players.Count) return;
        _readyPlayers.Clear();

        _playerInterfaceManager.RpcLog(" --- Blockers Declared ---", LogType.Standard);
        foreach (var (attacker, blockers) in _attackersBlockers)
        {
            foreach (var b in blockers) {
                b.RpcDeclaredBlock(attacker);
                _playerInterfaceManager.RpcLog($"  - {b.Title} blocks {attacker.Title}", LogType.Standard);
            }
        }
        UpdateCombatState(CombatState.Damage);
    }

    #region Damage
    private void ResolveDamage(){
        // Skip damage logic if there are no attackers 
        if (_attackersBlockers.Keys.Count == 0){
            UpdateCombatState(CombatState.CleanUp);
            return;
        }
        
        // creature dmg, player dmg, then update CombatState
        StartCoroutine(DealDamage());
    }
    
    private IEnumerator DealDamage(){
        
        var playerAttackers = new List<CreatureEntity>();
        
        foreach(var (target, attackers) in _targetsAttackers){
            foreach (var attacker in attackers){
            if(_attackersBlockers[attacker].Count == 0) continue;

            attacker.RpcSetCombatHighlight();
            yield return new WaitForSeconds(combatDamageWaitTime);

            int totalBlockerHealth = 0;
            foreach (var blocker in _attackersBlockers[attacker]){
                blocker.RpcSetCombatHighlight();
                _playerInterfaceManager.RpcLog($"Clashing creatures: {attacker.Title} vs {blocker.Title}", LogType.CombatClash);
                
                totalBlockerHealth += blocker.Health;
                CombatClash(attacker, blocker);

                yield return new WaitForSeconds(combatDamageWaitTime);
            }

            CheckTrample(attacker, totalBlockerHealth);
            yield return new WaitForSeconds(combatDamageWaitTime);
            }   
        }
        
        StartCoroutine(PlayerDamage(playerAttackers));
    }

    private IEnumerator PlayerDamage(List<CreatureEntity> playerAttackers)
    {
        // TODO: Redesign with targets

        // Skip if in single-player (for debugging)
        if (_gameManager.singlePlayer) {
            UpdateCombatState(CombatState.CleanUp);
            yield break;
        }

        foreach (var attacker in playerAttackers){
            attacker.RpcSetCombatHighlight();
            _playerInterfaceManager.RpcLog($"{attacker.Title} deals {attacker.Attack} damage", LogType.CombatDamage);

            // player takes damage from unblocked creatures
            var targetPlayer = attacker.Opponent;
            targetPlayer.Health -= attacker.Attack;
            _turnManager.PlayerHealthChanged(targetPlayer, attacker.Attack);
            yield return new WaitForSeconds(combatDamageWaitTime);
        }
        
        UpdateCombatState(CombatState.CleanUp);
    }   
    #endregion

    #region CombatLogic
    private void CombatClash(CreatureEntity attacker, CreatureEntity blocker){

        var firstStrike = CheckFirststrike(attacker, blocker);
        if (firstStrike) return; // Damage already happens in CheckFirstStrike

        var attackerKw = attacker.GetKeywords();
        var blockerKw = blocker.GetKeywords();

        blocker.TakesDamage(attacker.Attack, attackerKw.Contains(Keywords.Deathtouch));
        attacker.TakesDamage(blocker.Attack, blockerKw.Contains(Keywords.Deathtouch));
    }

    private bool CheckFirststrike(CreatureEntity attacker, CreatureEntity blocker){

        var attackerKw = attacker.GetKeywords();
        var blockerKw = blocker.GetKeywords();

        // XOR: return if none or both have first strike
        if( ! attackerKw.Contains(Keywords.First_Strike)
            ^ blockerKw.Contains(Keywords.First_Strike)){
            return false;
        }

        // Attacker has first strike
        if (attackerKw.Contains(Keywords.First_Strike))
            // && (attacker.Attack >= blocker.Health || attackerKw.Contains(Keywords.Deathtouch)))
        {
            blocker.TakesDamage(attacker.Attack, attackerKw.Contains(Keywords.Deathtouch));
            if(blocker.Health > 0) attacker.TakesDamage(blocker.Attack, blockerKw.Contains(Keywords.Deathtouch));
            return true;
        }
        

        if (blockerKw.Contains(Keywords.First_Strike))
            // && (blocker.Attack >= attacker.Health || blockerKw.Contains(Keywords.Deathtouch)))
        {
            attacker.TakesDamage(blocker.Attack, blockerKw.Contains(Keywords.Deathtouch));
            if(attacker.Health > 0) blocker.TakesDamage(attacker.Attack, attackerKw.Contains(Keywords.Deathtouch));
            return true;
        }

        return false;
    }

    private void CheckTrample(CreatureEntity attacker, int totalBlockerHealth ){

        var attackerKw = attacker.GetKeywords();
        if (attacker.Health <= 0 || !attackerKw.Contains(Keywords.Trample)) 
            return;

        var targetPlayer = attacker.Opponent;
        targetPlayer.Health -= attacker.Attack - totalBlockerHealth;
        _turnManager.PlayerHealthChanged(targetPlayer, attacker.Attack - totalBlockerHealth);
    }
    #endregion

    public void ResolveCombat(bool forced){
        _readyPlayers.Clear();
        _attackersBlockers.Clear();
        _boardManager.CombatCleanUp();
        
        UpdateCombatState(CombatState.Idle);
        if(!forced) _turnManager.CombatCleanUp();
    }

    private void OnDestroy(){
        GameManager.OnGameStart -= Prepare;
        BoardManager.OnAttackersDeclared -= PlayerDeclaredAttackers;
        BoardManager.OnBlockersDeclared -= PlayerDeclaredBlockers;
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
