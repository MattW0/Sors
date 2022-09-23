using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDeploy : MonoBehaviour
{
    private CardStats _cardStats;

    private void Awake()
    {
        _cardStats = gameObject.GetComponent<CardStats>();
    }

    public void OnDeployClick()
    {
        if (!_cardStats.IsDeployable) return;
        
        print("Deploying " + _cardStats.cardInfo.title);
        
        _cardStats.owner.CmdDeploy(_cardStats.cardInfo);
    }
}
