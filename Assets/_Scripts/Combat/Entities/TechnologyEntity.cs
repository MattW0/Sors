using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using Mirror;

public class TechnologyEntity : BattleZoneEntity
{
    private void Start(){
        DropZoneManager.OnDeclareAttackers += DeclareAttackers;
    }

    public void InitializeTechnology()
    {
        throw new System.NotImplementedException();
    }

    private void DeclareAttackers(bool begin) 
    {
        if(isOwned) return;

        IsTargetable = begin;
    }

    private void OnDestroy()
    {
        DropZoneManager.OnDeclareAttackers -= DeclareAttackers;
    }
}
