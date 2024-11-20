using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AbilityItemUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _effectDescription;
    [SerializeField] private Image _image;
    [SerializeField] private Image _highlight;
    [SerializeField] private Image _overlay;
    private CardInfo _cardInfo;

    public void SetUI(CardInfo cardInfo, Ability ability)
    {
        _cardInfo = cardInfo;
        _titleText.text = cardInfo.title;
        // _cost.text = cardInfo.cost.ToString();
        _image.sprite = Resources.Load<Sprite>(cardInfo.cardSpritePath);

        // Entity properties
        // _health.text = cardInfo.health.ToString();
        _effectDescription.text = ability.ToString();
    }

    internal void SetActive()
    {
        _highlight.enabled = true;
    }

    internal void SetInactive()
    {
        _highlight.enabled = false;
        _overlay.enabled = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        
    }
}
