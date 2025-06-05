using System.Collections.Generic;

public class PlayerTurnContext
{
    public List<int> SelectedCardIds { get; set; } = new();
    public CardInfo? SelectedMarketCard { get; set; }
    public List<TurnState> PhaseChoices { get; set; } = new();
    public List<PrevailOption> PrevailOptions { get; set; } = new();

    public void Reset()
    {
        SelectedCardIds.Clear();
        SelectedMarketCard = null;
        PhaseChoices.Clear();
        PrevailOptions.Clear();
    }
}
