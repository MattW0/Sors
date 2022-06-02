using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class KingdomCard : MonoBehaviour
{
    public TMP_Text title;
    public TMP_Text cost;
    public TMP_Text attack;
    public TMP_Text defense;

    public void SetCard(CardInfo _card)
    {
        print("Setcard");
        title.text = _card.title;
        cost.text = _card.cost.ToString();
        attack.text = _card.attack.ToString();
        defense.text = _card.health.ToString();
    }
}
