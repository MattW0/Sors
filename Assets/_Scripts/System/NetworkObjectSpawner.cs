using UnityEngine;
using Mirror;
using System.Linq;
using System.Collections.Generic;
using System;

public class NetworkObjectSpawner : NetworkBehaviour, INetworkObjectSpawner
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

    private void Awake() => ServiceLocator.Global.Register<INetworkObjectSpawner>(this);

    public void PlayerGainCard(PlayerManager player, CardInfo cardInfo, CardLocation destination)
    {
        // Load scriptable
        var pathPrefix = cardInfo.type switch {
            CardType.Money => "Cards/MoneyCards/",
            CardType.Creature => "Cards/CreatureCards/",
            CardType.Technology => "Cards/TechnologyCards/",
            _ => ""
        };

        var scriptableCard = Resources.Load<ScriptableCard>(pathPrefix + cardInfo.resourceName);
        var co = SpawnCard(player, scriptableCard, destination);

        player.Cards.RpcShowSpawnedCard(co, destination);
    }

    public GameObject SpawnCard(PlayerManager player, ScriptableCard scriptableCard, CardLocation destination)
    {
        if (scriptableCard == null) 
        {
            Debug.LogWarning("Trying to spawn card where scriptable is null");
            return null;
        }

        print($"Spawning card {scriptableCard.title} for {player.PlayerName}");

        var cardObject = CreateCardObject(scriptableCard);
        if (cardObject == null) return null;

        SpawnWithClientAuthority(cardObject, player);
        var cardStats = InitializeCardOnClients(cardObject, scriptableCard);
        AddCardToPlayerCollection(player, cardStats, destination);

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

    public void PlayerGainCurse(PlayerManager player)
    {
        SpawnCard(player, _curseCard, CardLocation.Discard);
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

    private CardStats InitializeCardOnClients(GameObject cardObject, ScriptableCard scriptableCard)
    {
        var instanceID = cardObject.GetInstanceID();
        cardObject.name = scriptableCard.title + "_" + instanceID.ToString();

        var cardStats = cardObject.GetComponent<CardStats>();
        var cardInfo = new CardInfo(scriptableCard, instanceID);

        cardStats.RpcSetCardStats(cardInfo);
        RegisterCard(cardInfo.goID, cardStats);

        return cardStats;
    }

    private void AddCardToPlayerCollection(PlayerManager owner, CardStats card, CardLocation destination)
    {
        if (destination == CardLocation.Deck) owner.Cards.deck.Add(card);
        else if(destination == CardLocation.Discard) owner.Cards.discard.Add(card);
        else if(destination == CardLocation.Hand) owner.Cards.hand.Add(card);
        else Debug.LogWarning("Trying to add card to invalid location: " + destination);
    }

    private void RegisterCard(int goID, CardStats card)
    {
        if (!_cardLookup.ContainsKey(goID))
            _cardLookup[goID] = card;
        else
            Debug.LogWarning($"Card with goID {goID} already registered.");
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