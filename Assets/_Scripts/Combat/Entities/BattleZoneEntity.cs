using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class BattleZoneEntity : NetworkBehaviour
{
    private BoardManager _boardManager;

    public PlayerManager Owner { get; private set; }
    public PlayerManager Opponent { get; private set; }
    public string Title { get; private set; }
    [SerializeField] private CreatureEntity _creatureEntity;
    [SerializeField] private DevelopmentEntity _developmentEntity;
    [SerializeField] private TargetArrowHandler _targetArrowHandler;
    [SerializeField] private EntityUI _entityUI;

    private bool _targetable;
    public bool Targetable {
        get => _targetable;
        set {
            _targetable = value;
            _entityUI.EffectHighlight(value, Color.blue);
        }
    }

    [Header("Stats")]
    [SerializeField] private CardInfo _cardInfo;
    public CardType cardType;
    [SerializeField] private int _health;
    public int Health
    {
        get => _health;
        set
        {
            _health = value;
            RpcSetHealth(_health);
            if (_health <= 0) Die();
        }
    }
    
    [SerializeField] private int _attack;
    public int Attack
    {
        get => _attack;
        set
        {
            _attack = value;
            RpcSetAttack(_attack);
        }
    }

    [SerializeField] private int _points;
    private int Points
    {
        get => _points;
        set
        {
            _points = value;
            RpcSetPoints(_points);
        }
    }

    [ClientRpc]
    public void RpcInitializeEntity(PlayerManager owner, PlayerManager opponent, CardInfo cardInfo)
    {
        _cardInfo = cardInfo;
        Owner = owner;
        Opponent = opponent;

        Title = cardInfo.title;
        cardType = cardInfo.type;
        _attack = cardInfo.attack;
        _health = cardInfo.health;
        _points = cardInfo.points;

        if(cardType == CardType.Creature) {
            _creatureEntity.SetKeywords(cardInfo.keywordAbilities);
        }

        _entityUI.SetEntityUI(cardInfo);
        
        if (!isServer) return;
        _boardManager = BoardManager.Instance;
    }

    [ClientRpc]
    private void RpcSetHealth(int value)=> _entityUI.SetHealth(value);

    [ClientRpc]
    private void RpcSetAttack(int value)=> _entityUI.SetAttack(value);

    [ClientRpc]
    private void RpcSetPoints(int value)=> _entityUI.SetPoints(value);

    [ClientRpc]
    public void RpcEffectHighlight(bool value) => _entityUI.EffectHighlight(value, Color.white);

    public void SpawnTargetArrow() => _targetArrowHandler.SpawnArrow();
    [TargetRpc]
    public void TargetSpawnTargetArrow(NetworkConnection target) => _targetArrowHandler.SpawnArrow();

    [ClientRpc]
    public void RpcDeclaredTarget(BattleZoneEntity target) => _targetArrowHandler.HandleFoundTarget(target);

    public void RemoveArrow() => _targetArrowHandler.RemoveArrow(true);

    [ClientRpc]
    public void RpcResetAfterTarget() => _targetArrowHandler.RemoveArrow(false);
    private void Die() => _boardManager.EntityDies(this);
}
