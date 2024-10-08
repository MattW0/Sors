using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;


[RequireComponent(typeof(AttackerArrowHandler), typeof(TargetArrowHandler))]
public class BattleZoneEntity : NetworkBehaviour
{
    private BoardManager _boardManager;
    public PlayerManager Owner { get; private set; }
    public string Title { get; private set; }
    [SerializeField] private BattleZoneEntityUI _entityUI;
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
            // TODO: How to combine player health with entity health ???
            // BUG: ability causes no player damage 
            if(cardType == CardType.Player) return;
            else {
                RpcSetHealth(_health);
                if (_health <= 0) Die();
            }
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

    [HideInInspector] public TargetArrowHandler targetArrowHandler;
    [HideInInspector] public AttackerArrowHandler attackerArrowHandler;
    [HideInInspector] public BlockerArrowHandler blockerArrowHandler;


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
    public void RpcInitializeEntity(PlayerManager owner, CardInfo cardInfo)
    {
        // Will be set active by cardMover, once entity is spawned correctly in UI
        gameObject.SetActive(false);

        CardInfo = cardInfo;
        Owner = owner;

        Title = cardInfo.title;
        cardType = cardInfo.type;
        _health = cardInfo.health;

        if(cardType == CardType.Creature) gameObject.GetComponent<CreatureEntity>().InitializeCreature(cardInfo.attack, cardInfo.traits);
        else if (cardType == CardType.Technology) gameObject.GetComponent<TechnologyEntity>().InitializeTechnology(cardInfo.points);

        _entityUI.SetEntityUI(cardInfo);
        
        if (!isServer) return;
        _boardManager = BoardManager.Instance;
    }

    private void CheckTargetable(EffectTarget target)
    {
        // TODO: check if this is targetable for target types
        // Any, AnyPlayer, Creature, Technology, Card
        
        if(target == EffectTarget.None) IsTargetable = false;
        else if(target == EffectTarget.Entity) IsTargetable = true;
        else if (target == EffectTarget.AnyPlayer) {
            if(gameObject.GetComponent<PlayerEntity>() != null) IsTargetable = true;
        } else if (target == EffectTarget.Creature){
            if(gameObject.GetComponent<CreatureEntity>() != null) IsTargetable = true;
        } else if (target == EffectTarget.Technology){
            if(gameObject.GetComponent<TechnologyEntity>() != null) IsTargetable = true;
        } else if (target == EffectTarget.Any){
            // TODO: Remove this ?
        }
    }

    [Server]
    public void EntityTakesDamage(int value, bool deathtouch){

        if (cardType == CardType.Player){
            Owner.Health -= value;
            return;
        }

        if (deathtouch){ 
            Health = 0;
            return;
        }
        Health = Mathf.Max(0, Health - value);
    }

    private void Die()
    {
        _boardManager.EntityDies(this);
    }

    [ClientRpc]
    private void RpcSetHealth(int value) => _entityUI.SetHealth(value);
    [ClientRpc] // Public because from creatureEntity
    public void RpcSetAttack(int value) => _entityUI.SetAttack(value);
    [ClientRpc] // Public because from technologyEntity§
    public void RpcSetPoints(int value) => _entityUI.SetPoints(value);
    [ClientRpc]
    public void RpcEffectHighlight(bool value) {
        // print("Showing effect target highlight");
        _entityUI.Highlight(value, SorsColors.effectTriggerHighlight);   
    }

    [ClientRpc]
    public virtual void RpcCombatStateChanged(CombatState newState){
        targetArrowHandler.CombatStateChanged(newState);
        attackerArrowHandler.CombatStateChanged(newState);
    }
    
    #region UI Target Arrows
    private void ResetEntityUI()
    {
        // print($"{Title} : Resetting entity UI");
        if(cardType != CardType.Player) _entityUI.DisableHighlight();
    }
    [ClientRpc]
    public void RpcSetCombatHighlight()
    {
        if (cardType != CardType.Player) _entityUI.Highlight(true, SorsColors.combatClash);
        else _playerUI.Highlight(true, SorsColors.combatClash);
    } 
    [TargetRpc]
    public void TargetSpawnTargetArrow(NetworkConnection target) => targetArrowHandler.SpawnArrow();
    [ClientRpc]
    public void RpcDeclaredTarget(BattleZoneEntity target) => targetArrowHandler.HandleFoundTarget(target.transform);
    [ClientRpc]
    public void RpcResetAfterTarget() => targetArrowHandler.RemoveArrow(false);
    #endregion

    // Need this for player UI highlights : attackable, targetable, ...
    public void SetPlayer(string title, PlayerUI playerUI)
    {
        Title = title;
        _playerUI = playerUI;
        Owner = GetComponent<PlayerManager>();
    } 

    // Somehow NetworkServer.Destroy(this) destroys the GO but does not call OnDestroy(),
    // Thus, do it here manually to prevent null references when events are triggered
    [ClientRpc] public void RpcUnsubscribeEvents() => UnsubscribeEvents();
    public void UnsubscribeEvents()
    {
        // print($"Destroying {Title}");
        DropZoneManager.OnTargetEntities -= CheckTargetable;
        if (cardType == CardType.Player) return;

        CombatManager.OnCombatStateChanged -= RpcCombatStateChanged;
        DropZoneManager.OnResetEntityUI -= ResetEntityUI;
    }
    private void OnDestroy() => UnsubscribeEvents();

    public bool Equals(BattleZoneEntity other)
    {
        if (other == null) return false;

        // Optimization for a common success case.
        if (GameObject.ReferenceEquals(this, other)) return true;

        // Return true if the fields match.
        return (gameObject.GetInstanceID() == other.gameObject.GetInstanceID());
    }
}
