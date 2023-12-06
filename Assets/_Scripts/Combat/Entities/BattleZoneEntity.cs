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
    [SerializeField] private EntityUI _entityUI;

    [Header("Stats")]
    public CardType cardType;
    public CardInfo CardInfo { get; private set; }
    [SerializeField] private int _health;
    public int Health
    {
        get => _health;
        set
        {
            _health = value;
            if(cardType == CardType.Player) return;
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

    private bool _targetable;
    public bool IsTargetable {
        get => _targetable;
        set {
            _targetable = value;

            if(cardType == CardType.Player) Player.EntityTargetHighlight(value);
            else {
                var color = value ? SorsColors.targetColor : SorsColors.creatureHighlight;
                _entityUI.EffectHighlight(true, color);
            }
        }
    }

    public PlayerManager Player { get; private set; }
    public CreatureEntity Creature { get; private set; }
    public TechnologyEntity Technology { get; private set; }
    private TargetArrowHandler _targetArrowHandler;
    private AttackerArrowHandler _attackerArrowHandler;
    private BlockerArrowHandler _blockerArrowHandler;

    private void Awake(){
        CombatManager.OnCombatStateChanged += RpcCombatStateChanged;
        _targetArrowHandler = gameObject.GetComponent<TargetArrowHandler>();
        _attackerArrowHandler = gameObject.GetComponent<AttackerArrowHandler>();

        if(cardType == CardType.Creature) {
            Creature = GetComponent<CreatureEntity>();
            _blockerArrowHandler = GetComponent<BlockerArrowHandler>();
        } else if (cardType == CardType.Technology){
            Technology = GetComponent<TechnologyEntity>();
        } else if (cardType == CardType.Player){
            Player = GetComponent<PlayerManager>();
        }
    }

    [ClientRpc]
    public void RpcInitializeEntity(PlayerManager owner, PlayerManager opponent, CardInfo cardInfo)
    {
        // Will be set active by cardMover, once entity is spawned correctly in UI
        gameObject.SetActive(false);

        CardInfo = cardInfo;
        Owner = owner;
        Opponent = opponent;

        Title = cardInfo.title;
        cardType = cardInfo.type;
        _attack = cardInfo.attack;
        _health = cardInfo.health;
        _points = cardInfo.points;

        if(cardType == CardType.Creature) Creature.SetKeywords(cardInfo.keywordAbilities);

        _entityUI.SetEntityUI(cardInfo);
        
        if (!isServer) return;
        _boardManager = BoardManager.Instance;
    }

    [Server]
    public void TakesDamage(int value, bool deathtouch){
        if (deathtouch){ 
            Health = 0;
            return;
        }
        Health -= value;
    }
    private void Die() => _boardManager.EntityDies(this);

    [ClientRpc]
    private void RpcSetHealth(int value)=> _entityUI.SetHealth(value);
    [ClientRpc]
    private void RpcSetAttack(int value)=> _entityUI.SetAttack(value);
    [ClientRpc]
    private void RpcSetPoints(int value)=> _entityUI.SetPoints(value);
    [ClientRpc]
    public void RpcEffectHighlight(bool value) => _entityUI.EffectHighlight(value, Color.red);

    [ClientRpc]
    public void RpcCombatStateChanged(CombatState newState){
        _targetArrowHandler.CombatStateChanged(newState);
        _attackerArrowHandler.CombatStateChanged(newState);
        if(cardType == CardType.Creature) _blockerArrowHandler.CombatStateChanged(newState);
    }

    private void OnDestroy()
    {
        CombatManager.OnCombatStateChanged -= RpcCombatStateChanged;
    }
    
    #region UI Target Arrows
    [TargetRpc]
    public void TargetDeclaredAttack(NetworkConnection conn, BattleZoneEntity target){
        Creature.CanAct = false;
        _attackerArrowHandler.HandleFoundTarget(target.transform);
    }

    [ClientRpc]
    public void RpcDeclaredAttack(BattleZoneEntity target){
        Creature.CanAct = false;
        Creature.IsAttacking = true;
        _attackerArrowHandler.HandleFoundTarget(target.transform);
        // PlayerInterfaceManager.Log($"  - {Title} attacks {target.Title}", LogType.Standard);
    }

    [TargetRpc]
    public void TargetDeclaredBlock(NetworkConnection conn, BattleZoneEntity target){
        Creature.CanAct = false;
        _blockerArrowHandler.HandleFoundTarget(target.transform);
    }
    [ClientRpc]
    public void RpcDeclaredBlock(BattleZoneEntity target){
        Creature.CanAct = false;
        Creature.IsBlocking = true;
        _blockerArrowHandler.HandleFoundTarget(target.transform);  
    }
    
    [TargetRpc]
    public void TargetSpawnTargetArrow(NetworkConnection target) => _targetArrowHandler.SpawnArrow();
    [ClientRpc]
    public void RpcDeclaredTarget(BattleZoneEntity target) => _targetArrowHandler.HandleFoundTarget(target.transform);
    [ClientRpc]
    public void RpcResetAfterTarget() => _targetArrowHandler.RemoveArrow(false);
    #endregion

}
