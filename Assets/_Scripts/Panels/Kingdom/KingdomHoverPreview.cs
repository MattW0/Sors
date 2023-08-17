using System;
using System.Collections.Generic;
using UnityEngine;

public class KingdomHoverPreview : MonoBehaviour
{
    [SerializeField] private RectTransform previewWindow;
    [SerializeField] private GameObject _creatureDetailCard;
    [SerializeField] private GameObject _technologyDetailCard;
    [SerializeField] private GameObject _moneyDetailCard;

    // _offset = absolute position of preview window + half of its size
    // TODO: Is not proper yet
    private Vector3 _offset = new Vector3(940f, 520f, 0f);
    private const float viewHeight = 440f;
    private const float viewWidth = 280f;

    public static Action<CardInfo> OnHoverTile;
    public static Action OnHoverExit;


    private void OnEnable(){
        OnHoverTile += ShowPreview;
        OnHoverExit += HidePreview;
    }

    private void OnDisable(){
        OnHoverTile -= ShowPreview;
        OnHoverExit -= HidePreview;
    }

    private void Start(){
        HidePreview();

        _creatureDetailCard.SetActive(false);
        _technologyDetailCard.SetActive(false);
        _moneyDetailCard.SetActive(false);
    }

    //TODO: Make sure the whole window is within screen bounds
    private void ShowPreview(CardInfo cardInfo){

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

    private void HidePreview(){
        _creatureDetailCard.SetActive(false);
        _technologyDetailCard.SetActive(false);
        _moneyDetailCard.SetActive(false);
    }

    private void SetViewPosition(){
        var anchor = Input.mousePosition;

        var endWidth = anchor.x + viewWidth;
        var endHeight = anchor.y + viewHeight;
        if(endWidth > Screen.width) anchor.x -= endWidth - Screen.width;
        if(endHeight > Screen.height) anchor.y -= endHeight - Screen.height;

        previewWindow.localPosition = anchor - _offset;
    }
}
