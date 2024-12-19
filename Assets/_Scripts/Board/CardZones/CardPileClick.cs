using UnityEngine;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;

public class CardPileClick : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private CardListInfo _cardListInfo;
    [SerializeField] private bool _isMine;
    private Graphic _clickOverlayImage;

    public static event Action<CardListInfo> OnLookAtCardList;

    void Start()
    {
        _clickOverlayImage = GetComponent<Image>();
        _clickOverlayImage.CrossFadeAlpha(0f, 0.5f, false);

        _cardListInfo = new CardListInfo(_isMine, GetComponentInParent<CardsPileSors>().pileType);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right) return;

        OnLookAtCardList?.Invoke(_cardListInfo);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _clickOverlayImage.CrossFadeAlpha(0.5f, 0.5f, false);
    }
  
    public void OnPointerExit(PointerEventData eventData)
    {
        _clickOverlayImage.CrossFadeAlpha(0f, 0.5f, false);
    }
}