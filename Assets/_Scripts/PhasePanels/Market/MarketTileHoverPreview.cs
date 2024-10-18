using System;
using System.Collections.Generic;
using UnityEngine;

public class MarketTileHoverPreview : MonoBehaviour
{
    [SerializeField] private RectTransform previewWindow;
    [SerializeField] private GameObject _creatureDetailCard;
    [SerializeField] private GameObject _technologyDetailCard;
    [SerializeField] private GameObject _moneyDetailCard;
    private Vector3 _offset = new Vector3(10f, -10f, 0f);
    private float _viewHeight;
    private float _viewWidth;

    public static Action<CardInfo> OnHoverTile;
    public static Action OnHoverExit;

    private void Awake()
    {
        _viewHeight = previewWindow.rect.height;
        _viewWidth = previewWindow.rect.width;
    }

    private void OnEnable()
    {
        OnHoverTile += ShowPreview;
        OnHoverExit += HidePreview;
    }

    private void OnDisable()
    {
        OnHoverTile -= ShowPreview;
        OnHoverExit -= HidePreview;
    }

    private void Start()
    {
        HidePreview();

        _creatureDetailCard.SetActive(false);
        _technologyDetailCard.SetActive(false);
        _moneyDetailCard.SetActive(false);
    }

    //TODO: Make sure the whole window is within screen bounds
    private void ShowPreview(CardInfo cardInfo)
    {
        var previewCardObject = cardInfo.type switch{
            CardType.Creature => _creatureDetailCard,
            CardType.Technology => _technologyDetailCard,
            CardType.Money => _moneyDetailCard,
            _ => null
        };

        var detailCard = previewCardObject.GetComponent<DetailCard>();
        detailCard.SetCardUI(cardInfo);

        SetViewPosition();
        previewCardObject.SetActive(true);
    }

    private void HidePreview()
    {
        _creatureDetailCard.SetActive(false);
        _technologyDetailCard.SetActive(false);
        _moneyDetailCard.SetActive(false);
    }

    private void SetViewPosition()
    {
        var anchor = Input.mousePosition + _offset;

        var endWidth = anchor.x + _viewWidth;
        var endHeight = anchor.y - _viewHeight;
        if(endWidth > Screen.width) anchor.x -= endWidth - Screen.width;
        if(endHeight < 0f) anchor.y -= endHeight;

        previewWindow.position = anchor;
    }
}
