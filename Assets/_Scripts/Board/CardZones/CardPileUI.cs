using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class CardPileUI : MonoBehaviour
{
    public Transform ParentTransform { get; set; }
    [SerializeField] private TMP_Text _cardNumber;

    private void Awake() 
    {
        if(_cardNumber != null) CardMover.OnUpdatePileNumbers += UpdateCardPileNumber;
    }

    public void UpdateCardPileNumber() => _cardNumber.text = ParentTransform.childCount.ToString();

    private void OnDestroy() 
    {
		CardMover.OnUpdatePileNumbers -= UpdateCardPileNumber;
	}
}
