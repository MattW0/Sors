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

    private GameManager _gameManager;
    private TurnManager _turnManager;
    private BoardManager _boardManager;
    private int _playersReady;

    private void Awake()
    {
        if (!Instance) Instance = this;
        
        GameManager.OnGameStart += Prepare;
        BoardManager.OnSkipCombatPhase += PlayerIsReady;
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
                DealDamage();
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

    private void PlayerDeclaredAttackers()
    {
        _playersReady++;
        if (_playersReady != _gameManager.players.Count) return;
        
        print("Attackers declared");
        foreach (var attacker in _boardManager.attackers)
        {
            _attackersBlockers.Add(attacker, new List<BattleZoneEntity>());
        }
        
        _playersReady = 0;
        UpdateCombatState(CombatState.Blockers);
    }

    public void PlayerChoosesBlocker(PlayerManager blockingPlayer,
        BattleZoneEntity attacker, List<BattleZoneEntity> blockers)
    {
        _attackersBlockers[attacker] = blockers;
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
        _playersReady = 0;
        UpdateCombatState(CombatState.Damage);
    }
    
    private void DealDamage()
    {
        print("Resolving damage");
        var attackers = _boardManager.attackers;
        print("Total attackers: " + attackers.Count);
        print("Total _attackersBlockers: " + _attackersBlockers.Count);
        // StartCoroutine(ResolveDamage(attackers));
        
        // Wait between each damage being dealt
        foreach (var attacker in attackers)
        {
            // player takes damage
            if (!_attackersBlockers.Keys.Contains(attacker))
            {
                var targetPlayer = attacker.Owner;
                print(targetPlayer.playerName + "takes damage: " + attacker.attack);
                targetPlayer.Health = attacker.attack;
            }

            foreach (var blocker in _attackersBlockers[attacker])
            {
                blocker.RpcTakesDamage(attacker.attack);
                attacker.RpcTakesDamage(blocker.attack);
            }
            
            // WaitForSeconds(combatDamageWaitTime);
        }
        
        UpdateCombatState(CombatState.CleanUp);
    }
    
    private IEnumerator ResolveDamage(List<BattleZoneEntity> attackers) {
        
        

        // Wait and disable
        yield return null;
    }

    private void ResolveCombat()
    {
        print("Combat finished");

        UpdateCombatState(CombatState.Idle);
        _turnManager.CombatCleanUp();
    }
    
    

    public void PlayerIsReady()
    {
        switch (state)
        {
            case CombatState.Attackers:
                PlayerDeclaredAttackers();
                break;
            case CombatState.Blockers:
                PlayerDeclaredBlockers();
                break;
        }
    }

    private void OnDestroy()
    {
        GameManager.OnGameStart -= Prepare;
        BoardManager.OnSkipCombatPhase -= PlayerIsReady;
    }
}

public enum CombatState{
    Idle,
    Attackers,
    Blockers,
    Damage,
    CleanUp
}
