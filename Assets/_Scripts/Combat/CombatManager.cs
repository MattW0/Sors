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
    private int _playersReady;

    private void Awake()
    {
        if (!Instance) Instance = this;
        
        GameManager.OnGameStart += Prepare;
        BoardManager.OnSkipCombatPhase += SkipCombatPhase;
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

    private void Prepare()
    {
        _gameManager = GameManager.Instance;
        _turnManager = TurnManager.Instance;
        _boardManager = BoardManager.Instance;
    }

    private void PlayerDeclaredAttackers(PlayerManager player)
    {
        _playersReady++;
        if (player) _boardManager.PlayerFinishedChoosingAttackers(player);

        if (_playersReady != _gameManager.players.Count) return;
        
        foreach (var attacker in _boardManager.attackers)
        {
            _attackersBlockers.Add(attacker, new List<BattleZoneEntity>());
        }
        
        _playersReady = 0;
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

    private void PlayerDeclaredBlockers()
    {
        _playersReady++;
        if (_playersReady != _gameManager.players.Count) return;
        
        print("Blockers declared");
        ShowAllBlockers();
        _playersReady = 0;
        UpdateCombatState(CombatState.Damage);
    }

    private void ShowAllBlockers()
    {
        _unblockedAttackers.AddRange(_boardManager.attackers);

        foreach (var entry in _attackersBlockers)
        {
            var blockers = entry.Value;
            
            // there are no blockers -> keep in list
            if (blockers.Count == 0) continue;
            _unblockedAttackers.Remove(entry.Key);
            
            entry.Key.RpcShowOpponentsBlockers(blockers);
        }
    }
    
    private void ResolveDamage()
    {
        StartCoroutine(DealDamage());
    }
    
    private IEnumerator DealDamage()
    {
        // Waiting to show blockers
        yield return new WaitForSeconds(2f);
        
        foreach (var attacker in _boardManager.attackers)
        { // foreach attacker, deal damage to each blocker
            foreach (var blocker in _attackersBlockers[attacker])
            {
                blocker.RpcTakesDamage(attacker.Attack);
                attacker.RpcTakesDamage(blocker.Attack);
                
                // print(attacker.Title + ": " + attacker.Health);
                // if (attacker.Health <= 0 && ! _deadEntities.Contains(attacker)) EntityDies(attacker);
                // if (blocker.Health <= 0) EntityDies(blocker);
                 
                yield return new WaitForSeconds(combatDamageWaitTime);
            }
        }
        
        StartCoroutine(PlayerDamage(_unblockedAttackers));
    }
    
    private IEnumerator PlayerDamage(List<BattleZoneEntity> unblockedAttackers)
    {
        foreach (var attacker in unblockedAttackers)
        {
            // player takes damage from unblocked creatures
            var targetPlayer = attacker.Target;
            targetPlayer.Health -= attacker.Attack;
            yield return new WaitForSeconds(combatDamageWaitTime);
        }
        
        UpdateCombatState(CombatState.CleanUp);
    }

    private void ResolveCombat()
    {
        print("Combat finished");
        _boardManager.CombatCleanUp();
        
        UpdateCombatState(CombatState.Idle);
        _turnManager.CombatCleanUp();
    }
    
    public void PlayerIsReady(PlayerManager player)
    {
        switch (state)
        {
            case CombatState.Attackers:
                PlayerDeclaredAttackers(player);
                break;
            case CombatState.Blockers:
                PlayerDeclaredBlockers();
                break;
        }
    }

    private void SkipCombatPhase()
    {
        PlayerIsReady(null);
    }

    private void OnDestroy()
    {
        GameManager.OnGameStart -= Prepare;
        BoardManager.OnSkipCombatPhase -= SkipCombatPhase;
    }
}

public enum CombatState{
    Idle,
    Attackers,
    Blockers,
    Damage,
    CleanUp
}
