using System.Collections.Generic;

public class PlayMoneyCardCommand : IPlayerCommand
{
    private CardStats card;
    private PlayerManager player;
    private List<CardStats> buffer;
    private CardMover mover;

    public PlayMoneyCardCommand()
    {
        // this.card = card;
        // this.player = player;
        // this.buffer = buffer;
        // this.mover = mover;
    }

    public void Execute()
    {
        buffer.Add(card);
        player.LocalCash += card.cardInfo.moneyValue;
        card.SetInteractable(false);
        mover.MoveTo(card.gameObject, true, CardLocation.Hand, CardLocation.MoneyZone);
    }

    public void ExecuteWith(CardMover mover)
    {
        this.mover = mover;
        Execute();
    }

    public void Undo()
    {
        if (!buffer.Contains(card)) return;

        player.LocalCash -= card.cardInfo.moneyValue;
        card.SetInteractable(true);
        mover.MoveTo(card.gameObject, true, CardLocation.MoneyZone, CardLocation.Hand);
        buffer.Remove(card);
    }

    public string Serialize()
    {
        return $"PlayMoney:{card.cardInfo.goID}";
    }
}
