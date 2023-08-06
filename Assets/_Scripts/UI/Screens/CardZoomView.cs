using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardZoomView : MonoBehaviour
{
    public static CardZoomView Instance { get; private set; }
    [SerializeField] private GameObject _cardZoomView;
    [SerializeField] private DetailCard _detailCard;

    private void Awake(){
        Instance = this;
    }

    public void ZoomCard(CardInfo card){
        _cardZoomView.SetActive(true);
        _detailCard.SetCardUI(card);
        _detailCard.DisableFocus();
        // _cardZoomView.GetComponent<CardUI>().SetCardUI(card);
    }

    public void OnClose(){
        _cardZoomView.SetActive(false);
    }
}
