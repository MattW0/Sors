using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class CardListUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _collectionTitle;
    [SerializeField] private Button _closeButton;
    private CardListInfo _listInfo;
    public static event Action<CardListInfo> OnCloseCardCollection;

    private void Start()
    {
        _closeButton.onClick.AddListener(Close);
    }

    public void Open(CardListInfo listInfo)
    {
        gameObject.SetActive(true);
        _listInfo = listInfo;

        var text = listInfo.isMine ? "" : "Opponent ";
        if (listInfo.location == CardLocation.Deck) text += "Deck";
        else if (listInfo.location == CardLocation.Discard) text += "Discard";
        else if (listInfo.location == CardLocation.Hand) text += "Hand";
        else if (listInfo.location == CardLocation.MoneyZone) text += "Money Zone";
        else if (listInfo.location == CardLocation.PlayZone) text += "Play Zone";
        // Nobody owns these collections
        else if (listInfo.location == CardLocation.Trash) {
            text = "Trash";
        }

        _collectionTitle.text = text;
    }

    private void Close()
    {
        OnCloseCardCollection?.Invoke(_listInfo); 
        Destroy(gameObject);
    }
}
