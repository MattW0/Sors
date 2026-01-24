using UnityEngine;
using Mirror;
using System.Linq;
using System.Collections.Generic;
using System;

public class NetworkObjectSpawner : NetworkBehaviour //, INetworkObjectSpawner
{
    [Header("Spawnable Prefabs")]
    [SerializeField] private GameObject _moneyCard;
    [SerializeField] private GameObject _creatureCard;
    [SerializeField] private GameObject _technologyCard;
    [SerializeField] private GameObject _creatureEntity;
    [SerializeField] private GameObject _technologyEntity;
    
    [Header("Special Cards")]
    [SerializeField] private ScriptableCard _curseCard;

    // Card lookup dictionary (goID -> CardStats)
    private readonly Dictionary<int, CardStats> _cardLookup = new();

    private void Awake() => ServiceLocator.Global.Register(this);

    public GameObject PlayerGainCard(PlayerManager player, CardInfo cardInfo)
    {
        var scriptableCard = ScriptableCardFactory.Load(cardInfo.resourceName, cardInfo.type);
        if (scriptableCard == null) 
        {
            Debug.LogWarning("Trying to spawn card where scriptable is null: " + scriptableCard.name);
            return null;
        }

        return SpawnCard(player, scriptableCard);
    }

    public GameObject SpawnCard(PlayerManager player, ScriptableCard scriptableCard)
    {
        // print($"Spawning card {scriptableCard.title} for {player.PlayerName}");

        var cardObject = CreateCardObject(scriptableCard);
        if (cardObject == null) return null;

        SpawnWithClientAuthority(cardObject, player);
        InitializeCardOnClients(cardObject, scriptableCard);

        return cardObject;
    }

    public BattleZoneEntity SpawnFieldEntity(PlayerManager owner, CardInfo cardInfo)
    {
        print($"Spawning entity {cardInfo.title} for {owner.PlayerName}");

        GameObject entityObject = CreateEntityObject(cardInfo);
        if (entityObject == null) return null;

        var id = entityObject.GetInstanceID();
        entityObject.name = cardInfo.title + "_" + id.ToString();
        
        SpawnWithClientAuthority(entityObject, owner);
        var entity = entityObject.GetComponent<BattleZoneEntity>();
        entity.RpcInitializeEntity(id, owner, cardInfo);
        
        return entity;
    }

    public GameObject PlayerGainCurse(PlayerManager player)
    {
        return SpawnCard(player, _curseCard);
    }

    private GameObject CreateCardObject(ScriptableCard scriptableCard)
    {
        return scriptableCard.type switch
        {
            CardType.Money => Instantiate(_moneyCard),
            CardType.Creature => Instantiate(_creatureCard),
            CardType.Technology => Instantiate(_technologyCard),
            _ => null
        };
    }

    private GameObject CreateEntityObject(CardInfo cardInfo)
    {
        return cardInfo.type switch
        {
            CardType.Creature => Instantiate(_creatureEntity),
            CardType.Technology => Instantiate(_technologyEntity),
            _ => null
        };
    }

    private void SpawnWithClientAuthority(GameObject o, PlayerManager p)
    {
        NetworkServer.Spawn(o, connectionToClient);
        if(p.connectionToClient != null)
        {
            o.GetComponent<NetworkIdentity>().AssignClientAuthority(p.connectionToClient);
        }
    }

    private void InitializeCardOnClients(GameObject cardObject, ScriptableCard scriptableCard)
    {
        var instanceID = cardObject.GetInstanceID();
        cardObject.name = scriptableCard.title + "_" + instanceID.ToString();

        var cardStats = cardObject.GetComponent<CardStats>();
        var cardInfo = new CardInfo(scriptableCard, instanceID);

        cardStats.RpcSetCardStats(cardInfo);

        // Register card
        _cardLookup.SafeAdd(cardInfo.goID, cardStats);
    }

    public CardStats GetCardById(int goID)
    {
        _cardLookup.TryGetValue(goID, out var card);
        if(card == null) throw new Exception("Try to look up inexistant card: " + goID);
        return card;
    }

    public List<CardStats> GetCardListByIds(List<int> ids)
    {
        return ids.Select(c => GetCardById(c)).ToList();
    }
}