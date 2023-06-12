using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class BattleZoneEntity : NetworkBehaviour
{
    public PlayerManager Owner { get; private set; }
    public PlayerManager Opponent { get; private set; }
    public string Title { get; private set; }
    [SerializeField] private CreatureEntity _creatureEntity;
    [SerializeField] private DevelopmentEntity _developmentEntity;
    
    [Header("Stats")]
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

    private BoardManager _boardManager;
    public event Action<PlayerManager, BattleZoneEntity> OnDeath;
    [SerializeField] private EntityUI _entityUI;

    [ClientRpc]
    public void RpcInitializeEntity(PlayerManager owner, PlayerManager opponent, CardInfo cardInfo){        
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
        
        if (isServer) _boardManager = BoardManager.Instance;
    }

    [ClientRpc]
    private void RpcSetHealth(int value)=> _entityUI.SetHealth(value);

    [ClientRpc]
    private void RpcSetAttack(int value)=> _entityUI.SetAttack(value);

    [ClientRpc]
    private void RpcSetPoints(int value)=> _entityUI.SetPoints(value);

    private void Die() => OnDeath?.Invoke(Owner, this);
}
