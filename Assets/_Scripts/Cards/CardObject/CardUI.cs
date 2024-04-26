using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    [Header("General Card Properties")]
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _cost;
    [SerializeField] private TMP_Text _health;
    [SerializeField] private TMP_Text _description;
    [SerializeField] private Image _image;

    [Header("Card Type Specific")]
    [SerializeField] private TMP_Text _attack;
    [SerializeField] private TMP_Text _points;
    [SerializeField] private TMP_Text _moneyValue;
    [SerializeField] private TMP_Text _traitsText;
    [SerializeField] private GameObject _traitsBox;

    [Header("UI Elements")]
    public Image highlight;
    public GameObject titleBox;

    public virtual void SetCardUI(CardInfo cardInfo)
    {
        _titleText.text = cardInfo.title;
        _cost.text = cardInfo.cost.ToString();
        _image.sprite = Resources.Load<Sprite>(cardInfo.cardSpritePath);

        if (cardInfo.type == CardType.Money) {
            _moneyValue.text = cardInfo.moneyValue.ToString();
            return;
        }

        // Entity properties
        _health.text = cardInfo.health.ToString();
        _description.text = cardInfo.description;

        if (cardInfo.type == CardType.Creature){
            _attack.text = cardInfo.attack.ToString();
            if(cardInfo.traits.Count > 0){
                _traitsBox.SetActive(true);
                _traitsText.text = string.Join(", ", cardInfo.traits.ConvertAll(f => f.ToString()));
            } else { 
                _traitsBox.SetActive(false);
            }
        } else if (cardInfo.type == CardType.Technology) {
            _points.text = cardInfo.points.ToString();
        }
    }
}
