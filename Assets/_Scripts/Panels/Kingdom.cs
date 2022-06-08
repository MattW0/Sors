using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;
using UnityEngine.UI;

public class Kingdom : NetworkBehaviour
{
    public static Kingdom Instance { get; private set; }

    [SerializeField] private KingdomCard[] _kingdomCards;
    [SerializeField] private GameObject _cardGrid;
    
    // UI
    [SerializeField] private Button _maximize, _minimize;
    [SerializeField] private GameObject _smallView, _kingdom;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        gameObject.transform.SetParent(GameObject.Find("UI").transform, false);
    }

    public void SetKingdomCards(CardInfo[] _kingdomCardsInfo)
    {   
        _kingdomCards = new KingdomCard[_kingdomCardsInfo.Length];
        _kingdomCards = _cardGrid.GetComponentsInChildren<KingdomCard>();

        for (int i = 0; i < _kingdomCardsInfo.Length; i++)
        {
            _kingdomCards[i].SetCard(_kingdomCardsInfo[i]);
        }
    }

    public void MinButton()
    {
        _smallView.SetActive(true);
        _kingdom.SetActive(false);
    }

    public void MaxButton()
    {
        _smallView.SetActive(false);
        _kingdom.SetActive(true);
    }
}
