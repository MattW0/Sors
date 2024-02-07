using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MarketTileUI : MonoBehaviour
{
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text moneyValueText;
    [SerializeField] private TMP_Text defenseText;
    [SerializeField] private TMP_Text pointsText;
    [SerializeField] private TMP_Text attackText;
    [SerializeField] private TMP_Text description;
    [SerializeField] private TMP_Text keywords;
    [SerializeField] private Image highlight;
    // [SerializeField] private Image image;

    public void SetTileUI(CardInfo card){
        title.text = card.title;

        if(card.type == CardType.Money){
            moneyValueText.text = card.moneyValue.ToString();
            return;
        }

        // Creature and Technology
        defenseText.text = card.health.ToString();
        description.text = card.description;

        if (card.type == CardType.Technology){
            pointsText.text = card.points.ToString();
        } else if (card.type == CardType.Creature){
            attackText.text = card.attack.ToString();
            keywords.text = string.Join(", ", card.keywordAbilities.ConvertAll(f => f.ToString()));
        }
    }

    public void SetCost(int cost) => costText.text = cost.ToString();

    public void Highlight(bool active, Color color = default(Color)){
        highlight.enabled = active;
        highlight.color = color;
    }
}
