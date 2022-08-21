using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CardPileUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _cardNumber;
    [SerializeField] private Transform _cardHolder;

    private void Awake()
    {
        PlayerManager.OnCardPileChanged += UpdateCardPileNumber;
    }

    // Should use event to only fire when card pile changes, i think?
    private void UpdateCardPileNumber(){
        // print("Updating Card Pile Number");
        _cardNumber.text = _cardHolder.childCount.ToString();
    }

    private void Update(){
        // print("Updating Card Pile Number");
        _cardNumber.text = _cardHolder.childCount.ToString();
    }

    private void Destroy(){
        PlayerManager.OnCardPileChanged -= UpdateCardPileNumber;
    }
}
