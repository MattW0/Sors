using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDeploy : MonoBehaviour
{
    private PlayerManager _owner;
    private CardStats _cardStats;
    
    private bool _isDeployable;

    private void Awake()
    {
        _cardStats = gameObject.GetComponent<CardStats>();
        _owner = _cardStats.owner;
    }

    public void OnDeployClick()
    {
        if (!_cardStats.IsDeployable) return;
        
        print("Deploying " + _cardStats.cardInfo.title);
        
        _cardStats.owner.CmdDeploy(_cardStats.cardInfo);
    }
}
