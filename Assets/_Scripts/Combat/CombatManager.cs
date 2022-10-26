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
        _playersReady++;
        if (player) _boardManager.PlayerFinishedChoosingAttackers(player);
        
        if (_playersReady != _gameManager.players.Count) return;
        
        print("Blockers declared");
        _playersReady = 0;
        UpdateCombatState(CombatState.Damage);
    }
    
    private void DealDamage()
    {
        foreach (var attacker in _boardManager.attackers)
        {
            // blockers and attackers battle
            foreach (var blocker in _attackersBlockers[attacker])
            {
                blocker.RpcTakesDamage(attacker.attack);
                attacker.RpcTakesDamage(blocker.attack);
                
                // Wait between each damage being dealt
                StartCoroutine(WaitBetweenDamage(combatDamageWaitTime));
            }
            
            if (_attackersBlockers.Keys.Contains(attacker)) continue;
            
            // player takes damage from unblocked creatures
            var targetPlayer = attacker.Target;
            print(targetPlayer.playerName + "takes damage: " + attacker.attack);
            targetPlayer.Health = attacker.attack;

            StartCoroutine(WaitBetweenDamage(combatDamageWaitTime));
        }
        
        // UpdateCombatState(CombatState.CleanUp);
    }
    
    private static IEnumerator WaitBetweenDamage(float waitTime)
    {
        var startTime = Time.time;
        while (startTime < startTime + waitTime) {
            startTime = Time.time;
            yield return null; // wait 1 frame then check the time again...
        }
    }

    private void ResolveCombat()
    {
        print("Combat finished");

        UpdateCombatState(CombatState.Idle);
        _turnManager.CombatCleanUp();
    }
    
    

    public void PlayerIsReady(PlayerManager player)
    {
        switch (state)
        {
            case CombatState.Attackers:
                PlayerDeclaredAttackers();
                break;
            case CombatState.Blockers:
                PlayerDeclaredBlockers(player);
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
