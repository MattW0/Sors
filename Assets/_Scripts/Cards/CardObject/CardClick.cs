using UnityEngine;
using UnityEngine.EventSystems;

public class CardClick : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private CardStats cardStats;
    private CardInfo _cardInfo;
    private PlayerManager _owner;
    private CardZoomView _cardZoomView;

    
    private void Start() {
        _cardZoomView = CardZoomView.Instance;

        // only money cards needs owner
        if(cardStats) {
            _owner = cardStats.owner;
            _cardInfo = cardStats.cardInfo;
        }
    }
    public void OnPointerClick(PointerEventData eventData) {
        
        // print($"click card {_cardInfo.title}");
        
        // Right click to preview card only
        if (eventData.button == PointerEventData.InputButton.Right) {
            _cardZoomView.ZoomCard(_cardInfo);
            return;
        }

        // Return if card can't be played (not in hand or no money card)
        if(!cardStats || !cardStats.IsInteractable) return;
        _owner.CmdPlayMoneyCard(gameObject, cardStats.cardInfo);        
        cardStats.IsInteractable = false;
    }

    public void SetCardInfo(CardInfo cardInfo) => _cardInfo = cardInfo;
}
