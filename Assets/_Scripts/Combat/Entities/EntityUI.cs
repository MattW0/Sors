using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class EntityUI : MonoBehaviour
{
    [SerializeField] private Image effectHighlight;

    [Header("Entity Stats")]
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text health;
    [SerializeField] private TMP_Text attack;
    [SerializeField] private TMP_Text points;

    [Header("Body")]
    [SerializeField] private TMP_Text keywords;
    [SerializeField] private TMP_Text description;


    public void SetEntityUI(CardInfo cardInfo)
    {
        // Set card stats
        title.text = cardInfo.title;
        health.text = cardInfo.health.ToString();
        description.text = cardInfo.description;

        if(cardInfo.type == CardType.Creature){
            attack.text = cardInfo.attack.ToString();
            keywords.text = string.Join(", ", cardInfo.keywordAbilities.ConvertAll(f => f.ToString()));
        } else if (cardInfo.type == CardType.Development){
            points.text = cardInfo.points.ToString();
        }
    }

    public void SetHealth(int newValue) => health.text = newValue.ToString();
    public void SetAttack(int newValue) => attack.text = newValue.ToString();
    public void SetPoints(int newValue) => points.text = newValue.ToString();

    public void EffectHighlight(bool value) => effectHighlight.enabled = value;
}
