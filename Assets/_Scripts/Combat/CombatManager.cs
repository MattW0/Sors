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
    public static event Action OnDeclareAttackers;
    public static event Action OnDeclareBlockers;

    private Dictionary<BattleZoneEntity, List<BattleZoneEntity>> _attackersBlockers = new ();
    private List<BattleZoneEntity> _unblockedAttackers = new();
    
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
                OnDeclareAttackers?.Invoke();
                break;
            case CombatState.Blockers:
                OnDeclareBlockers?.Invoke();
                break;
            case CombatState.Damage:
                ResolveDamage();
                break;
            case CombatState.CleanUp:
                ResolveCombat();
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

    private void PlayerDeclaredAttackers(PlayerManager player)
    {
        _readyPlayers.Add(player);
        if (_readyPlayers.Count != _gameManager.players.Count) return;
        
        // tracking which entity blocks which attackers
        foreach (var attacker in _boardManager.boardAttackers)
        {
            _attackersBlockers.Add(attacker, new List<BattleZoneEntity>());
        }
        
        _boardManager.ShowOpponentAttackers();
        _readyPlayers.Clear();
        _playerInterfaceManager.RpcLog("<color=#42000c> --- Attackers declared --- </color>");
        UpdateCombatState(CombatState.Blockers);
    }

    public void PlayerChoosesAttackerToBlock(BattleZoneEntity attacker, List<BattleZoneEntity> blockers)
    {
        _attackersBlockers[attacker].AddRange(blockers);
        foreach (var blocker in blockers)
        {
            blocker.RpcBlockerDeclared(attacker);
        }
    }

    private void PlayerDeclaredBlockers(PlayerManager player)
    {
        _readyPlayers.Add(player);
        if (_readyPlayers.Count != _gameManager.players.Count) return;
        
        _playerInterfaceManager.RpcLog("<color=#420028> --- Blockers declared --- </color>");
        ShowAllBlockers();
        UpdateCombatState(CombatState.Damage);
    }

    private void ShowAllBlockers()
    {
        _unblockedAttackers.AddRange(_boardManager.boardAttackers);

        foreach (var entry in _attackersBlockers)
        {
            var attacker = entry.Key;
            var blockers = entry.Value;
            
            // there are no blockers -> keep in list
            if (blockers.Count == 0) continue;
            _unblockedAttackers.Remove(attacker);
            
            attacker.RpcShowOpponentsBlockers(blockers);
        }
    }

    #region Damage
    private void ResolveDamage()
    {
        // Skip damage logic if there are no attackers 
        if (_attackersBlockers.Keys.Count == 0)
        {
            UpdateCombatState(CombatState.CleanUp);
            return;
        }
        
        // creature dmg, player dmg, then update CombatState
        StartCoroutine(DealDamage());
    }
    
    private IEnumerator DealDamage()
    {
        // Waiting to show blockers
        yield return new WaitForSeconds(1f);
        
        foreach (var attacker in _boardManager.boardAttackers)
        { // foreach attacker, deal damage to each blocker

            CombatClash(attacker);
            yield return new WaitForSeconds(combatDamageWaitTime);
        }
        StartCoroutine(PlayerDamage());
    }
    
    private void CombatClash(BattleZoneEntity attacker)
    {
        bool attackerfirstdeath = false;
        int totalBlockerHealth = 0;
        foreach (var blocker in _attackersBlockers[attacker])
            {
                _playerInterfaceManager.RpcLog("Clashing creatures: " + attacker.Title + " vs " + blocker.Title + "");
                attackerfirstdeath = CheckFirststrike(attacker, blocker);
                if (attacker.Health > 0 && blocker.Health > 0)
                { 
                    totalBlockerHealth += blocker.Health;
                    Block(attacker, blocker);        
                }   
            }
        if (!attackerfirstdeath) CheckTrample(attacker, totalBlockerHealth);
    }

    private IEnumerator PlayerDamage()
    {
        // Skip if in single-player (for debugging)
        if (_gameManager.singlePlayer) {
            UpdateCombatState(CombatState.CleanUp);
            yield return null;
        }

        foreach (var attacker in _unblockedAttackers)
        {
            _playerInterfaceManager.RpcLog("" + attacker.Title + " is unblocked");

            // player takes damage from unblocked creatures
            var targetPlayer = attacker.Target;
            targetPlayer.Health -= attacker.Attack;
            _turnManager.PlayerHealthChanged(targetPlayer, attacker.Attack);
            yield return new WaitForSeconds(combatDamageWaitTime);
        }
        
        UpdateCombatState(CombatState.CleanUp);
    }
    #endregion

    private void ResolveCombat()
    {
        _readyPlayers.Clear();
        _attackersBlockers.Clear();
        _unblockedAttackers.Clear();
        _boardManager.CombatCleanUp();
        
        UpdateCombatState(CombatState.Idle);
        _turnManager.CombatCleanUp();
    }

    private bool CheckFirststrike(BattleZoneEntity attacker, BattleZoneEntity blocker)
    {
        if (attacker._keywordAbilities.Contains(Keywords.First_Strike) && (attacker.Attack >= blocker.Health || attacker._keywordAbilities.Contains(Keywords.Deathtouch)) && !blocker._keywordAbilities.Contains(Keywords.First_Strike))
        {
            blocker.TakesDamage(attacker.Attack, attacker._keywordAbilities.Contains(Keywords.Deathtouch));
        }

        if (blocker._keywordAbilities.Contains(Keywords.First_Strike) && (blocker.Attack >= attacker.Health || blocker._keywordAbilities.Contains(Keywords.Deathtouch)) && !attacker._keywordAbilities.Contains(Keywords.First_Strike))
        {
            attacker.TakesDamage(blocker.Attack, blocker._keywordAbilities.Contains(Keywords.Deathtouch));
            return true;
        }
        return false;
    }

    private void Block(BattleZoneEntity attacker, BattleZoneEntity blocker)
    {
        blocker.TakesDamage(attacker.Attack, attacker._keywordAbilities.Contains(Keywords.Deathtouch));
        attacker.TakesDamage(blocker.Attack, blocker._keywordAbilities.Contains(Keywords.Deathtouch));
    }

    private void CheckTrample(BattleZoneEntity attacker, int tothealth )
    {
        if (attacker._keywordAbilities.Contains(Keywords.Trample))
        {
            var targetPlayer = attacker.Target;
            targetPlayer.Health -= attacker.Attack - tothealth;
            _turnManager.PlayerHealthChanged(targetPlayer, attacker.Attack - tothealth);            
        }
    }

    private void OnDestroy()
    {
        GameManager.OnGameStart -= Prepare;
        BoardManager.OnAttackersDeclared -= PlayerDeclaredAttackers;
        BoardManager.OnBlockersDeclared -= PlayerDeclaredBlockers;
    }
}

public enum CombatState{
    Idle,
    Attackers,
    Blockers,
    Damage,
    CleanUp
}
