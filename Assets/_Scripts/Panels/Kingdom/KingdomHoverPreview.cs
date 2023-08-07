using System;
using System.Collections.Generic;
using UnityEngine;

public class KingdomHoverPreview : MonoBehaviour
{
    [SerializeField] private RectTransform previewWindow;
    [SerializeField] private DetailCard previewCard;

    // _offset = absolute position of preview window + half of its size
    private Vector3 _offset = new Vector3(960f, 540f, 0f);
    private const float viewHeight = 400f;
    private const float viewWidth = 260f;

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
    }

    //TODO: Make sure the whole window is within screen bounds
    private void ShowPreview(CardInfo cardInfo){
        previewCard.SetCardUI(cardInfo);
        SetViewPosition();
        previewWindow.gameObject.SetActive(true);
    }

    private void HidePreview(){
        previewWindow.gameObject.SetActive(false);
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
