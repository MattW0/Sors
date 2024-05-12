using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardCollectionUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _collectionTitle;
    [SerializeField] private Button _closeButton;

    private void Start()
    {
        _closeButton.onClick.AddListener(OnClose);
    }

    public void OpenCardCollection(CardLocation cardCollectionType, bool ownsCollection)
    {
        var text = ownsCollection ? "" : "Opponent ";
        switch(cardCollectionType){
            case CardLocation.Deck:
                text += "Deck";
                break;
            case CardLocation.Discard:
                text += "Discard";
                break;
            case CardLocation.Hand:
                text += "Hand";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(cardCollectionType), cardCollectionType, null);
        }

        _collectionTitle.text = text;
    }

    private void OnClose() => Destroy(gameObject);
}
