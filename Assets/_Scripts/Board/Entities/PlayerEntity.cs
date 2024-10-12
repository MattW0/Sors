using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEntity : BattleZoneEntity
{
    // Start is called before the first frame update
    void Start()
    {
        DropZoneManager.OnDeclareAttackers += DeclareAttackers;
    }

    private void DeclareAttackers(bool begin)
    {
        if (isOwned) return;

        IsTargetable = begin;
    }

    private void OnDestroy()
    {
        DropZoneManager.OnDeclareAttackers -= DeclareAttackers;
    }
}
