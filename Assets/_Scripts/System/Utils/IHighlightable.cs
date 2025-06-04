public interface IHighlightable
{
    bool TooltipDisabled { get; set; }
    public void Highlight(float alpha, float fadeDuration);
    public void Disable(float fadeDuration);
}
