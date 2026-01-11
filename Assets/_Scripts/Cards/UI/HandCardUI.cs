using UnityEngine;

public class HandCardUI : CardUI
{
    [SerializeField] private GameObject _back;

    public void CardBackUp()
    {
        _back.SetActive(true);
    }
    public void CardFrontUp()
    {
        _back.SetActive(false);
    }

    public void SetHighlight(Color color)
    {
        highlight.enabled = true;
        highlight.color = color;
    }

    public void DisableHighlight() => highlight.enabled = false;
}
