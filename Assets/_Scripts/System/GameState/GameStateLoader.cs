using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SorsGameState;
using Cysharp.Threading.Tasks;

public class GameStateLoader : MonoBehaviour
{
    private GameManager _gameManager;

    public void LoadGameState(string fileName)
    {
        _gameManager = GameManager.Instance;

        var gameState = new GameState(_gameManager.players.Count, fileName).LoadState();
        if(gameState == null)
        { 
            print("Error loading game state");
            return;
        }
        PlayerSetupFromFile(gameState.players);
        LoadMarketFromFile(gameState.market);
    }

    private void PlayerSetupFromFile(Player[] playerData)
    {
        Player host = new Player();
        Player client = new Player();

        foreach (var p in playerData){
            if (p.isHost) host = p;
            else client = p;
        }

        Dictionary<PlayerManager, Cards> playerCards = new();
        Dictionary<PlayerManager, Entities> playerEntities = new();
        foreach (var player in _gameManager.players.Values)
        {
            // p has player info and game state 
            Player p = new Player();
            if(player.isLocalPlayer) p = host;
            else p = client;

            player.Health = p.health;

            playerCards.Add(player, p.cards);
            playerEntities.Add(player, p.entities);
        }

        SpawningFromFile(playerCards, playerEntities).Forget();
    }

    private async UniTaskVoid SpawningFromFile(Dictionary<PlayerManager, Cards> playerCards, 
                                               Dictionary<PlayerManager, Entities> playerEntities)
    {
        foreach(var (player, cards) in playerCards){
            await SpawnCardsFromFile(player, cards);
        }

        foreach(var (player, entities) in playerEntities){
            await SpawnEntitiesFromFile(player, entities);
        }

        await UniTask.Delay(SorsTimings.waitForSpawnFromFile);

        _gameManager.StartGame();
    }

    private async UniTask SpawnCardsFromFile(PlayerManager p, Cards cards)
    {
        List<GameObject> cardList = new();
        foreach(var c in cards.handCards){
            var scriptableCard = Resources.Load<ScriptableCard>(c);
            cardList.Add(_gameManager.SpawnCardAndAddToCollection(p, scriptableCard, CardLocation.Hand));
        }
        p.RpcShowSpawnedCards(cardList, CardLocation.Hand, true);
        cardList.Clear();

        await UniTask.Delay(SorsTimings.waitForSpawnFromFile);

        foreach(var c in cards.deckCards){
            var scriptableCard = Resources.Load<ScriptableCard>(c);
            cardList.Add(_gameManager.SpawnCardAndAddToCollection(p, scriptableCard, CardLocation.Deck));
        }
        p.RpcShowSpawnedCards(cardList, CardLocation.Deck, true);
        cardList.Clear();

        await UniTask.Delay(SorsTimings.waitForSpawnFromFile);

        foreach(var c in cards.discardCards){
            var scriptableCard = Resources.Load<ScriptableCard>(c);
            cardList.Add(_gameManager.SpawnCardAndAddToCollection(p, scriptableCard, CardLocation.Discard));
        }
        p.RpcShowSpawnedCards(cardList, CardLocation.Discard, true);
        
        await UniTask.Delay(SorsTimings.waitForSpawnFromFile);
    }

    private async UniTask SpawnEntitiesFromFile(PlayerManager p, Entities entities)
    {
        var entitiesDict = new Dictionary<GameObject, BattleZoneEntity>();

        foreach(var e in entities.creatures){
            var scriptableCard = Resources.Load<ScriptableCard>(e.scriptableCard);
            var cardObject = _gameManager.SpawnCardAndAddToCollection(p, scriptableCard, CardLocation.PlayZone);
            
            // Wait for card initialization
            await UniTask.Delay(10);
            var entity = _gameManager.SpawnFieldEntity(p, cardObject);

            // Wait for entity initialization
            await UniTask.Delay(10);
            entity.Health = e.health;
            entity.GetComponent<CreatureEntity>().Attack = e.attack;

            entitiesDict.Add(cardObject, entity);
        }

        foreach(var e in entities.technologies){
            var scriptableCard = Resources.Load<ScriptableCard>(e.scriptableCard);
            var cardObject = _gameManager.SpawnCardAndAddToCollection(p, scriptableCard, CardLocation.PlayZone);
            
            // Wait for card initialization
            await UniTask.Delay(10);
            var entity = _gameManager.SpawnFieldEntity(p, cardObject);
            
            // Wait for entity initialization
            await UniTask.Delay(10);
            entity.Health = e.health;

            entitiesDict.Add(cardObject, entity);
        }

        p.RpcShowSpawnedCards(entitiesDict.Keys.ToList(), CardLocation.PlayZone, true);
        BoardManager.Instance.PlayEntities(entitiesDict);
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
