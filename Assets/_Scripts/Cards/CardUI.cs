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

    private GameObject _front;
    private GameObject _back;
    
    private void Awake(){
        _front = gameObject.transform.Find("CardFront").gameObject;
        _back = gameObject.transform.Find("CardBack").gameObject;
    }

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

    public void CardBackUp()
    {
        _back.SetActive(true);
        _front.SetActive(false);
    }

    public void CardFrontUp()
    {
        _back.SetActive(false);
        _front.SetActive(true);
    }

    public void Highlight(bool active){
        highlight.enabled = active;
    }
}
