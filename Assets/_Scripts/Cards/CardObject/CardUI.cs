using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class CardUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _title;
    [SerializeField] private TMP_Text _description;
    [SerializeField] private TMP_Text _keywords;
    [SerializeField] private TMP_Text _cost;
    [SerializeField] private TMP_Text _attack;
    [SerializeField] private TMP_Text _health;
    [SerializeField] private TMP_Text _points;
    [SerializeField] private TMP_Text _moneyValue;
    [SerializeField] private Image _image;
    [SerializeField] private Image _highlight;    
    [SerializeField] private GameObject _front;
    [SerializeField] private GameObject _creatureUi;
    [SerializeField] private GameObject _moneyUi;
    [SerializeField] private Image _border;

    public void SetCardUI(CardInfo cardInfo){

        _title.text = cardInfo.title;
        _cost.text = cardInfo.cost.ToString();
        
        if (cardInfo.type == CardType.Creature){
            _image.sprite = Resources.Load<Sprite>("Sprites/Cards/Creature/creature");
            _attack.text = cardInfo.attack.ToString();
            _health.text = cardInfo.health.ToString();
            _points.text = cardInfo.points.ToString();
            _description.text = cardInfo.description;
            _keywords.text = string.Join(", ", cardInfo.keywordAbilities.ConvertAll(f => f.ToString()));
            _creatureUi.SetActive(true);
        } else if (cardInfo.type == CardType.Money) {
            _image.sprite = Resources.Load<Sprite>("Sprites/Cards/Money/" + cardInfo.title);
            _moneyValue.text = cardInfo.moneyValue.ToString();
            _moneyUi.SetActive(true);
        } else if (cardInfo.type == CardType.Technology) {
            _image.sprite = Resources.Load<Sprite>("Sprites/Cards/Development/development");
            _points.text = cardInfo.points.ToString();
            _health.text = cardInfo.health.ToString();
            _description.text = cardInfo.description;
        }

        HighlightReset();
    }

    public void CardBackUp(){
        _front.SetActive(false);
        // _border.enabled = false;
    }
    public void CardFrontUp(){
        _front.SetActive(true);
        // _border.enabled = true;
    }

    public void Highlight(bool active, Color color){
        // if(!_highlight) return;

        _highlight.color = color;
        _highlight.enabled = active;
    }
    
    private void HighlightReset(){
        _highlight.color = SorsColors.standardHighlight;
        _highlight.enabled = false;
    }
}
