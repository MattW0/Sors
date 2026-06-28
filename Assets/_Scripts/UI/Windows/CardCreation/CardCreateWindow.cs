using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.Shift;
using UnityEngine.UI;
using System;
using Cysharp.Threading.Tasks;
using System.IO;

[RequireComponent(typeof(CardCreator))]
public class CardCreateWindow : ModalWindow
{
    [SerializeField] private Button _createCard;
    [SerializeField] private Button _cancel;
    private CardCreator _creator;
    
    void Start()
    {
        _creator = GetComponent<CardCreator>();
        _createCard.onClick.AddListener(CreateCard);
        _cancel.onClick.AddListener(WindowOut);
    }

    private void CreateCard()
    {
        var cardInfo = _creator.card;
        string json = JsonUtility.ToJson(cardInfo, true);

        var path = Path.Combine(Application.persistentDataPath, $"{cardInfo.title}.json");
        File.WriteAllText(path, json);

        print("wrote file to: " + path);
    }

    private async UniTask DisableWindow()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
        gameObject.SetActive(false);
    }
}
