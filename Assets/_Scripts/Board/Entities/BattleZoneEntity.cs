using System;
using System.Collections.Generic;
using Mirror;
using Unity.VisualScripting;
using UnityEngine;


[Serializable, RequireComponent(typeof(EntityClickHandler))]
public class BattleZoneEntity : NetworkBehaviour
{
    public PlayerManager Owner { get; private set; }
    public string Title { get; private set; }

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
            
            RpcSetHealth(_health);
            if (_health <= 0) OnEntityDies?.Invoke(this);
        }
    }
    private bool _targetable;
    public bool IsTargetable {
        get => _targetable;
        set {
            _targetable = value;
            if(cardType == CardType.Player) _playerUI.TargetHighlight(value, isOwned);
            else _entityUI.Highlight(HighlightType.Target);
        }
    }

    public PlayZoneCardHolder EntityHolder { get; internal set; }

    [ClientRpc]
    public void RpcMoveToHolder() => EntityHolder.EntityEnters(transform);

    [SerializeField] private PlayerUI _playerUI;
    private EntityUI _entityUI;

    public static event Action<Transform, int> OnTargetStart;
    public static event Action<bool, Transform, Transform> OnTargetFinish;
    public static event Action<BattleZoneEntity> OnEntityDies;

    private void Awake()
    {
        DropZoneManager.OnTargetEntities += CheckTargetable;
        if (cardType == CardType.Player) return;
        
        _entityUI = GetComponent<EntityUI>();
        DropZoneManager.OnResetEntityUI += ResetEntityUI;
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

        _entityUI.SetCardUI(cardInfo);
    }

    [Server]
    public void EntityTakesDamage(int value, bool deathtouch)
    {
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

    [ClientRpc]
    private void RpcSetHealth(int value) => _entityUI.SetHealth(value);
    [ClientRpc] // Public because from creatureEntity
    public void RpcSetAttack(int value) => _entityUI.SetAttack(value);
    [ClientRpc] // Public because from technologyEntity
    public void RpcSetPoints(int value) => _entityUI.SetPoints(value);
    [ClientRpc]
    public void RpcSetHighlight(HighlightType type) => _entityUI.Highlight(type);
    
    #region Targeting
    public void CheckTargetable(Target target)
    {
        // TODO: check if this is targetable for target types
        // Any, AnyPlayer, Creature, Technology, Card
        
        if(target == Target.None) 
            IsTargetable = false;
        else if(target == Target.Entity) 
            IsTargetable = true;
        else if (target == Target.AnyPlayer)
           IsTargetable = gameObject.GetComponent<PlayerEntity>() != null;
        else if (target == Target.Creature)
            IsTargetable = gameObject.GetComponent<CreatureEntity>() != null;
        else if (target == Target.Technology)
            IsTargetable = gameObject.GetComponent<TechnologyEntity>() != null;
    }

    [TargetRpc]
    public void TargetSpawnTargetArrow(NetworkConnection conn) => OnTargetStart?.Invoke(transform, GetInstanceID());
    [ClientRpc]
    public void RpcDeclaredTarget(BattleZoneEntity target) => OnTargetFinish?.Invoke(isOwned, transform, target.transform);

    #endregion

    // Need this for player UI highlights : attackable, targetable, ...
    public void SetPlayer(string title, PlayerUI playerUI)
    {
        Title = title;
        _playerUI = playerUI;
        Owner = GetComponent<PlayerManager>();
    }

    private void ResetEntityUI() => _entityUI.DisableHighlight();

    // Somehow NetworkServer.Destroy(this) destroys the GO but does not call OnDestroy(),
    // Thus, do it here manually to prevent null references when events are triggered
    [ClientRpc] public void RpcUnsubscribeEvents() => UnsubscribeEvents();
    public void UnsubscribeEvents()
    {
        // print($"Destroying {Title}");
        DropZoneManager.OnTargetEntities -= CheckTargetable;
        if (cardType == CardType.Player) return;

        DropZoneManager.OnResetEntityUI -= ResetEntityUI;
    }
    private void OnDestroy() => UnsubscribeEvents();

    public bool Equals(BattleZoneEntity other)
    {
        if (other == null) return false;

        // Optimization for a common success case.
        if (ReferenceEquals(this, other)) return true;

        // Return true if the fields match.
        return gameObject.GetInstanceID() == other.gameObject.GetInstanceID();
    }
}
