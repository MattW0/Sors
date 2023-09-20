using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

[System.Serializable]
public class KingdomTile : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Kingdom _kingdom;
    private CardZoomView _cardZoomView;
    public CardInfo cardInfo;
    [SerializeField] private KingdomTileUI _ui;
    private int _cost;
    public int Cost{
        get => _cost;
        set{
            _cost = value;
            _ui.SetCost(value);
        }
    }
    
    private bool _isInteractable;
    public bool Interactable {
        get => _isInteractable;
        set {
            if(_alreadyChosen) return;
            _isInteractable = value;
            _ui.Highlight(value, SorsColors.tileSelectable);
        }
    }
    private bool _isSelected;
    private bool IsSelected {
        get => _isSelected;
        set {
            _isSelected = value;
            if(value) {
                _kingdom.PlayerSelectsTile(this);
                _ui.Highlight(true, SorsColors.tileSelected);
            } else {
                _ui.Highlight(true, SorsColors.tileSelectable);
            }
        }
    }

    private bool _alreadyChosen;
    private bool _hovering;

    private void Awake(){
        _kingdom = Kingdom.Instance;
        _cardZoomView = CardZoomView.Instance;
    }

    public void SetTile(CardInfo card){
        cardInfo = card;
        Cost = card.cost;
        _ui.SetTileUI(card);

        if(card.type == CardType.Money || card.type == CardType.Technology){
            Kingdom.OnDevelopPhaseEnded += EndDevelopPhase;
        } else if (card.type == CardType.Creature){
            Kingdom.OnRecruitPhaseEnded += EndRecruitPhase;
        }
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
        KingdomHoverPreview.OnHoverTile(cardInfo);
    }

    public void OnPointerExit(PointerEventData eventData){
        StopAllCoroutines();
        KingdomHoverPreview.OnHoverExit();
    }

    public void OnPointerClick(PointerEventData eventData){

        // Right click to preview card only
        if (eventData.button == PointerEventData.InputButton.Right) {
            _cardZoomView.ZoomCard(cardInfo);
            return;
        }

        if (!_isInteractable) return;

        // Reset highlight and preview in kingdom if 2nd click on selected tile
        if(_isSelected) _kingdom.PlayerDeselectsTile();
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
        _ui.Highlight(true, SorsColors.tilePreviouslySelected);
    }
    
    private void EndDevelopPhase() => ResetTile();
    private void EndRecruitPhase() => ResetTile();
    private void ResetTile(){
        Interactable = false;
        _isSelected = false;
        _alreadyChosen = false;

        // Reset cost (undo bonus)
        Cost = cardInfo.cost;
    }

    private void OnDestroy(){
        Kingdom.OnDevelopPhaseEnded -= EndDevelopPhase;
        Kingdom.OnRecruitPhaseEnded -= EndRecruitPhase;
    }
}
