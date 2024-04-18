using UnityEngine;
using UnityEngine.UI;

public class MarketTileUI : EntityUI
{
    [SerializeField] private Image _overlay;

    public override void Highlight(bool enabled, Color color)
    {
        base.Highlight(enabled, color);
        _overlay.enabled = !enabled;
    }

    public void ShowAsChosen()
    {
        base.Highlight(false, SorsColors.tileSelectable);
        _overlay.enabled = true;
    }
}
