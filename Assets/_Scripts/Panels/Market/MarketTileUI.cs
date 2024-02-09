using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MarketTileUI : MonoBehaviour
{
    [Header("General Properties")]
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text description;
    [SerializeField] private Image _image;
    [SerializeField] private Image _highlight;

    [Header("Card Type Specifics")]
    [SerializeField] private TMP_Text attackText;
    [SerializeField] private TMP_Text moneyValueText;
    [SerializeField] private TMP_Text pointsText;
    [SerializeField] private TMP_Text keywordsText;
    [SerializeField] private GameObject keywordsBox;

    public void SetTileUI(CardInfo card){
        title.text = card.title;
        _image.sprite = Resources.Load<Sprite>(card.entitySpritePath);
        // Cost is set via property and changes due to market bonus

        if(card.type == CardType.Money){
            moneyValueText.text = card.moneyValue.ToString();
            return;
        }

        // Creature and Technology
        healthText.text = card.health.ToString();
        description.text = card.description;

        if (card.type == CardType.Technology){
            pointsText.text = card.points.ToString();
        } else if (card.type == CardType.Creature){
            attackText.text = card.attack.ToString();
            if(card.keywordAbilities.Count > 0)
            {
                keywordsBox.SetActive(true);
                keywordsText.text = string.Join(", ", card.keywordAbilities.ConvertAll(f => f.ToString()));
            } else {
                keywordsBox.SetActive(false);
            }
        }
    }

    public void SetCost(int cost) => costText.text = cost.ToString();

    public void Highlight(bool active, Color color = default(Color)){
        _highlight.enabled = active;
        _highlight.color = color;
    }
}
