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
    [SerializeField] public EntityUI _entityUI;

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

            if(cardType == CardType.Player) Player.EntityTargetHighlight(value);
            else {
                if(value) _entityUI.Highlight(true, SorsColors.targetColor);
                else _entityUI.DisableHighlight();
            }
        }
    }

    public PlayerManager Player { get; private set; }
    public TargetArrowHandler targetArrowHandler;
    public AttackerArrowHandler attackerArrowHandler;
    public BlockerArrowHandler blockerArrowHandler;


    private void Awake(){
        targetArrowHandler = GetComponent<TargetArrowHandler>();
        attackerArrowHandler = GetComponent<AttackerArrowHandler>();

        if (cardType == CardType.Player){
            Player = GetComponent<PlayerManager>();
        } else {
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
    private void RpcSetPoints(int value)=> _entityUI.SetPoints(value);
    [ClientRpc]
    public void RpcEffectHighlight(bool value) => _entityUI.Highlight(value, SorsColors.effectTriggerHighlight);

    [ClientRpc]
    public virtual void RpcCombatStateChanged(CombatState newState){
        targetArrowHandler.CombatStateChanged(newState);
        attackerArrowHandler.CombatStateChanged(newState);
    }
    
    #region UI Target Arrows
    private void ResetEntityUI()
    {
        if(!(cardType == CardType.Player)) _entityUI.DisableHighlight();
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

    private void OnDestroy()
    {
        CombatManager.OnCombatStateChanged -= RpcCombatStateChanged;
        DropZoneManager.OnResetEntityUI -= ResetEntityUI;
    }
}
