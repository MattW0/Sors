using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class DetailCardUI : CardUI, IPointerEnterHandler, IPointerExitHandler {

    private TurnState _state;
    
    [Header("Card UI")]
    private bool _enableFocus = true;
    private Canvas _tempCanvas;
    private GraphicRaycaster _tempRaycaster;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(!_enableFocus) return;

        // Puts the card on top of others
        _tempCanvas = gameObject.AddComponent<Canvas>();
        _tempCanvas.overrideSorting = true;
        _tempCanvas.sortingOrder = 1;
        _tempRaycaster = gameObject.AddComponent<GraphicRaycaster>();
        
        if(_state == TurnState.Deploy) return;
        highlight.enabled = true;
        // _highlight.DOColor(standardHighlight, 0.2f);
    }

    public void OnPointerExit(PointerEventData eventData){
        // Removes focus from the card
        Destroy(_tempRaycaster);
        Destroy(_tempCanvas);
        
        if(_state == TurnState.Deploy) return;
        highlight.enabled = false;
        // _highlight.DOColor(standardHighlight, 0.2f);
    }

    public void DisableFocus() => _enableFocus = false;
    public void EnableHighlight() => highlight.enabled = true;
    public void DisableHighlight() => highlight.enabled = false;
}
