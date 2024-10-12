using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.Shift;
using UnityEngine.UI;
using System;
using Cysharp.Threading.Tasks;

public class CardCreateWindow : ModalWindow
{
    [SerializeField] private Button _createCard;
    [SerializeField] private Button _cancel;
    
    void Start()
    {
        _createCard.onClick.AddListener(CreateCard);
        _cancel.onClick.AddListener(ModalWindowOut);
    }

    private void CreateCard()
    {
        throw new NotImplementedException();
    }

    private async UniTask DisableWindow()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
        gameObject.SetActive(false);
    }
}
