using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;


public class CardCollectionPanel : NetworkBehaviour
{
    public static CardCollectionPanel Instance { get; private set; }
    [SerializeField] private CardCollectionPanelUI _ui;
    [SerializeField] private CardSpawner _cardSpawner;
    private PlayerManager _player;

    [Header("Helper Fields")]
    
    private List<DetailCard> _detailCards = new();
    private List<CardInfo> _selectedCards = new();
    // Linking detail cards with their hand card gameobject
    private Dictionary<CardInfo, GameObject> _cache = new();

    private void Awake(){
        if (Instance == null) Instance = this;
    }

    [ClientRpc]
    public void RpcPrepareCardCollectionPanel(int nbCardsToDiscard){
        _ui.PrepareCardCollectionPanelUi(nbCardsToDiscard);
        _player = PlayerManager.GetLocalPlayer();
    }

    [TargetRpc]
    public void TargetShowCardCollection(NetworkConnection target, TurnState turnState, 
                                         List<GameObject> cardObjects, List<CardInfo> handCards)
    {
        var targetCardType = CardType.None;
        if(turnState == TurnState.Develop) targetCardType = CardType.Technology;
        else if(turnState == TurnState.Deploy) targetCardType = CardType.Creature;

        var detailCardObjects = new List<GameObject>();
        for(var i=0; i<handCards.Count; i++){

            var cardInfo = handCards[i];
            _cache.Add(cardInfo, cardObjects[i]);

            if(targetCardType == CardType.None){
                // Show all cards
                var detailCard = _cardSpawner.SpawnDetailCardObject(cardInfo);
                _detailCards.Add(detailCard);
                continue;
            }

            // Else show only cards of the target type and money in seperate grids
            // if(cardInfo.type == CardType.Money) moneyCards.Add(card);
            // else if(cardInfo.type == targetCardType) entityCards.Add(card);
        }

        _ui.ToggleView();
    }

    private void SpawnMoneyDetailCard(CardInfo cardInfo){

    }

    #region States
    [ClientRpc]
    public void RpcBeginState(TurnState state){
        foreach(var card in _detailCards) card.SetCardState(state);
        _ui.InteractionBegin(state);
    }

    [TargetRpc]
    public void TargetBeginTrash(NetworkConnection conn, int nbCardsToTrash){
        foreach(var card in _detailCards) card.SetCardState(TurnState.Trash);

        _ui.BeginTrash(nbCardsToTrash);
    }

    [TargetRpc]
    public void TargetCheckPlayability(NetworkConnection target, int currentCash){
        foreach (var card in _detailCards) card.CheckPlayability(currentCash);
    }

    public void ConfirmPlay(){
        var card = _cache[_selectedCards[0]];
        _player.CmdPlayCard(card);
        _detailCards.Remove(card.GetComponent<DetailCard>());

        // _detailCards.Remove(card.GetComponent<DetailCard>());
        _selectedCards.Clear();
        _cardSpawner.ClearChosenGrid();
    }

    public void ConfirmDiscard(){
        var cards = _selectedCards.Select(card => _cache[card]).ToList();
        _player.CmdDiscardSelection(cards);
    }

    public void ConfirmTrash(){
        var cards = _selectedCards.Select(card => _cache[card]).ToList();
        _player.CmdTrashSelection(cards);
    }

    
    #endregion

    public void AddCardToChosen(Transform t, CardInfo card){
        t.SetParent(_gridChosen, false);
        _selectedCards.Add(card);
        _ui.UpdateInteractionElements(_selectedCards.Count);
    }

    public void RemoveCardFromChosen(Transform t, CardInfo card){
        t.SetParent(_gridAll, false);
        _selectedCards.Remove(card);
        _ui.UpdateInteractionElements(_selectedCards.Count);
    }

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

    public void ClearPanel(){
        _cardSpawner.ClearGrids();
        _detailCards.Clear();
        _selectedCards.Clear();
        _cache.Clear();
    }

    public void ToggleView() => _ui.ToggleView();
}
