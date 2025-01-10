using System.Collections.Generic;
using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class TurnStateManager : MonoBehaviour
{
    // Add getters/setters for properties needed by states
    public int TurnNumber => GameManager.Instance.turnNumber;
    public PlayerInterfaceManager Logger => PlayerInterfaceManager.Instance;
    public List<int> ReadyPlayers;
    public int NumberOfPlayers;


    private PlayerInterfaceManager _logger;
    [SerializeField] private AbilityQueue _abilityQueue;
    private Phase _currentPhase;
    public Queue<Phase> PhasesToPlay;
    public Dictionary<PlayerManager, TurnState[]> PlayerPhaseChoices = new();
    public static event Action<TurnState> OnTurnStateChanged;


    public void TransitionToState(Phase nextPhase)
    {
        _currentPhase?.ExitState();
        _currentPhase = nextPhase;
        _currentPhase.EnterState();
    }

    public void NextPhase()
    {
        var nextPhase = PhasesToPlay.Dequeue();
        OnTurnStateChanged?.Invoke(nextPhase.turnState);

        AsyncAwaitQueue(SorsTimings.waitShort)
            .ContinueWith(() => {
                TransitionToState(nextPhase);
                _logger.RpcLog(nextPhase.turnState);
            })
            .Forget();
    }

    private async UniTask AsyncAwaitQueue(int delayMiliseconds)
    {
        await UniTask.Delay(delayMiliseconds);

        // Waiting for AbilityQueue to finish resolving Buy triggers
        await _abilityQueue.Resolve();
    }
}
