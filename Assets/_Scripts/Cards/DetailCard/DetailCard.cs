using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DetailCard : MonoBehaviour, IPointerClickHandler
{
    private CardInfo _cardInfo;
    private CardCollectionPanel _collectionPanel;
    private DetailCardUI _ui;
    private bool _chosen;
    public bool isChoosable;

    private void Awake()
    {
        _collectionPanel = CardCollectionPanel.Instance;
        _ui = gameObject.GetComponent<DetailCardUI>();
    }

    public void SetCardUI(CardInfo card) {
        _cardInfo = card;
        _ui.SetCardUI(card);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isChoosable) return;

        if(_chosen) {
            _collectionPanel.RemoveCardFromChosen(transform, _cardInfo);
            _chosen = false;
            return;
        }

        _collectionPanel.AddCardToChosen(transform, _cardInfo);
        _chosen = true;
    }

}