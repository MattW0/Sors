using UnityEngine;
using System;

[System.Serializable]
public class MarketTile : MonoBehaviour
{
    public CardInfo cardInfo;
    [SerializeField] private MarketTileUI _ui;
    private int _cost;
    public int Cost
    {
        get => _cost;
        set{
            _cost = value;
            _ui.SetCost(value);
        }
    }
    
    private bool _isInteractable;
    public bool Interactable
    {
        get => _isInteractable;
        set {
            if(_alreadyChosen) return;
            _isInteractable = value;
            if (value) _ui.Highlight(HighlightType.InteractionPositive);
            else _ui.Highlight(HighlightType.None);
        }
    }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set {
            _isSelected = value;
            if(value) {
                OnTileSelected?.Invoke(this);
                _ui.Highlight(HighlightType.Default);
            } else {
                OnTileDeselected?.Invoke();
                _ui.Highlight(HighlightType.InteractionPositive);
            }
        }
    }

    public int Index { get; private set; }
    private bool _alreadyChosen;
    public static event Action<MarketTile> OnTileSelected;
    public static event Action OnTileDeselected;


    public void InitializeTile(CardInfo card, int index)
    {
        Index = index;
        SetTile(card);
        Market.OnMarketPhaseEnded += ResetTile;
    }

    public void SetTile(CardInfo card)
    {
        cardInfo = card;
        Cost = card.cost;
        _ui.SetCardUI(card);
    }

    public void SetBonus(int priceReduction)
    {
        if(Cost - priceReduction <= 0) Cost = 0;
        else Cost -= priceReduction;
    }

    public void ResetSelected()
    {
        if(!IsSelected) return;
        IsSelected = false;
    }

    public void HasBeenChosen()
    {
        IsSelected = false;
        Interactable = false;

        _alreadyChosen = true;
        _ui.ShowAsChosen();
    }
    
    private void ResetTile()
    {
        Interactable = false;
        _isSelected = false;
        _alreadyChosen = false;

        // Reset cost (undo bonus)
        Cost = cardInfo.cost;
    }

    private void OnDestroy()
    {
        Market.OnMarketPhaseEnded -= ResetTile;
    }
}
