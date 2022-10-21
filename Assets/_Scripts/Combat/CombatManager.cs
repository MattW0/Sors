using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;

public class CombatManager : NetworkBehaviour
{
    public static CombatManager Instance { get; private set; }
    
    [SerializeField] private CombatState state;
    public static event Action<CombatState> OnCombatStateChanged;
    public static event Action OnDeclareAttackers;
    public static event Action OnDeclareBlockers;

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
                DeclaringAttackers();
                break;
            case CombatState.Blockers:
                DeclaringBlockers();
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

    private static void DeclaringAttackers()
    {
        OnDeclareAttackers?.Invoke();
    }

    private void PlayerDeclaredAttackers()
    {
        _playersReady++;
        if (_playersReady != _gameManager.players.Count) return;
        
        print("Attackers declared");
        _playersReady = 0;
        UpdateCombatState(CombatState.Blockers);
    }
    
    private void DeclaringBlockers()
    {
        OnDeclareBlockers?.Invoke();
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
        UpdateCombatState(CombatState.CleanUp);
    }
    
    private void ResolveCombat()
    {
        print("Combat finished");
        
        UpdateCombatState(CombatState.Idle);
        TurnManager.Instance.CombatCleanUp();
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
