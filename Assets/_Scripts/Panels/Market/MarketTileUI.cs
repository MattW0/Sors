using UnityEngine;

public class MarketTileUI : CardUI
{
    public void SetCost(int cost) => Cost = cost;

    public void Highlight(bool active, Color color = default(Color)){
        highlight.enabled = active;
        highlight.color = color;
    }
}
