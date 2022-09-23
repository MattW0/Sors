using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUI : MonoBehaviour {

    [SerializeField] private TMP_Text _title;
    [SerializeField] private TMP_Text _description;
    [SerializeField] private TMP_Text _cost;
    [SerializeField] private TMP_Text _attack;
    [SerializeField] private TMP_Text _health;
    [SerializeField] private Image _image;
    [SerializeField] private Image _highlight;
    
    public Color standardHighlight = Color.white;

    private GameObject _front;
    private GameObject _back;
    
    private void Awake(){
        _front = gameObject.transform.Find("CardFront").gameObject;
        _back = gameObject.transform.Find("CardBack").gameObject;
    }

    public void SetCardUI(CardInfo cardInfo){
        _title.text = cardInfo.title;
        _cost.text = cardInfo.cost.ToString();
        
        if (cardInfo.isCreature){
            gameObject.transform.Find("CardFront/Special").gameObject.SetActive(true);
            _description.text = cardInfo.hash;
            _attack.text = cardInfo.attack.ToString();
            _health.text = cardInfo.health.ToString();
        } else {
            gameObject.transform.Find("CardFront").GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/Copper");
        }
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
        _highlight.color = color;
        _highlight.enabled = active;
    }
    
    public void HighlightReset(){
        _highlight.color = standardHighlight;
        _highlight.enabled = false;
    }
}
