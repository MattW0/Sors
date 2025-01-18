using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SorsGameState;
using Cysharp.Threading.Tasks;

public class GameStateLoader : MonoBehaviour
{
    private GameManager _gameManager;
    private Dictionary<PlayerManager, Cards> _playerCards = new();
    private Dictionary<PlayerManager, Entities> _playerEntities = new();
    private List<GameObject> _cardList = new();
    private Dictionary<GameObject, BattleZoneEntity> _entitiesDict = new();

    public void LoadGameState(string fileName)
    {
        _gameManager = GameManager.Instance;

        var gameState = new GameState(_gameManager.players.Count, fileName).LoadState() 
            ?? throw new System.Exception("Trying to load invalid GameState constructed from file name " + fileName);

        PlayerSetupFromFile(gameState.players);
        LoadMarketFromFile(gameState.market);
    }

    private void PlayerSetupFromFile(Player[] playerData)
    {
        Player host = new();
        Player client = new();

        foreach (var p in playerData){
            if (p.isHost) host = p;
            else client = p;
        }

        foreach (var player in _gameManager.players.Values)
        {
            // p has player info and game state 
            Player p;
            if(player.isLocalPlayer) p = host;
            else p = client;

            player.Health = p.health;

            _playerCards.Add(player, p.cards);
            _playerEntities.Add(player, p.entities);
        }

        SpawningFromFile().Forget();
    }

    private async UniTaskVoid SpawningFromFile()
    {
        foreach(var (player, cards) in _playerCards){
            await SpawnCardsFromFile(player, cards);
        }

        foreach(var (player, entities) in _playerEntities){
            await SpawnEntitiesFromFile(player, entities);
        }

        _gameManager.StartGame();
    }

    private async UniTask SpawnCardsFromFile(PlayerManager p, Cards cards)
    {
        SpawnCardCollection(p, cards, CardLocation.Hand);
        await UniTask.Delay(SorsTimings.waitForSpawnFromFile);

        SpawnCardCollection(p, cards, CardLocation.Deck);
        await UniTask.Delay(SorsTimings.waitForSpawnFromFile);

        SpawnCardCollection(p, cards, CardLocation.Discard);
        await UniTask.Delay(SorsTimings.waitForSpawnFromFile);
    }

    private void SpawnCardCollection(PlayerManager p, Cards cards, CardLocation location)
    {
        List<string> collection = location switch
        {
            CardLocation.Deck => cards.deckCards,
            CardLocation.Discard => cards.discardCards,
            CardLocation.Hand => cards.handCards,
            _ => throw new System.Exception("Invalid card location to spawn cards from file: " + location) 
        };

        foreach (var c in collection)
        {
            var scriptableCard = Resources.Load<ScriptableCard>(c);
            _cardList.Add(_gameManager.SpawnCard(p, scriptableCard, location));
        }
        p.Cards.RpcShowSpawnedCards(_cardList, CardLocation.Hand, true);
        _cardList.Clear();
    }

    private async UniTask SpawnEntitiesFromFile(PlayerManager p, Entities entities)
    {
        print("Spawning entities for " + p.PlayerName);
        foreach(var e in entities.creatures) await SpawnEntity(p, e, true);
        // await UniTask.Delay(SorsTimings.wait);

        print("Spawning technologies ");
        foreach (var e in entities.technologies) await SpawnEntity(p, e, false);
        // await UniTask.Delay(SorsTimings.wait);

        print("Showing spawned entities");
        p.Cards.RpcShowSpawnedCards(_entitiesDict.Keys.ToList(), CardLocation.PlayZone, true);
        await BoardManager.Instance.PlayEntities(_entitiesDict);

        _entitiesDict.Clear();
    }

    private async UniTask SpawnEntity(PlayerManager p, Entity e, bool isCreature)
    {
        var scriptableCard = Resources.Load<ScriptableCard>(e.scriptableCard);

        // Wait for card initialization
        var cardObject = _gameManager.SpawnCard(p, scriptableCard, CardLocation.PlayZone);
        await UniTask.Delay(10);

        // Wait for entity initialization
        var entity = _gameManager.SpawnFieldEntity(p, cardObject.GetComponent<CardStats>().cardInfo);
        await UniTask.Delay(10);

        entity.Health = e.health;
        if (isCreature) entity.GetComponent<CreatureEntity>().Attack = e.attack;

        _entitiesDict.Add(cardObject, entity);
    }

    private void LoadMarketFromFile(SorsGameState.Market market)
    {
        // Get reference to the market object in the game
        var _market = Market.Instance;

        if(market.money.Count == 0 || market.technologies.Count == 0 || market.creatures.Count == 0)
        {
            print("Incomplete market data, loading randomized default settings");
            _market.RpcInitializeMarket();
            return;
        }

        // Money
        var moneyCards = new CardInfo[market.money.Count];
        for (var i = 0; i < market.money.Count; i++) 
            moneyCards[i] = new CardInfo(Resources.Load<ScriptableCard>(market.money[i]));
        _market.RpcSetMoneyTiles(moneyCards);

        // Technologies
        var technologies = new CardInfo[market.technologies.Count];
        for (var i = 0; i < market.technologies.Count; i++)
            technologies[i] = new CardInfo(Resources.Load<ScriptableCard>(market.technologies[i]));
        _market.RpcSetTechnologyTiles(technologies);

        // Creatures
        var creatures = new CardInfo[market.creatures.Count];
        for (var i = 0; i < market.creatures.Count; i++)
            creatures[i] = new CardInfo(Resources.Load<ScriptableCard>(market.creatures[i]));
        _market.RpcSetCreatureTiles(creatures);
    }
}
