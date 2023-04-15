using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Linq;

public class DetailCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    [Header("Card Stats")]
    [SerializeField] private TMP_Text _title;
    [SerializeField] private TMP_Text _description;
    [SerializeField] private TMP_Text _cost;
    [SerializeField] private TMP_Text _attack;
    [SerializeField] private TMP_Text _health;
    [SerializeField] private TMP_Text _points;
    [SerializeField] private TMP_Text _moneyValue;
    [SerializeField] private GameObject _creatureUi;
    [SerializeField] private GameObject _moneyUi;

    [Header("Card UI")]
    [SerializeField] private Image _image;
    [SerializeField] private Image _highlight;
    [SerializeField] private Color highlightColor = Color.white;
    private Canvas _tempCanvas;
    private GraphicRaycaster _tempRaycaster;
    private TurnState _state;
    
    public void SetCardUI(CardInfo cardInfo){
        _title.text = cardInfo.title;
        _cost.text = cardInfo.cost.ToString();
        
        if (cardInfo.isCreature){
            _description.text = cardInfo.keyword_abilities.ToString();
            _attack.text = cardInfo.attack.ToString();
            _health.text = cardInfo.health.ToString();
            _points.text = cardInfo.points.ToString();
            _description.text = string.Join(" ", cardInfo.keyword_abilities.ConvertAll(f => f.ToString()));
            _creatureUi.SetActive(true);

        } else {
            _image.sprite = Resources.Load<Sprite>("Sprites/Money/" + cardInfo.title);
            _moneyValue.text = cardInfo.moneyValue.ToString();
            _moneyUi.SetActive(true);
        }
    }

    public void SetCardState(TurnState state) {
        _state = state;
        
        if(state == TurnState.Discard) _highlight.color = Color.yellow;
        else if(state == TurnState.Deploy) _highlight.color = Color.cyan;
        else if(state == TurnState.Trash) _highlight.color = Color.red;
    }

    public void OnPointerEnter(PointerEventData eventData){
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

    public void EnableHighlight() => _highlight.enabled = true;
    public void DisableHighlight() => _highlight.enabled = false;
}
