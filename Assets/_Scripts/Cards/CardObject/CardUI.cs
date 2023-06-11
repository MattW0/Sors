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
    [SerializeField] private GameObject _front;
    [SerializeField] private GameObject _creatureUi;
    [SerializeField] private GameObject _moneyUi;

    public void SetCardUI(CardInfo cardInfo){

        _title.text = cardInfo.title;
        _cost.text = cardInfo.cost.ToString();
        
        if (cardInfo.type == CardType.Creature){
            _image.sprite = Resources.Load<Sprite>("Sprites/Cards/Creature/creature");
            _description.text = cardInfo.keywordAbilities.ToString();
            _attack.text = cardInfo.attack.ToString();
            _health.text = cardInfo.health.ToString();
            _points.text = cardInfo.points.ToString();
            _description.text = string.Join(" ", cardInfo.keywordAbilities.ConvertAll(f => f.ToString()));
            _creatureUi.SetActive(true);

        } else if (cardInfo.type == CardType.Money) {
            _image.sprite = Resources.Load<Sprite>("Sprites/Cards/Money/" + cardInfo.title);
            _moneyUi.SetActive(true);
        } else if (cardInfo.type == CardType.Development) {
            _image.sprite = Resources.Load<Sprite>("Sprites/Cards/Development/development");
            _points.text = cardInfo.points.ToString();
            _health.text = cardInfo.health.ToString();
        }
    }

    public void CardBackUp() => _front.SetActive(false);
    public void CardFrontUp() => _front.SetActive(true);

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
