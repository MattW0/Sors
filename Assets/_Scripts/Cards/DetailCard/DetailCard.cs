using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DetailCard : MonoBehaviour, IPointerClickHandler
{
    private CardInfo _cardInfo;
    private CardCollectionPanel _collectionPanel;
    private DetailCardUI _ui;
    public bool isChoosable;
    private bool _chosen;
    private bool IsChosen{
        get => _chosen;
        set{
            _chosen = value;
            if (_chosen) _collectionPanel.AddCardToChosen(transform, _cardInfo);
            else _collectionPanel.RemoveCardFromChosen(transform, _cardInfo);
        }
    }

    private void Awake()
    {
        _collectionPanel = CardCollectionPanel.Instance;
        _ui = gameObject.GetComponent<DetailCardUI>();
    }

    public void CheckDeployability(int cash){
        isChoosable = cash >= _cardInfo.cost;
        if(isChoosable) _ui.EnableHighlight();
        else _ui.DisableHighlight();
    }

    public void SetCardUI(CardInfo card) {
        _cardInfo = card;
        _ui.SetCardUI(card);
    }

    public void SetCardState(TurnState state){
        _ui.SetCardState(state);

        if(state == TurnState.Discard) isChoosable = true;
    } 

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isChoosable) return;
        IsChosen = !_chosen;
    }

}