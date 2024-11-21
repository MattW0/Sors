using UnityEngine;

public class HandCardUI : CardUI
{
    [SerializeField] private GameObject _front;

    public void CardBackUp(){
        _front.SetActive(false);
        // _border.enabled = false;
    }
    public void CardFrontUp(){
        _front.SetActive(true);
        // _border.enabled = true;
    }

    public void Highlight(HighlightType type)
    {
        // highlight.color = color;
        highlight.enabled = type != HighlightType.None;
    }
}
