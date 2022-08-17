using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CardPileUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _cardNumber;
    [SerializeField] private Transform _cardHolder;

    // Should use event to only fire when card pile changes
    private void Update(){
        _cardNumber.text = _cardHolder.childCount.ToString();
    }
}
