using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using Random = UnityEngine.Random;

public class Market : NetworkBehaviour
{
    public static Market Instance { get; private set; }

    [Header("Buy phase")]
    [SerializeField] private TurnState _currentPhase;
    [SerializeField] private MarketTile _selectedTile;

    [Header("Available Cards")]
    [SerializeField] private ScriptableCard[] _startEntities;
    [SerializeField] private ScriptableCard[] _moneyCardsDb;
    [SerializeField] private ScriptableCard[] _creatureCardsDb;
    [SerializeField] private ScriptableCard[] _technologyCardsDb;

    [Header("UI Elements")]
    [SerializeField] private MarketUI _ui;
    [SerializeField] private MarketTile[] _moneyTiles;
    [SerializeField] private MarketTile[] _technologyTiles;
    [SerializeField] private MarketTile[] _creatureTiles;

    private List<int> _availableCreatureIds = new();
    private List<int> _availableTechnologyIds = new();
    public static event Action OnMarketPhaseEnded;

    private void Awake(){
        if (Instance == null) Instance = this;

        MarketTile.OnTileSelected += PlayerSelectsTile;
        MarketTile.OnTileDeselected += PlayerDeselectsTile;
        InteractionPanel.OnCheckMarketPrices += CheckMarketPrices;
    }

    private void Start(){

        // Databases of generated cards
        _startEntities = Resources.LoadAll<ScriptableCard>("Cards/_StartCards/");
        _moneyCardsDb = Resources.LoadAll<ScriptableCard>("Cards/MoneyCards/");
        _creatureCardsDb = Resources.LoadAll<ScriptableCard>("Cards/CreatureCards/");        
        _technologyCardsDb = Resources.LoadAll<ScriptableCard>("Cards/TechnologyCards/");
    }

    #region Setup

    [Server]
    public void InitializeMarket()
    {
        // Money
        var moneyCards = new CardInfo[_moneyTiles.Length];
        for (var i = 0; i < _moneyTiles.Length; i++)
            moneyCards[i] = new CardInfo(_moneyCardsDb[i]);
        RpcSetMoneyTiles(moneyCards);

        // Technologies
        var technologyCards = new CardInfo[_technologyTiles.Length];
        for (var i = 0; i < _technologyTiles.Length; i++)
            technologyCards[i] = GetNewTechnologyFromDb();
        RpcSetTechnologyTiles(technologyCards);

        // Creatures
        var creatureCards = new CardInfo[_creatureTiles.Length];
        for (var i = 0; i < _creatureTiles.Length; i++)
            creatureCards[i] = GetNewCreatureFromDb();
        RpcSetCreatureTiles(creatureCards);
    }

    // Public for GameStateLoader    
    [ClientRpc]
    public void RpcSetMoneyTiles(CardInfo[] moneyTilesInfo){
        for (var i = 0; i < moneyTilesInfo.Length; i++) 
            _moneyTiles[i].InitializeTile(moneyTilesInfo[i], i);
    }

    [ClientRpc]
    public void RpcSetTechnologyTiles(CardInfo[] technologyTilesInfo){
        for (var i = 0; i < technologyTilesInfo.Length; i++) 
            _technologyTiles[i].InitializeTile(technologyTilesInfo[i], i);
    }

    [ClientRpc]
    public void RpcSetCreatureTiles(CardInfo[] creatureTilesInfo){   
        for (var i = 0; i < creatureTilesInfo.Length; i++) 
            _creatureTiles[i].InitializeTile(creatureTilesInfo[i], i);
    }

    [ClientRpc]
    public void RpcBeginMarketPhase(TurnState phase)
    {
        _currentPhase = phase;
        _ui.BeginPhase(phase);
    }
    #endregion

    #region Tile Cost

    [TargetRpc]
    public void TargetMarketPriceReduction(NetworkConnection target, CardType type, int priceReduction)
    {
        if (type == CardType.Money){
            foreach(var tile in _moneyTiles) tile.SetBonus(priceReduction);
        } else if (type == CardType.Technology){
            foreach(var tile in _technologyTiles) tile.SetBonus(priceReduction);
        } else if (type == CardType.Creature){
            foreach(var tile in _creatureTiles) tile.SetBonus(priceReduction);
        }
    }

    [TargetRpc]
    public void TargetCheckMarketPrices(NetworkConnection target, int playerCash)
    {
        CheckMarketPrices(playerCash);
    }

    private void CheckMarketPrices(int playerCash) 
    {
        // Can always buy money cards
        foreach(var tile in _moneyTiles) tile.Interactable = playerCash >= tile.Cost;

        if (_currentPhase == TurnState.Invent){
            foreach (var tile in _technologyTiles) tile.Interactable = playerCash >= tile.Cost;
            foreach (var tile in _creatureTiles) tile.Interactable = false;
        } else if (_currentPhase == TurnState.Recruit){
            foreach (var tile in _creatureTiles) tile.Interactable = playerCash >= tile.Cost;
            foreach (var tile in _technologyTiles) tile.Interactable = false;
        }
    }
    #endregion
    
    public void PlayerSelectsTile(MarketTile tile)
    {
        // Reset all other tiles -> single selection
        foreach (var t in _moneyTiles) if (t != tile) t.ResetSelected();
        if (_currentPhase == TurnState.Invent){
            foreach (var t in _technologyTiles) if (t != tile) t.ResetSelected();
        } else if (_currentPhase == TurnState.Recruit){
            foreach (var t in _creatureTiles) if (t != tile) t.ResetSelected();
        }

        _selectedTile = tile;
    }

    public void PlayerDeselectsTile() => _selectedTile = null;

    #region Reset and EoP
    [TargetRpc]
    public void TargetResetMarket(NetworkConnection target, int actionsLeft)
    {
        _selectedTile.HasBeenChosen();
        PlayerDeselectsTile();
    }

    public void EndMarketPhase(List<(int, CardType)> boughtCards)
    {
        foreach (var (index, type) in boughtCards)
        {
            if(type == CardType.Money) continue;

            if(type == CardType.Technology)
                RpcSetTile(type, index, GetNewTechnologyFromDb());
            else
                RpcSetTile(type, index, GetNewCreatureFromDb());
        }

        RpcEndMarketPhase();
    }

    [ClientRpc]
    public void RpcSetTile(CardType type, int index, CardInfo cardInfo)
    {
        if (type == CardType.Money) {}
        else if (type == CardType.Technology) _technologyTiles[index].SetTile(cardInfo);
        else _creatureTiles[index].SetTile(cardInfo);
    }
    
    [ClientRpc]
    public void RpcEndMarketPhase()
    {
        _ui.EndPhase();
        OnMarketPhaseEnded?.Invoke();
    }
    #endregion

    [ClientRpc] public void RpcMinButton() => _ui.MinButton();
    [ClientRpc] public void RpcMaxButton() => _ui.MaxButton();
    public void MaxButton() => _ui.MaxButton();

    public List<CardInfo>[] GetTileInfos()
    {
        var scriptableTiles = new List<CardInfo>[3];
        scriptableTiles[0] = new List<CardInfo>();
        scriptableTiles[1] = new List<CardInfo>();
        scriptableTiles[2] = new List<CardInfo>();

        foreach (var tile in _moneyTiles) scriptableTiles[0].Add(tile.cardInfo);
        foreach (var tile in _technologyTiles) scriptableTiles[1].Add(tile.cardInfo);
        foreach (var tile in _creatureTiles) scriptableTiles[2].Add(tile.cardInfo);

        return scriptableTiles;
    }

    public CardInfo GetNewTechnologyFromDb()
    {
        if(_availableTechnologyIds.Count == 0){
            // Random order of ids -> pop first element for random card
            _availableTechnologyIds = Enumerable.Range(0, _technologyCardsDb.Length)
                                        .OrderBy(x => Random.value)
                                        .ToList();
        }

        var id = _availableTechnologyIds[0];
        _availableTechnologyIds.RemoveAt(0);
        return new CardInfo(_technologyCardsDb[id]);
    }

    public CardInfo GetNewCreatureFromDb()
    {
        if(_availableCreatureIds.Count == 0){
            // Random order of ids -> pop first element for random card
            _availableCreatureIds = Enumerable.Range(0, _creatureCardsDb.Length)
                                        .OrderBy(x => Random.value)
                                        .ToList();
        }

        var id = _availableCreatureIds[0];
        _availableCreatureIds.RemoveAt(0);
        return new CardInfo(_creatureCardsDb[id]);
    }

    internal ScriptableCard GetStartMoneyCard() => _moneyCardsDb[0];
    internal ScriptableCard[] GetStartEntities() => _startEntities;

    private void OnDestroy()
    {
        MarketTile.OnTileSelected -= PlayerSelectsTile;
        MarketTile.OnTileDeselected -= PlayerDeselectsTile;
        InteractionPanel.OnCheckMarketPrices -= CheckMarketPrices;

    }
}
