using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EntityUI : MonoBehaviour, IPointerClickHandler
{
    private CardZoomView _cardZoomView;
    [SerializeField] private Image effectHighlight;

    [Header("Entity Stats")]
    private CardInfo _cardInfo;
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text health;
    [SerializeField] private TMP_Text attack;
    [SerializeField] private TMP_Text points;

    [Header("Body")]
    [SerializeField] private TMP_Text keywords;
    [SerializeField] private TMP_Text description;

    private void Start(){
        _cardZoomView = CardZoomView.Instance;
    }

    public void SetEntityUI(CardInfo cardInfo){
        _cardInfo = cardInfo;

        // Set card stats
        title.text = cardInfo.title;
        health.text = cardInfo.health.ToString();
        description.text = cardInfo.description;

        if(cardInfo.type == CardType.Creature){
            attack.text = cardInfo.attack.ToString();
            keywords.text = string.Join(", ", cardInfo.keywordAbilities.ConvertAll(f => f.ToString()));
        } else if (cardInfo.type == CardType.Technology){
            points.text = cardInfo.points.ToString();
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        // Right click to zoom only
        if (eventData.button != PointerEventData.InputButton.Right) return;
        
        _cardZoomView.ZoomCard(_cardInfo);
    }

    public void SetHealth(int newValue) => health.text = newValue.ToString();
    public void SetAttack(int newValue) => attack.text = newValue.ToString();
    public void SetPoints(int newValue) => points.text = newValue.ToString();

    public void Highlight(bool enabled, Color color){
        effectHighlight.enabled = enabled;
        effectHighlight.color = color;
    }
    public void DisableHighlight() => effectHighlight.enabled = false;
}
