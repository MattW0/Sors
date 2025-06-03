using System;
using Mirror;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public interface IPlayerCommand
{
    void Execute();
    void Undo();
    string Serialize();

    // Optional alternative to Execute that uses injected dependencies
    void ExecuteWith(CardMover mover);
}

public static class CommandFactory
{
    public static IPlayerCommand Deserialize(string data)
    {
        var parts = data.Split(':');
        var type = parts[0];
        var payload = parts.Length > 1 ? parts[1] : "";

        switch (type)
        {
            case "PlayMoney":
                int cardId = int.Parse(payload);
                // Card card = state.GetCardById(cardId);
                // return new PlayMoneyCardCommand(card, player);
                return new PlayMoneyCardCommand();

            default:
                throw new Exception("Unknown command: " + data);
        }
    }
}

public class GameNetworkManager : NetworkBehaviour
{
    // private GameState gameState;
    // private Player localPlayer;

    private List<IPlayerCommand> localCommandBuffer = new();

    public void ExecuteCommandLocally(IPlayerCommand cmd)
    {
        cmd.Execute();
        localCommandBuffer.Add(cmd);
    }

    public void UndoLast()
    {
        if (localCommandBuffer.Count > 0)
        {
            var last = localCommandBuffer[^1];
            last.Undo();
            localCommandBuffer.RemoveAt(localCommandBuffer.Count - 1);
        }
    }

    public void ConfirmTurn()
    {
        var serializedCommands = localCommandBuffer.Select(c => c.Serialize()).ToList();
        CmdSubmitTurn(serializedCommands);
        localCommandBuffer.Clear();
    }

    [Command]
    private void CmdSubmitTurn(List<string> commands)
    {
        // Store these per-player on the server until both players submit
        // For demo, just echo them back:
        TargetRevealCommands(connectionToClient, commands);
    }

    [TargetRpc]
    private void TargetRevealCommands(NetworkConnection target, List<string> commands)
    {
        foreach (var cmdStr in commands)
        {
            var cmd = CommandFactory.Deserialize(cmdStr);
            cmd.Execute(); // Reveal synced version
        }
    }
}