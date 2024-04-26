using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EntityUI : MonoBehaviour
{
    public CardInfo CardInfo { get ; private set; }

    [Header("Entity Stats")]
    [SerializeField] private TMP_Text _title;
    [SerializeField] private TMP_Text _cost;
    [SerializeField] private TMP_Text _health;
    [SerializeField] private TMP_Text _attack;
    [SerializeField] private TMP_Text _points;
    [SerializeField] private TMP_Text _value;

    [Header("Body")]
    [SerializeField] private Image _image;
    [SerializeField] private Image _highlight;
    [SerializeField] private GameObject _traitsBox;
    [SerializeField] private TMP_Text _traitsText;
    [SerializeField] private TMP_Text _description;

    public void SetEntityUI(CardInfo cardInfo){
        CardInfo = cardInfo;

        // Set card stats
        _title.text = cardInfo.title;
        _cost.text = cardInfo.cost.ToString();
        _health.text = cardInfo.health.ToString();
        _description.text = cardInfo.description;

        _image.sprite = Resources.Load<Sprite>(cardInfo.entitySpritePath);

        if(cardInfo.type == CardType.Creature){
            _attack.text = cardInfo.attack.ToString();
            if(cardInfo.traits.Count > 0){
                _traitsBox.SetActive(true);
                _traitsText.text = string.Join(", ", cardInfo.traits.ConvertAll(f => f.ToString()));
            } else {
                _traitsBox.SetActive(false);
            }
        } else if (cardInfo.type == CardType.Technology){
            _points.text = cardInfo.points.ToString();
        } else if (cardInfo.type == CardType.Money){
            _value.text = cardInfo.moneyValue.ToString();
        }
    }

    public void SetCost(int newValue) => _cost.text = newValue.ToString();
    public void SetHealth(int newValue) => _health.text = newValue.ToString();
    public void SetAttack(int newValue) => _attack.text = newValue.ToString();
    public void SetPoints(int newValue) => _points.text = newValue.ToString();

    public virtual void Highlight(bool enabled, Color color)
    {
        if(_highlight == null) return;

        _highlight.enabled = enabled;
        _highlight.color = color;
    }
    public void DisableHighlight()
    {
        if(_highlight == null) return;

        _highlight.enabled = false;
    } 
}
