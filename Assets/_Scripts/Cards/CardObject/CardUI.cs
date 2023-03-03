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
    [SerializeField] private Vector3 attackRotation;
    private const float TappingDuration = 1f;
    
    private GameObject _front;
    private GameObject _back;
    
    private void Awake(){
        _transform = gameObject.transform;
        _transform.eulerAngles = Vector3.zero;
        attackRotation = new Vector3(0, 0, -90);
        
        _front = _transform.Find("CardFront").gameObject;
        _back = _transform.Find("CardBack").gameObject;
    }

    public void SetCardUI(CardInfo cardInfo){
        _title.text = cardInfo.title;
        _cost.text = cardInfo.cost.ToString();
        
        if (cardInfo.isCreature){
            _transform.Find("CardFront/Creature").gameObject.SetActive(true);
            _transform.Find("CardFront/Money").gameObject.SetActive(false);

            _description.text = cardInfo.keyword_abilities.ToString();
            _attack.text = cardInfo.attack.ToString();
            _health.text = cardInfo.health.ToString();
            _points.text = cardInfo.points.ToString();
            _description.text = string.Join(" ", cardInfo.keyword_abilities.ConvertAll(f => f.ToString()));

        } else {
            _transform.Find("CardFront/Money").gameObject.SetActive(true);
            _transform.Find("CardFront/Creature").gameObject.SetActive(false);
            _image.sprite = Resources.Load<Sprite>("Sprites/Money/" + cardInfo.title);
        }
    }
    
    public void TapCreature()
    {
        _transform.DORotate(attackRotation, TappingDuration).OnComplete(HighlightReset);
    }

    public void UntapCreature()
    {
        _transform.DORotate(Vector3.zero, TappingDuration);
    }

    public void CardBackUp()
    {
        _back.SetActive(true);
        _front.SetActive(false);
    }

    public void CardFrontUp()
    {
        _back.SetActive(false);
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
