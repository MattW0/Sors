using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class DetailCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    [Header("Card Stats")]
    [SerializeField] private TMP_Text _title;
    [SerializeField] private TMP_Text _cost;
    [SerializeField] private TMP_Text _attack;
    [SerializeField] private TMP_Text _health;
    [SerializeField] private TMP_Text _points;
    [SerializeField] private TMP_Text _moneyValue;
    [SerializeField] private TMP_Text _description;
    [SerializeField] private TMP_Text _keywords;

    [Header("Card UI")]
    public bool enableFocus = true;
    [SerializeField] private Image _image;
    [SerializeField] private Image _highlight;
    private Canvas _tempCanvas;
    private GraphicRaycaster _tempRaycaster;
    private TurnState _state;
    
    public void SetCardUI(CardInfo cardInfo){
        _title.text = cardInfo.title;
        _cost.text = cardInfo.cost.ToString();
        _image.sprite = Resources.Load<Sprite>(cardInfo.cardSpritePath);

        // Money
        if (cardInfo.type == CardType.Money) {
            _moneyValue.text = cardInfo.moneyValue.ToString();
            return;
        }

        // Entity
        _health.text = cardInfo.health.ToString();
        _points.text = cardInfo.points.ToString();
        _description.text = cardInfo.description;

        if (cardInfo.type == CardType.Creature) {
            _attack.text = cardInfo.attack.ToString();
            _keywords.text = string.Join(", ", cardInfo.keywordAbilities.ConvertAll(f => f.ToString()));
        }
    }

    public void SetCardState(TurnState state) {
        _state = state;
        
        if(state == TurnState.Discard) _highlight.color = SorsColors.discardHighlight;
        else if(state == TurnState.Deploy) _highlight.color = SorsColors.deployHighlight;
        else if(state == TurnState.Trash) _highlight.color = SorsColors.trashHighlight;
    }

    public void OnPointerEnter(PointerEventData eventData){
        if(!enableFocus) return;

        // Puts the card on top of others
        _tempCanvas = gameObject.AddComponent<Canvas>();
        _tempCanvas.overrideSorting = true;
        _tempCanvas.sortingOrder = 1;
        _tempRaycaster = gameObject.AddComponent<GraphicRaycaster>();
        
        if(_state == TurnState.Deploy) return;
        _highlight.enabled = true;
        // _highlight.DOColor(standardHighlight, 0.2f);
    }

    public void OnPointerExit(PointerEventData eventData){
        // Removes focus from the card
        Destroy(_tempRaycaster);
        Destroy(_tempCanvas);
        
        if(_state == TurnState.Deploy) return;
        _highlight.enabled = false;
        // _highlight.DOColor(standardHighlight, 0.2f);
    }

    public void DisableFocus() => enableFocus = false;
    public void EnableHighlight() => _highlight.enabled = true;
    public void DisableHighlight() => _highlight.enabled = false;
}
