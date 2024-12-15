using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class CardCollectionUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _collectionTitle;
    [SerializeField] private Button _closeButton;
    private CardLocation _cardCollectionType;
    public static event Action<CardLocation> OnCloseCardCollection;

    private void Start()
    {
        _closeButton.onClick.AddListener(Close);
    }

    public void Open(CardLocation cardCollectionType, bool ownsCollection)
    {
        gameObject.SetActive(true);

        var text = ownsCollection ? "" : "Opponent ";
        if (cardCollectionType == CardLocation.Deck) text += "Deck";
        else if (cardCollectionType == CardLocation.Discard) text += "Discard";
        else if (cardCollectionType == CardLocation.Hand) text += "Hand";
        else if (cardCollectionType == CardLocation.MoneyZone) text += "Money Zone";
        else if (cardCollectionType == CardLocation.PlayZone) text += "Play Zone";
        // Nobody owns these collections
        else if (cardCollectionType == CardLocation.Trash) {
            text = "Trash";
        }

        _collectionTitle.text = text;
        _cardCollectionType = cardCollectionType;
    }

    private void Close()
    {
        OnCloseCardCollection?.Invoke(_cardCollectionType); 
        Destroy(gameObject);
    } 
}
