using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class InteractionPanel : NetworkBehaviour
{
    public static InteractionPanel Instance { get; private set; }
    [SerializeField] private CardGrid _selectedGrid;
    private Hand _playerHand;
    private InteractionUI _ui;
    private PlayerManager _player;

    [Header("Helper Fields")]
    private List<DetailCard> _detailCards = new();
    private List<CardInfo> _selectedCards = new();
    // Linking detail cards with their hand card gameobject
    private Dictionary<CardInfo, GameObject> _cache = new();

    private void Awake(){
        if (Instance == null) Instance = this;
    }

    private void Start(){
        _playerHand = Hand.Instance;
    }

    [ClientRpc]
    public void RpcPrepareInteractionPanel(int nbCardsToDiscard){
        _ui = gameObject.GetComponent<InteractionUI>();
        _ui.PrepareInteractionPanel(nbCardsToDiscard);
        _player = PlayerManager.GetLocalPlayer();
    }

    [TargetRpc]
    public void TargetStartInteraction(NetworkConnection target, TurnState turnState, 
                                         List<GameObject> cardObjects, List<CardInfo> cardInfos, int numberPlays)
    {

        _playerHand.StartInteraction(turnState);
        _ui.InteractionBegin(turnState, numberPlays);

        // caching hand cards gameobjects
        // for(var i=0; i<cardInfos.Count; i++) _cache.Add(cardInfos[i], cardObjects[i]);
        
        // var detailCards = _cardSpawner.SpawnDetailCardObjects(cardInfos, turnState);
        // _detailCards.AddRange(detailCards);
    }

    #region States
    [TargetRpc]
    public void TargetBeginPrevailSelection(NetworkConnection conn, TurnState turnState, int nbCardsToTrash){
        foreach(var card in _detailCards) card.SetCardState(turnState);

        if (turnState == TurnState.CardIntoHand) _ui.BeginCardIntoHand(nbCardsToTrash);
        else if (turnState == TurnState.Trash) _ui.BeginTrash(nbCardsToTrash);
    }

    [TargetRpc]
    public void TargetCheckPlayability(NetworkConnection target, int currentCash){
        foreach (var card in _detailCards) card.CheckPlayability(currentCash);
    }

    public void ConfirmPlay(){
        var card = _cache[_selectedCards[0]];
        _player.CmdPlayCard(card);
    }

    public void ConfirmDiscard(){
        var cards = _selectedCards.Select(card => _cache[card]).ToList();
        _player.CmdDiscardSelection(cards);
    }

    public void ConfirmPrevailCardSelection(){
        var cards = _selectedCards.Select(card => _cache[card]).ToList();
        _player.CmdPrevailCardsSelection(cards);
    }

    
    #endregion
    public void AddCardToChosen(Transform t, CardInfo card){
        _selectedCards.Add(card);
        _ui.UpdateInteractionElements(_selectedCards.Count);
        
        // _cardSpawner.SelectCard(t);
        _selectedGrid.AddCard(t);
    }

    public void RemoveCardFromChosen(Transform t, CardInfo card){
        _selectedCards.Remove(card);
        _ui.UpdateInteractionElements(_selectedCards.Count);

        // _selectablesGrid.AddCard(t);
        // _cardSpawner.DeselectCard(t);
    }

    public void SkipCardPlay() => _player.CmdSkipCardPlay();
    public void PlayerSkipsPrevailOption() => _player.CmdPlayerSkipsPrevailOption();

    [ClientRpc]
    public void RpcResetPanel(){
        ClearPanel();
        _ui.ResetPanelUI(true);
    }

    [ClientRpc]
    public void RpcSoftResetPanel(){
        ClearPanel();
        _ui.ResetPanelUI(false);
    }

    public void ClearPanel()
    {
        _selectedGrid.ClearGrid();
        _playerHand.EndInteraction();

        // _cardSpawner.ClearGrids();
        _detailCards.Clear();
        _selectedCards.Clear();
        _cache.Clear();
    }
}

public enum InteractionType
{
    Play,
    Discard,
    Prevail
}
