using System;
using System.Collections.Generic;
using System.Linq;

public class DrawState : Phase
{
    public int drawPerTurn;
    public int extraDraw;
    public DrawState(TurnStateManager manager, List<PlayerManager> players, int nbCardDraw) : base(manager, players) 
    {
        drawPerTurn = nbCardDraw;
    }

    public override void EnterState()
    {
        foreach (var player in players)
        {
            var nbCardDraw = drawPerTurn;
            if (turnManager.PlayerPhaseChoices[player].Contains(TurnState.Draw))
                nbCardDraw += extraDraw;

            player.DrawCards(nbCardDraw);
            turnManager.Logger.RpcLog(player.ID, nbCardDraw);
        }

        turnManager.TransitionToState(new DiscardState(turnManager, players));
    }

    public override void ExitState() { }

    public override void HandlePlayerReady(PlayerManager player) { }
}