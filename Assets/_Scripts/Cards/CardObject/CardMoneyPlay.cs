using UnityEngine;
using UnityEngine.EventSystems;

public class CardMoneyPlay : MonoBehaviour, IPointerClickHandler
{
    private PlayerManager _owner;
    [SerializeField] private CardStats _cardStats;
    
    private void Start() {
        // only money cards need stats and owner
        if(_cardStats) _owner = _cardStats.owner;
    }

    public void OnPointerClick(PointerEventData eventData) {
        // Right click to zoom only
        if (eventData.button == PointerEventData.InputButton.Right) return;

        
        // Return if card can't be played (not in hand or no money card)
        if(!_cardStats.IsInteractable) return;

        _owner.CmdPlayMoneyCard(gameObject, _cardStats.cardInfo);        
        _cardStats.IsInteractable = false;
    }
}
