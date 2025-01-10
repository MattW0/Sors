using System.Collections.Generic;

public abstract class Phase
{
    protected TurnStateManager turnManager;
    public List<PlayerManager> players;
    public TurnState turnState;

    public Phase(TurnStateManager manager, List<PlayerManager> p)
    {
        turnManager = manager;
        players = p;
    }

    public abstract void EnterState();
    public abstract void ExitState();
    public abstract void HandlePlayerReady(PlayerManager player);
    public virtual void HandlePlayerSkip(PlayerManager player) { }
}
