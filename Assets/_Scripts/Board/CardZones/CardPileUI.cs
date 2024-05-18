using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CardPileUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _cardNumber;
    public void UpdateCardPileNumber(int numberCards){
        if(! _cardNumber) return;
        _cardNumber.text = numberCards.ToString();
    }
}
