using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityUtils;

public class CardUI : MonoBehaviour
{
    public CardInfo CardInfo { get ; private set; }

    [Header("Card Properties")]
    [SerializeField] private TMP_Text _title;
    [SerializeField] private TraitsUI _traits;
    [SerializeField] private AbilitiesUI _abilities;
    
    [Header("Stats")]
    [SerializeField] private TMP_Text _cost;
    [SerializeField] private TMP_Text _health;
    [SerializeField] private TMP_Text _attack;
    [SerializeField] private TMP_Text _points;
    [SerializeField] private TMP_Text _moneyValue;

    [Header("UI Elements")]
    public Image highlight;
    [SerializeField] private Image _image;

    protected SorsColors ColorPalette;

    private void Start()
    {
        ColorPalette = UIManager.Instance.ColorPalette;
    }

    public virtual void SetCardUI(CardInfo cardInfo, string spritePath)
    {
        CardInfo = cardInfo;

        _title.text = cardInfo.title;
        _cost.text = cardInfo.cost.ToString();
        _image.sprite = Resources.Load<Sprite>(spritePath);

        if (cardInfo.type == CardType.Money) {
            _moneyValue.text = cardInfo.moneyValue.ToString();
            return;
        }

        // Entity properties
        _health.text = cardInfo.health.ToString();

        // TODO: Pictos
        _abilities.SetAbilities(cardInfo.abilities);

        if (cardInfo.type == CardType.Creature){
            _attack.text = cardInfo.attack.ToString();
            _traits.SetTraits(cardInfo.traits);
        } else if (cardInfo.type == CardType.Technology) {
            _points.text = cardInfo.points.ToString();
        }
    }

    // Currently only used for entities
    public void SetCost(int newValue) => _cost.text = newValue.ToString();
    public void SetHealth(int newValue) => _health.text = newValue.ToString();
    public void SetAttack(int newValue) => _attack.text = newValue.ToString();
    public void SetPoints(int newValue) => _points.text = newValue.ToString();
}
