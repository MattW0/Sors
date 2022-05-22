using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;

public class CardUI : NetworkBehaviour {

    public TMP_Text title;
    public TMP_Text description;
    public TMP_Text cost;
    public TMP_Text attack;
    public TMP_Text health;
    public Image image;
    public Image highlight;


    [ClientRpc]
    public void RpcSetCardUI(CardInfo cardInfo)
    {
        title.text = cardInfo.title;
        cost.text = cardInfo.cost.ToString();
        
        if (cardInfo.isCreature){
            gameObject.transform.Find("CardFront/Special").gameObject.SetActive(true);
            description.text = cardInfo.hash;
            attack.text = cardInfo.attack.ToString();
            health.text = cardInfo.health.ToString();
        } else {
            gameObject.transform.Find("CardFront").GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/Copper");
        }

    }

    public void Flip()
    {
        GameObject front = gameObject.transform.Find("CardFront").gameObject;
        GameObject back = gameObject.transform.Find("CardBack").gameObject;

        if (front.activeSelf){
            // Debug.Log("Front side up");
            back.SetActive(true);
            front.SetActive(false);
        } else {
            // Debug.Log("Back side up");
            back.SetActive(false);
            front.SetActive(true);
        }
    }

    public void Highlight(bool active){
        highlight.enabled = active;
    }
}
