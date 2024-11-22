using UnityEngine;

public class TechnologyEntity : BattleZoneEntity
{

    [SerializeField] private int _points;
    public int Points
    {
        get => _points;
        private set
        {
            _points = value;
            RpcSetPoints(_points);
        }
    }

    private void Start(){
        DropZoneManager.OnDeclareAttackers += DeclareAttackers;
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

    internal void InitializeTechnology(int points)
    {
        _points = points;
    }
}
