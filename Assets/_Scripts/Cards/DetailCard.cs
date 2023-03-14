using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DetailCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private CardCollectionView _collectionView;
    [SerializeField] private GameObject _cardHighlight;
    private Canvas _tempCanvas;
    private GraphicRaycaster _tempRaycaster;
    private bool _chosen;
    public bool isChoosable;

    private void Awake()
    {
        _collectionView = CardCollectionView.Instance;
    }

    public void SetCardUI(CardInfo card) => gameObject.GetComponent<CardUI>().SetCardUI(card);

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isChoosable) return;

        if(_chosen) {
            _collectionView.RemoveCardFromChosen(this);
            _chosen = false;
            return;
        }

        _collectionView.AddCardToChosen(this);
        _chosen = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // add and configure necessary components
        _tempCanvas = gameObject.AddComponent<Canvas>();
        _tempCanvas.overrideSorting = true;
        _tempCanvas.sortingOrder = 1;
        _tempRaycaster = gameObject.AddComponent<GraphicRaycaster>();
        
        _cardHighlight.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // remove components that are not needed anymore
        Destroy(_tempRaycaster);
        Destroy(_tempCanvas);
        _cardHighlight.SetActive(false);
    }

}