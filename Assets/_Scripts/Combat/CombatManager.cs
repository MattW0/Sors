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

    private Dictionary<CreatureEntity, BattleZoneEntity> _attackerTarget = new ();
    private Dictionary<CreatureEntity, CreatureEntity> _blockerAttacker = new ();
    
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

    public void PlayerChoosesTargetToAttack(BattleZoneEntity target, List<CreatureEntity> attackers)
    {
        foreach(var a in attackers) _attackerTarget.Add(a, target);

        var attackingPlayerConn = attackers[0].Owner.connectionToClient;

        foreach (var attacker in attackers){
            // Locally shows attackers (freezes arrows to target)
            attacker.TargetDeclaredAttack(attackingPlayerConn, target);
        }
    }

    private void PlayerDeclaredAttackers(PlayerManager player)
    {
        // print($"Player {player.PlayerName} declared attackers");
        _readyPlayers.Add(player);
        if (_readyPlayers.Count != _gameManager.players.Count) return;
        _readyPlayers.Clear();

        _playerInterfaceManager.RpcLog(" --- Attackers Declared ---", LogType.Standard);
        foreach(var (a, t) in _attackerTarget)
        {
            a.RpcDeclaredAttack(t);
            _playerInterfaceManager.RpcLog($"  - {a.Title} attacks {t.Title}", LogType.Standard);
        }

        UpdateCombatState(CombatState.Blockers);
    }

    public void PlayerChoosesAttackerToBlock(CreatureEntity attacker, List<CreatureEntity> blockers)
    {
        foreach(var b in blockers) _blockerAttacker.Add(b, attacker);

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
        foreach (var (b, a) in _blockerAttacker)
        {
            b.RpcDeclaredBlock(a);
            _playerInterfaceManager.RpcLog($"  - {b.Title} blocks {a.Title}", LogType.Standard);
        }
        UpdateCombatState(CombatState.Damage);
    }

    #region Damage
    private void ResolveDamage(){
        // Skip damage logic if there are no attackers 
        if (_blockerAttacker.Keys.Count == 0){
            UpdateCombatState(CombatState.CleanUp);
            return;
        }
        
        StartCoroutine(Blocks());
    }

    private IEnumerator Blocks(){

        print($"Blocked creatures : {_blockerAttacker.Count}");
        print($"Unlocked creatures pre blocks : {_attackerTarget.Count}");

        int totalBlockerHealth = 0;
        CreatureEntity aPrev = null;
        foreach(var (b, a) in _blockerAttacker){
            _playerInterfaceManager.RpcLog($"Clashing creatures: {a.Title} vs {b.Title}", LogType.CombatClash);

            // Changing group of blockers
            if(a != aPrev){
                CheckTrample(a, totalBlockerHealth);
                totalBlockerHealth = 0;
            }

            a.RpcSetCombatHighlight();
            b.RpcSetCombatHighlight();
            CombatClash(a, b);

            totalBlockerHealth += b.Health;
            _attackerTarget.Remove(a);

            yield return new WaitForSeconds(combatDamageWaitTime);
        }
        StartCoroutine(Unblocked());
    }

    private IEnumerator Unblocked(){

        print($"Unblocked creatures after blocks : {_attackerTarget.Count}");

        var playerAttackers = new List<CreatureEntity>();
        foreach(var (a, t) in _attackerTarget){
            _playerInterfaceManager.RpcLog($"{a.Title} attacks {t.Title}", LogType.CombatClash);

            if(t.cardType == CardType.Player){
                playerAttackers.Add(a);
                continue;
            }

            a.RpcSetCombatHighlight();
            // TODO: Move highlight to BZE
            // t.RpcSetCombatHighlight(); 
            CombatClash(a, t);

            yield return new WaitForSeconds(combatDamageWaitTime);
        }

        print($"Creatures attacking player(s) : {playerAttackers.Count}");
        StartCoroutine(PlayerDamage(playerAttackers));
        yield return null;
    }

    private IEnumerator PlayerDamage(List<CreatureEntity> playerAttackers)
    {
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
    private void CombatClash(CreatureEntity attacker, BattleZoneEntity blocker){

        var firstStrike = CheckFirststrike(attacker, blocker);
        if (firstStrike) return; // Damage already happens in CheckFirstStrike

        var attackerKw = attacker.GetKeywords();
        var blockerKw = new List<Keywords>();
        if (blocker.Creature) blockerKw = blocker.Creature.GetKeywords();

        blocker.TakesDamage(attacker.Attack, attackerKw.Contains(Keywords.Deathtouch));
        attacker.TakesDamage(blocker.Attack, blockerKw.Contains(Keywords.Deathtouch));
    }

    private bool CheckFirststrike(CreatureEntity attacker, BattleZoneEntity blocker)
    {
        var attackerKw = attacker.GetKeywords();
        var blockerKw = new List<Keywords>();
        if (blocker.Creature) blockerKw = blocker.Creature.GetKeywords();

        // XOR: return if none or both have first strike
        if( ! attackerKw.Contains(Keywords.First_Strike)
            ^ blockerKw.Contains(Keywords.First_Strike)){
            return false;
        }

        // Attacker has first strike
        if (attackerKw.Contains(Keywords.First_Strike))
        {
            blocker.TakesDamage(attacker.Attack, attackerKw.Contains(Keywords.Deathtouch));
            if(blocker.Health > 0) attacker.TakesDamage(blocker.Attack, blockerKw.Contains(Keywords.Deathtouch));
            return true;
        }
        

        if (blockerKw.Contains(Keywords.First_Strike))
        {
            attacker.TakesDamage(blocker.Attack, blockerKw.Contains(Keywords.Deathtouch));
            if(attacker.Health > 0) blocker.TakesDamage(attacker.Attack, attackerKw.Contains(Keywords.Deathtouch));
            return true;
        }

        return false;
    }

    private void CheckTrample(CreatureEntity attacker, int totalBlockerHealth )
    {
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
        _attackerTarget.Clear();
        _blockerAttacker.Clear();
        _boardManager.CombatCleanUp();
        
        UpdateCombatState(CombatState.Idle);
        if(!forced) _turnManager.CombatCleanUp();
    }

    public void PlayerPressedReadyButton(PlayerManager player) => _boardManager.PlayerPressedReadyButton(player);
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
