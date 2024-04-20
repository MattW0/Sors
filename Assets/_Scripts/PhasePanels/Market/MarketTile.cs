using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

[System.Serializable]
public class MarketTile : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Market _market;
    private CardZoomView _cardZoomView;
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
            _ui.Highlight(value, SorsColors.tileSelectable);
        }
    }
    private bool _isSelected;
    private bool IsSelected
    {
        get => _isSelected;
        set {
            _isSelected = value;
            if(value) {
                _market.PlayerSelectsTile(this);
                _ui.Highlight(true, SorsColors.tileSelected);
            } else {
                _market.PlayerDeselectsTile();
                _ui.Highlight(true, SorsColors.tileSelectable);
            }
        }
    }

    public int Index { get; private set; }

    private bool _alreadyChosen;

    private void Awake(){
        _market = Market.Instance;
        _cardZoomView = CardZoomView.Instance;
    }

    public void InitializeTile(CardInfo card, int index){
        Index = index;

        SetTile(card);
        
        Market.OnMarketPhaseEnded += EndMarketPhase;
    }

    public void SetTile(CardInfo card)
    {
        cardInfo = card;
        Cost = card.cost;
        _ui.SetEntityUI(card);
    }

    public void SetBonus(int priceReduction){
        if(Cost - priceReduction <= 0) Cost = 0;
        else Cost -= priceReduction;
    }

    public void OnPointerEnter(PointerEventData eventData){
        StopAllCoroutines();
        StartCoroutine(HoverPreviewWaitTimer());
    }

    private IEnumerator HoverPreviewWaitTimer(){
        yield return new WaitForSeconds(0.5f);
        MarketTileHoverPreview.OnHoverTile(cardInfo);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();
        MarketTileHoverPreview.OnHoverExit();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Right click to preview card only
        if (eventData.button == PointerEventData.InputButton.Right) {
            _cardZoomView.ZoomCard(cardInfo);
            return;
        }

        if (!_isInteractable) return;

        // Reset highlight and preview in market if 2nd click on selected tile
        IsSelected = !_isSelected;
    }

    public void ResetSelected(){
        if(!IsSelected) return;
        IsSelected = false;
    }

    public void HasBeenChosen(){
        IsSelected = false;
        Interactable = false;

        _alreadyChosen = true;
        _ui.ShowAsChosen();
    }
    
    private void EndMarketPhase() => ResetTile();
    
    private void ResetTile(){
        Interactable = false;
        _isSelected = false;
        _alreadyChosen = false;

        // Reset cost (undo bonus)
        Cost = cardInfo.cost;
    }

    private void OnDestroy(){
        Market.OnMarketPhaseEnded -= EndMarketPhase;
    }
}
