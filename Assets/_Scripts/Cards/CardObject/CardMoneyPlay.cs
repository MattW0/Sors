using UnityEngine;
using UnityEngine.EventSystems;

public class CardMoneyPlay : MonoBehaviour, IPointerClickHandler
{
    private PlayerManager _owner;
    private CardStats _cardStats;
    
    private void Awake() {
        _cardStats = gameObject.GetComponent<CardStats>();
        _owner = _cardStats.owner;
    }

    public void OnPointerClick(PointerEventData eventData) {
        // Return if card can't be played (not in hand or no money card)
        if(!_cardStats.IsInteractable) return;
        // Right click to zoom only
        if (eventData.button == PointerEventData.InputButton.Right) return;

        _owner.CmdPlayMoneyCard(gameObject, _cardStats.cardInfo);        
        _cardStats.IsInteractable = false;
    }
}
