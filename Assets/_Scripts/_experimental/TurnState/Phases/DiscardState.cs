using System.Collections.Generic;
using UnityEngine;

public class DiscardState : Phase
{
    public int discardPerDrawPhase;
    private readonly Dictionary<PlayerManager, List<CardStats>> _selectedCards = new();

    public DiscardState(TurnStateManager manager, List<PlayerManager> players) : base(manager, players) { }

    public override void EnterState()
    {
        // Start interaction for each player
        foreach (var player in players)
        {
            // Get number of cards to discard
            int nbInteractions = discardPerDrawPhase;

            // Start the discard interaction
            InteractionPanel.Instance.TargetStartCardInteraction(
                player.connectionToClient, 
                player.Cards.hand, 
                TurnState.Discard, 
                nbInteractions
            );
        }
    }

    public override void ExitState()
    {
        // Process discards for each player
        foreach (var (player, cards) in _selectedCards)
        {
            // player.Cards.DiscardSelection();
            turnManager.Logger.RpcLog(player.ID, cards);
        }

        // Reset the interaction panel
        InteractionPanel.Instance.RpcResetPanel();
        turnManager.NextPhase();
    }

    public override void HandlePlayerReady(PlayerManager player)
    {
        if (!turnManager.ReadyPlayers.Contains(player.ID))
            turnManager.ReadyPlayers.Add(player.ID);

        // Log for debugging
        Debug.Log($"    - {player.PlayerName} ready ({turnManager.ReadyPlayers.Count} / {turnManager.NumberOfPlayers})");

        // When all players have made their selections
        if (turnManager.ReadyPlayers.Count >= turnManager.NumberOfPlayers)
        {
        }
    }

    public override void HandlePlayerSkip(PlayerManager player)
    {
        // Add player to skipped list and mark as ready
        // turnManager.SkippedPlayers.Add(player.ID);
        HandlePlayerReady(player);
    }
}