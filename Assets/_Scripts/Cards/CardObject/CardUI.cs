using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityUtils;

public class CardUI : MonoBehaviour
{
    [Header("General Card Properties")]
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _cost;
    [SerializeField] private TMP_Text _health;
    [SerializeField] private Image _image;

    [Header("Card Type Specific")]
    [SerializeField] private TMP_Text _attack;
    [SerializeField] private TMP_Text _points;
    [SerializeField] private TMP_Text _moneyValue;
    [SerializeField] private TraitsUI _traits;

    [Header("UI Elements")]
    public Image highlight;
    [SerializeField] private Transform _abilitiesParent;
    [SerializeField] private GameObject _abilityPrefab;

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

        // TODO: Pictos
        _abilitiesParent.DestroyChildren();
        foreach (var ability in cardInfo.abilities)
        {
            var abilityItem = Instantiate(_abilityPrefab, _abilitiesParent);
            // abilityItem.transform.position = Vector3.zero;
            abilityItem.GetComponent<AbilityUI>().SetPictos(ability);
        }
        // _description.text = cardInfo.description;

        if (cardInfo.type == CardType.Creature){
            _attack.text = cardInfo.attack.ToString();
            _traits.SetTraits(cardInfo.traits);
        } else if (cardInfo.type == CardType.Technology) {
            _points.text = cardInfo.points.ToString();
        }
    }
}
