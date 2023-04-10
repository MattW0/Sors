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
    
    public void SetCardUI(CardInfo cardInfo){
        _title.text = cardInfo.title;
        _cost.text = cardInfo.cost.ToString();

        _highlight.color = highlightColor;
        
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Puts the card on top of others
        _tempCanvas = gameObject.AddComponent<Canvas>();
        _tempCanvas.overrideSorting = true;
        _tempCanvas.sortingOrder = 1;
        _tempRaycaster = gameObject.AddComponent<GraphicRaycaster>();
        
        _highlight.enabled = true;
        // _highlight.DOColor(standardHighlight, 0.2f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Removes focus from the card
        Destroy(_tempRaycaster);
        Destroy(_tempCanvas);
        
        _highlight.enabled = false;
        // _highlight.DOColor(standardHighlight, 0.2f);
    }
}
