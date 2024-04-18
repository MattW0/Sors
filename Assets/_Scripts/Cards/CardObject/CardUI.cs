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
    [SerializeField] private TMP_Text _keywordsText;
    [SerializeField] private GameObject _keywordsBox;

    // Could use properites for public access
    // public int Cost { 
    //     get => int.Parse(_cost.text); 
    //     set => _cost.text = value.ToString();
    // }

    [Header("UI Elements")]
    public Image highlight;
    public GameObject titleBox;
    private Vector3 YES_KEYWORDS_TITLE_POSITION = new(0f, -15f, 0f);

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
            if(cardInfo.keywordAbilities.Count > 0){
                _keywordsBox.SetActive(true);
                _keywordsText.text = string.Join(", ", cardInfo.keywordAbilities.ConvertAll(f => f.ToString()));
                // titleBox.transform.localPosition = YES_KEYWORDS_TITLE_POSITION;
            } else { 
                _keywordsBox.SetActive(false);
            }
        } else if (cardInfo.type == CardType.Technology) {
            _points.text = cardInfo.points.ToString();
        }
    }
}
