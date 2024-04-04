using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;

public class CardPileClick : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private bool _isMine;
    private CardLocation _pileType;
    private Graphic _clickOverlayImage;

    public static event Action<CardLocation, bool> OnLookAtCollection;

    void Start()
    {
        _clickOverlayImage = GetComponent<Image>();
        _pileType = gameObject.transform.parent.GetComponent<CardsPileSors>().pileType;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right) return;

        OnLookAtCollection?.Invoke(_pileType, _isMine);
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
