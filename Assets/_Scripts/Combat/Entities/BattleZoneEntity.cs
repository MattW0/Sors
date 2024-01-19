using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;


[RequireComponent(typeof(AttackerArrowHandler), typeof(TargetArrowHandler))]
public class BattleZoneEntity : NetworkBehaviour
{
    private BoardManager _boardManager;
    public PlayerManager Owner { get; private set; }
    public PlayerManager Opponent { get; private set; }
    public string Title { get; private set; }
    [SerializeField] private EntityUI _entityUI;
    [SerializeField] private PlayerUI _playerUI;

    [field: Header("Stats")]
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
            if(cardType == CardType.Player) _playerUI.TargetHighlight(value, isOwned);
            else _entityUI.Highlight(value, SorsColors.targetColor);
        }
    }

    public TargetArrowHandler targetArrowHandler;
    public AttackerArrowHandler attackerArrowHandler;
    public BlockerArrowHandler blockerArrowHandler;


    private void Awake(){
        targetArrowHandler = GetComponent<TargetArrowHandler>();
        attackerArrowHandler = GetComponent<AttackerArrowHandler>();

        DropZoneManager.OnTargetEntities += CheckTargetable;
        if (cardType != CardType.Player){
            CombatManager.OnCombatStateChanged += RpcCombatStateChanged;
            DropZoneManager.OnResetEntityUI += ResetEntityUI;
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
        _health = cardInfo.health;
        _points = cardInfo.points;

        if(cardType == CardType.Creature) gameObject.GetComponent<CreatureEntity>().InitializeCreature(cardInfo.attack, cardInfo.keywordAbilities);

        _entityUI.SetEntityUI(cardInfo);
        
        if (!isServer) return;
        _boardManager = BoardManager.Instance;
    }

    private void CheckTargetable(EffectTarget target)
    {
        // TODO: check if this is targetable for target types
        // Any, AnyPlayer, Creature, Technology, Card
        if(target == EffectTarget.Any){
            IsTargetable = true;
        } else if (target == EffectTarget.AnyPlayer) {
            if(gameObject.GetComponent<PlayerEntity>() != null) IsTargetable = true;
        } else if (target == EffectTarget.Creature){
            if(gameObject.GetComponent<CreatureEntity>() != null) IsTargetable = true;
        } else if (target == EffectTarget.Technology){
            if(gameObject.GetComponent<TechnologyEntity>() != null) IsTargetable = true;
        }
    }

    [Server]
    public void TakesDamage(int value, bool deathtouch){
        if (deathtouch){ 
            Health = 0;
            return;
        }
        Health -= value;
    }
    private void Die(){
        // Somehow NetworkServer.Destroy(this) destroys the GO but does not call OnDestroy(),
        // Thus, do it here manually to prevent null references when events are triggered
        UnsubscribeEvents();
        _boardManager.EntityDies(this);
    } 

    [ClientRpc]
    private void RpcSetHealth(int value)=> _entityUI.SetHealth(value);
    
    // Public because from creatureEntity
    [ClientRpc]
    public void RpcSetAttack(int value)=> _entityUI.SetAttack(value);
    
    [ClientRpc]
    private void RpcSetPoints(int value)=> _entityUI.SetPoints(value);
    [ClientRpc]
    public void RpcEffectHighlight(bool value) => _entityUI.Highlight(value, SorsColors.effectTriggerHighlight);

    [ClientRpc]
    public virtual void RpcCombatStateChanged(CombatState newState){
        targetArrowHandler.CombatStateChanged(newState);
        attackerArrowHandler.CombatStateChanged(newState);

        // if(newState == CombatState.Attackers && cardType == CardType.Technology || cardType == CardType.Player){
        //     attackerArrowHandler.SpawnArrow();
        // }
    }
    
    #region UI Target Arrows
    private void ResetEntityUI()
    {
        // print($"{Title} : Resetting entity UI");
        if(cardType != CardType.Player) _entityUI.DisableHighlight();
    }
    [ClientRpc]
    public void RpcSetCombatHighlight() => _entityUI.Highlight(true, SorsColors.creatureClashing);
    [TargetRpc]
    public void TargetSpawnTargetArrow(NetworkConnection target) => targetArrowHandler.SpawnArrow();
    [ClientRpc]
    public void RpcDeclaredTarget(BattleZoneEntity target) => targetArrowHandler.HandleFoundTarget(target.transform);
    [ClientRpc]
    public void RpcResetAfterTarget() => targetArrowHandler.RemoveArrow(false);
    #endregion

    public void SetPlayerUI(PlayerUI playerUI) => _playerUI = playerUI;

    private void UnsubscribeEvents() => OnDestroy();

    private void OnDestroy()
    {
        print($"Destroying {Title}");
        DropZoneManager.OnTargetEntities -= CheckTargetable;
        if (cardType != CardType.Player){
            print("Destroying entity UI");
            CombatManager.OnCombatStateChanged -= RpcCombatStateChanged;
            DropZoneManager.OnResetEntityUI -= ResetEntityUI;
        }
    }
}
