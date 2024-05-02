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
    [SerializeField] private GameManager _gameManager;
    private PlayerManager _player;
    private Phase _currentPhase;

    [SerializeField] private MarketUI _ui;
    [SerializeField] private MarketTile[] moneyTiles;
    [SerializeField] private MarketTile[] technologyTiles;
    [SerializeField] private MarketTile[] creatureTiles;
    [SerializeField] private GameObject moneyGrid;
    [SerializeField] private GameObject developmentsGrid;
    [SerializeField] private GameObject creaturesGrid;
    private MarketTile _selectedTile;

    [Header("Available Cards")]

    [SerializeField] private ScriptableCard[] _startEntities;
    [SerializeField] private ScriptableCard[] _moneyCardsDb;
    [SerializeField] private ScriptableCard[] _creatureCardsDb;
    [SerializeField] private ScriptableCard[] _technologyCardsDb;
    private List<int> _availableCreatureIds = new();
    private List<int> _availableTechnologyIds = new();

    public static event Action OnMarketPhaseEnded;

    private void Awake(){
        if (Instance == null) Instance = this;
    }

    private void Start(){
        // _marketTiles = GetComponentsInChildren<MarketTile>();
        moneyTiles = moneyGrid.GetComponentsInChildren<MarketTile>();
        technologyTiles = developmentsGrid.GetComponentsInChildren<MarketTile>();
        creatureTiles = creaturesGrid.GetComponentsInChildren<MarketTile>();

        // Databases of generated cards
        _startEntities = Resources.LoadAll<ScriptableCard>("Cards/_StartCards/");
        _moneyCardsDb = Resources.LoadAll<ScriptableCard>("Cards/MoneyCards/");
        _creatureCardsDb = Resources.LoadAll<ScriptableCard>("Cards/CreatureCards/");
        _technologyCardsDb = Resources.LoadAll<ScriptableCard>("Cards/TechnologyCards/");
    }

    #region Setup

    [ClientRpc]
    public void RpcInitializeMarket()
    {
        _player = PlayerManager.GetLocalPlayer();

        // Money
        for (var i = 0; i < moneyTiles.Length; i++) 
            moneyTiles[i].InitializeTile(new CardInfo(_moneyCardsDb[i]), i);

        // Technologies
        for (var i = 0; i < technologyTiles.Length; i++)
            technologyTiles[i].InitializeTile(new CardInfo(_technologyCardsDb[i]), i);

        // Creatures
        for (var i = 0; i < creatureTiles.Length; i++)
            creatureTiles[i].InitializeTile(new CardInfo(_creatureCardsDb[i]), i);
    }

    [ClientRpc]
    public void RpcSetTile(CardType type, int index, CardInfo cardInfo)
    {
        if (type == CardType.Money) {}
        else if (type == CardType.Technology) technologyTiles[index].SetTile(cardInfo);
        else creatureTiles[index].SetTile(cardInfo);
    }

    [ClientRpc]
    public void RpcBeginPhase(Phase phase){
        _currentPhase = phase;
        _ui.BeginPhase(phase);
    }
    #endregion

    #region Tile Cost

    [TargetRpc]
    public void TargetMarketPriceReduction(NetworkConnection target, CardType type, int priceReduction){
        if (type == CardType.Money){
            foreach(var tile in moneyTiles) tile.SetBonus(priceReduction);
        } else if (type == CardType.Technology){
            foreach(var tile in technologyTiles) tile.SetBonus(priceReduction);
        } else if (type == CardType.Creature){
            foreach(var tile in creatureTiles) tile.SetBonus(priceReduction);
        }
    }

    [TargetRpc]
    public void TargetCheckMarketPrices(NetworkConnection target, int playerCash)
    {
        // Can always buy money cards
        foreach(var tile in moneyTiles) tile.Interactable = playerCash >= tile.Cost;

        if (_currentPhase == Phase.Invent){
            foreach (var tile in technologyTiles) tile.Interactable = playerCash >= tile.Cost;
            foreach (var tile in creatureTiles) tile.Interactable = false;
        } else if (_currentPhase == Phase.Recruit){
            foreach (var tile in creatureTiles) tile.Interactable = playerCash >= tile.Cost;
            foreach (var tile in technologyTiles) tile.Interactable = false;
        }
    }
    #endregion
    
    public void PlayerSelectsTile(MarketTile tile)
    {
        PlayerDeselectsTile();

        _selectedTile = tile;
        _ui.SelectTile(tile.cardInfo);

        // Reset all other tiles -> single selection
        foreach (var t in moneyTiles) if (t != tile) t.ResetSelected();
        if (_currentPhase == Phase.Invent){
            foreach (var t in technologyTiles) if (t != tile) t.ResetSelected();
        } else if (_currentPhase == Phase.Recruit){
            foreach (var t in creatureTiles) if (t != tile) t.ResetSelected();
        }        
    }

    public void PlayerDeselectsTile(){
        _selectedTile = null;
        _ui.DeselectTile();
    }

    #region Reset and EoP
    [TargetRpc]
    public void TargetResetMarket(NetworkConnection target, int actionsLeft)
    {
        _selectedTile.HasBeenChosen();
        PlayerDeselectsTile();
        _ui.ResetInteractionButtons();
    }

    public void ResetMarket(List<(int, CardType)> boughtCards){
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
    public void RpcEndMarketPhase(){
        _ui.EndPhase();
        OnMarketPhaseEnded?.Invoke();
    }
    #endregion

    public void PlayerPressedConfirmButton()
    {        
        // Need the cost here as market bonus are not reflected in cardInfo itself
        if(_selectedTile) _player.CmdConfirmBuy(_selectedTile.cardInfo, _selectedTile.Cost, _selectedTile.Index);
        else print("ERROR: No tile selected");
    }

    public void PlayerPressedSkipButton() => _player.CmdSkipBuy();

    [ClientRpc]
    public void RpcMinButton() => _ui.MinButton();

    [ClientRpc]
    public void RpcMaxButton() => _ui.MaxButton();
    public void MaxButton() => _ui.MaxButton();

    public List<CardInfo>[] GetTileInfos(){

        var scriptableTiles = new List<CardInfo>[3];
        scriptableTiles[0] = new List<CardInfo>();
        scriptableTiles[1] = new List<CardInfo>();
        scriptableTiles[2] = new List<CardInfo>();

        foreach (var tile in moneyTiles) scriptableTiles[0].Add(tile.cardInfo);
        foreach (var tile in technologyTiles) scriptableTiles[1].Add(tile.cardInfo);
        foreach (var tile in creatureTiles) scriptableTiles[2].Add(tile.cardInfo);

        return scriptableTiles;
    }

    // FOR GAME STATE LOADING
    // TODO: Change this to conform with new way of loading from gameManager
    [ClientRpc]
    public void RpcSetPlayer()
    {
        _player = PlayerManager.GetLocalPlayer();
    }
    
    [ClientRpc]
    public void RpcSetMoneyTiles(CardInfo[] moneyTilesInfo){
        for (var i = 0; i < moneyTilesInfo.Length; i++) 
            moneyTiles[i].InitializeTile(moneyTilesInfo[i], i);
    }

    [ClientRpc]
    public void RpcSetTechnologyTiles(CardInfo[] technologyTilesInfo){
        for (var i = 0; i < technologyTilesInfo.Length; i++) 
            technologyTiles[i].InitializeTile(technologyTilesInfo[i], i);
    }

    [ClientRpc]
    public void RpcSetCreatureTiles(CardInfo[] creatureTilesInfo){   
        for (var i = 0; i < creatureTilesInfo.Length; i++) 
            creatureTiles[i].InitializeTile(creatureTilesInfo[i], i);
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

}
