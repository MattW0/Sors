using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class CardPileUI : MonoBehaviour
{
    public Transform CardHolder { get; set; }
    [SerializeField] private TMP_Text _cardNumber;

    private void Awake() {
        CardMover.OnUpdatePileNumbers += UpdateCardPileNumber;
    }

    public void UpdateCardPileNumber(){
        // print("Update card number");
        _cardNumber.text = CardHolder.childCount.ToString();
    }

    private void OnDestroy() {
		CardMover.OnUpdatePileNumbers -= UpdateCardPileNumber;
	}
}
