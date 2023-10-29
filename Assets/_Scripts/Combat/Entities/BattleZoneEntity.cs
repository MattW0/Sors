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

    private bool _targetable;
    public bool IsTargetable {
        get => _targetable;
        set {
            _targetable = value;
            _entityUI.EffectHighlight(value, Color.blue);
        }
    }

    public CreatureEntity Creature {get; private set;}
    private TechnologyEntity _technology;
    private TargetArrowHandler _targetArrowHandler;
    private AttackerArrowHandler _attackerArrowHandler;
    private BlockerArrowHandler _blockerArrowHandler;

    private void Awake(){
        CombatManager.OnCombatStateChanged += RpcCombatStateChanged;
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
            Creature = gameObject.GetComponent<CreatureEntity>();
            Creature.SetKeywords(cardInfo.keywordAbilities);
        } else if (cardType == CardType.Technology){
            _technology = gameObject.GetComponent<TechnologyEntity>();
        }

        _targetArrowHandler = gameObject.GetComponent<TargetArrowHandler>();
        _attackerArrowHandler = gameObject.GetComponent<AttackerArrowHandler>();
        _blockerArrowHandler = gameObject.GetComponent<BlockerArrowHandler>();
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
    [ClientRpc]
    public void RpcResetAfterTarget() => _targetArrowHandler.RemoveArrow(false);
        
    private void Die() => _boardManager.EntityDies(this);

    [ClientRpc]
    public void RpcCombatStateChanged(CombatState newState){
        _targetArrowHandler.CombatStateChanged(newState);
        _attackerArrowHandler.CombatStateChanged(newState);
        _blockerArrowHandler.CombatStateChanged(newState);
    }

    [ClientRpc]
    public void RpcAttackTargetDeclared(BattleZoneEntity target){
        if (!isOwned) return;
        _attackerArrowHandler.FoundTarget(target.transform.position);
    }

    [ClientRpc]
    public void RpcShowOpponentAttackers(List<CreatureEntity> attackers){
        if (!isOwned) return;

        print($"Showing opponent attackers on entity {Title}");
        foreach (var attacker in attackers){
            print($"Getting attacked by {attacker.Title}");
            _attackerArrowHandler.ShowOpponentAttacker(attacker);
        }
    }

    [ClientRpc]
    public void RpcBlockerDeclared(CreatureEntity attacker){
        if (!isOwned) return;
        _blockerArrowHandler.FoundTarget(attacker.transform.position);
    }
    
    [ClientRpc]
    public void RpcShowOpponentBlockers(List<CreatureEntity> blockers){
        if (!isOwned) return;
        foreach (var blocker in blockers){
            _blockerArrowHandler.ShowOpponentBlocker(blocker);
        }
    }

    private void OnDestroy()
    {
        CombatManager.OnCombatStateChanged -= RpcCombatStateChanged;
    }
}
