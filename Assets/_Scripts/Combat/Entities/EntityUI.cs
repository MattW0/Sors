using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class EntityUI : MonoBehaviour
{
    [Header("Entity Stats")]
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text health;
    [SerializeField] private GameObject attackObject;
    [SerializeField] private TMP_Text attack;
    [SerializeField] private GameObject pointsObject;
    [SerializeField] private TMP_Text points;
    [SerializeField] private List<TMP_Text> keyWords;

    public void SetEntityUI(CardInfo cardInfo)
    {
        // Set card stats
        title.text = cardInfo.title;
        health.text = cardInfo.health.ToString();

        if(cardInfo.type == CardType.Creature){
            attackObject.SetActive(true);
            pointsObject.SetActive(false);
            attack.text = cardInfo.attack.ToString();
        } else if (cardInfo.type == CardType.Development){
            attackObject.SetActive(false);
            pointsObject.SetActive(true);
            points.text = cardInfo.points.ToString();
        }

        // Set keywords
        int i = 0;
        foreach (var kw in cardInfo.keywordAbilities)
        {
            keyWords[i].text = kw.ToString();
            keyWords[i].enabled = true;
            i++;
        }
    }

    public void SetHealth(int newValue) => health.text = newValue.ToString();
    public void SetAttack(int newValue) => attack.text = newValue.ToString();
    public void SetPoints(int newValue) => points.text = newValue.ToString();
}
