using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Linq;

public class CardUI : MonoBehaviour {

    [SerializeField] private TMP_Text _title;
    [SerializeField] private TMP_Text _description;
    [SerializeField] private TMP_Text _cost;
    [SerializeField] private TMP_Text _attack;
    [SerializeField] private TMP_Text _health;
    [SerializeField] private TMP_Text _points;
    [SerializeField] private Image _image;
    [SerializeField] private Image _highlight;
    
    public Color standardHighlight = Color.white;
    private Transform _transform;
    private const float TappingDuration = 1f;
    
    [SerializeField] private GameObject _front;
    // [SerializeField] private GameObject _back;
    [SerializeField] private GameObject _creatureUi;
    [SerializeField] private GameObject _moneyUi;

    public void SetCardUI(CardInfo cardInfo){

        _title.text = cardInfo.title;
        _cost.text = cardInfo.cost.ToString();
        
        if (cardInfo.type == CardType.Creature){
            _description.text = cardInfo.keyword_abilities.ToString();
            _attack.text = cardInfo.attack.ToString();
            _health.text = cardInfo.health.ToString();
            _points.text = cardInfo.points.ToString();
            _description.text = string.Join(" ", cardInfo.keyword_abilities.ConvertAll(f => f.ToString()));
            _creatureUi.SetActive(true);

        } else if (cardInfo.type == CardType.Money) {
            _image.sprite = Resources.Load<Sprite>("Sprites/Money/" + cardInfo.title);
            _moneyUi.SetActive(true);
        } else if (cardInfo.type == CardType.Development) {
            // _description.text = cardInfo.keyword_abilities.ToString();
            // _description.text = string.Join(" ", cardInfo.keyword_abilities.ConvertAll(f => f.ToString()));
        }
    }

    public void CardBackUp()
    {
        _front.SetActive(false);
    }

    public void CardFrontUp()
    {
        _front.SetActive(true);
    }

    public void Highlight(bool active, Color color){
        if(!_highlight) return;

        _highlight.color = color;
        _highlight.enabled = active;
    }
    
    public void HighlightReset(){
        _highlight.color = standardHighlight;
        _highlight.enabled = false;
    }
}
