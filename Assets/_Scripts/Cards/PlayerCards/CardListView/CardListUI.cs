using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class CardListUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _collectionTitle;
    [SerializeField] private Button _closeButton;
    [SerializeField] IDragHandler _draggable;

    private void Start()
    {
        _closeButton.onClick.AddListener(Close);
    }

    public void Open(CardListInfo listInfo)
    {
        gameObject.SetActive(true);

        var text = listInfo.isMine ? "Player " : "Opponent ";
        if (listInfo.location == CardLocation.Deck) text += "Deck";
        else if (listInfo.location == CardLocation.Discard) text += "Discard";
        else if (listInfo.location == CardLocation.Hand) text += "Hand";
        else if (listInfo.location == CardLocation.PlayZone) text += "Play Zone";
        // Nobody owns these collections
        else if (listInfo.location == CardLocation.Trash) {
            text = "Trash";
        }

        _collectionTitle.text = text;
    }

    private void Close()
    {
        Destroy(gameObject);
    }
}
