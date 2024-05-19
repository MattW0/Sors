using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DetailCard : MonoBehaviour
{
    private CardInfo _cardInfo;
    [SerializeField] private DetailCardUI _ui;
    
    public void SetCardUI(CardInfo card) 
    {
        _cardInfo = card;
        _ui.SetCardUI(card);
    }

    public void DisableFocus() => _ui.DisableFocus();

}