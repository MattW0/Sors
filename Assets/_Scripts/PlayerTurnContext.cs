using System.Collections.Generic;
using Mirror;

public class PlayerTurnContext
{
    public List<int> SelectedCardIds { get; set; } = new();
    public CardInfo? SelectedCard { get; set; }
    public List<TurnState> PhaseChoices { get; set; } = new();
    public List<PrevailOption> PrevailOptions { get; set; } = new();
    public int CashBuffer { get; set; }

    [Server]
    public void ResetState()
    {
        CashBuffer = 0;
        SelectedCardIds.Clear();
        SelectedCard = null;
    }

    [Server]
    public void Reset()
    {
        ResetState();
        PhaseChoices.Clear();
    }
}
