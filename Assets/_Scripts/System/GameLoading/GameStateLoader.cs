using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SorsGameState;

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
        LoadKingdomFromFile(gameState.market);
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
        foreach (var player in _gameManager.players.Keys)
        {
            // p has player info and game state 
            Player p = new Player();
            if(player.isLocalPlayer) p = host;
            else p = client;

            playerCards.Add(player, p.cards);
            playerEntities.Add(player, p.entities);
        }

        StartCoroutine(SpawningFromFile(playerCards, playerEntities));
    }

    private IEnumerator SpawningFromFile(Dictionary<PlayerManager, Cards> playerCards, Dictionary<PlayerManager, Entities> playerEntities)
    {
        foreach(var (player, cards) in playerCards){
            StartCoroutine(SpawnCardsFromFile(player, cards));

            yield return new WaitForSeconds(SorsTimings.waitForSpawnFromFile);
            // yield return new WaitForSeconds(0.1f);

            StartCoroutine(SpawnEntitiesFromFile(player, playerEntities[player]));
        }

        yield return new WaitForSeconds(SorsTimings.waitForSpawnFromFile / 3f);
        // yield return new WaitForSeconds(0.1f);

        _gameManager.StartGame();
    }

    private IEnumerator SpawnCardsFromFile(PlayerManager p, Cards cards)
    {
        List<GameObject> cardList = new();
        foreach(var c in cards.handCards){
            var scriptableCard = Resources.Load<ScriptableCard>(c);
            cardList.Add(_gameManager.SpawnCardAndAddToCollection(p, scriptableCard, CardLocation.Hand));
        }
        p.RpcShowSpawnedCards(cardList, CardLocation.Hand, true);

        yield return new WaitForSeconds(SorsTimings.waitForSpawnFromFile / 3f);
        
        // Tracking hand cards on each client for UI stuff
        // TODO: Could replace this with observer pattern maybe?
        p.TargetSetHandCards(p.connectionToClient, cardList);
        cardList.Clear();

        foreach(var c in cards.deckCards){
            var scriptableCard = Resources.Load<ScriptableCard>(c);
            cardList.Add(_gameManager.SpawnCardAndAddToCollection(p, scriptableCard, CardLocation.Deck));
        }
        p.RpcShowSpawnedCards(cardList, CardLocation.Deck, true);
        cardList.Clear();

        yield return new WaitForSeconds(SorsTimings.waitForSpawnFromFile / 3f);

        foreach(var c in cards.discardCards){
            var scriptableCard = Resources.Load<ScriptableCard>(c);
            cardList.Add(_gameManager.SpawnCardAndAddToCollection(p, scriptableCard, CardLocation.Discard));
        }
        p.RpcShowSpawnedCards(cardList, CardLocation.Discard, true);

        yield return new WaitForSeconds(SorsTimings.waitForSpawnFromFile / 3f);
    }

    private IEnumerator SpawnEntitiesFromFile(PlayerManager p, Entities entities)
    {
        var entitiesDict = new Dictionary<GameObject, BattleZoneEntity>();

        foreach(var e in entities.creatures){
            var scriptableCard = Resources.Load<ScriptableCard>(e.scriptableCard);
            var cardObject = _gameManager.SpawnCardAndAddToCollection(p, scriptableCard, CardLocation.PlayZone);
            
            // Wait for card initialization
            yield return new WaitForSeconds(0.01f);
            var entity = _gameManager.SpawnFieldEntity(p, cardObject);

            // Wait for entity initialization
            yield return new WaitForSeconds(0.01f);
            entity.Health = e.health;
            entity.GetComponent<CreatureEntity>().Attack = e.attack;
            // entity.GetComponent<CreatureEntity>().SetDefense(e.defense);

            entitiesDict.Add(cardObject, entity);
        }

        foreach(var e in entities.technologies){
            var scriptableCard = Resources.Load<ScriptableCard>(e.scriptableCard);
            var cardObject = _gameManager.SpawnCardAndAddToCollection(p, scriptableCard, CardLocation.PlayZone);
            
            // Wait for card initialization
            yield return new WaitForSeconds(0.01f);
            var entity = _gameManager.SpawnFieldEntity(p, cardObject);
            
            // Wait for entity initialization
            yield return new WaitForSeconds(0.01f);
            entity.Health = e.health;

            entitiesDict.Add(cardObject, entity);
        }

        p.RpcShowSpawnedCards(entitiesDict.Keys.ToList(), CardLocation.PlayZone, true);
        BoardManager.Instance.PlayEntities(entitiesDict);
    }

    private void LoadKingdomFromFile(Market market)
    {
        var _kingdom = Kingdom.Instance;
        _kingdom.RpcSetPlayer();

        // Money
        var moneyCards = new CardInfo[market.money.Count];
        for (var i = 0; i < market.money.Count; i++) 
            moneyCards[i] = new CardInfo(Resources.Load<ScriptableCard>(market.money[i]));
        _kingdom.RpcSetMoneyTiles(moneyCards);

        // Technologies
        var technologies = new CardInfo[market.technologies.Count];
        for (var i = 0; i < market.technologies.Count; i++)
            technologies[i] = new CardInfo(Resources.Load<ScriptableCard>(market.technologies[i]));
        _kingdom.RpcSetTechnologyTiles(technologies);

        // Creatures
        var creatures = new CardInfo[market.creatures.Count];
        for (var i = 0; i < market.creatures.Count; i++)
            creatures[i] = new CardInfo(Resources.Load<ScriptableCard>(market.creatures[i]));
        _kingdom.RpcSetCreatureTiles(creatures);
    }
}
