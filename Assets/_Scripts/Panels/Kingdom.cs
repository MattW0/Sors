using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;
using UnityEngine.UI;

public class Kingdom : NetworkBehaviour
{
    public static Kingdom Instance { get; private set; }

    private KingdomCard[] _kingdomCards;
    [SerializeField] private GameObject _parent;
    
    // UI
    [SerializeField] private Button _maximize, _minimize;
    [SerializeField] private GameObject _smallView, _kingdom;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void SetKingdomCards(CardInfo[] _kingdomCardsInfo)
    {   
        print(_kingdomCardsInfo.Length);
        this._kingdomCards = new KingdomCard[_kingdomCardsInfo.Length];

        print("SetKingdomCards");
        for (int i = 0; i < _kingdomCardsInfo.Length; i++)
        {
            this._kingdomCards[i].SetCard(_kingdomCardsInfo[i]);
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
