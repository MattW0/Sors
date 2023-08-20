using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;


public class CardCollectionPanel : NetworkBehaviour
{
    public static CardCollectionPanel Instance { get; private set; }
    [SerializeField] private CardCollectionPanelUI _ui;
    private PlayerManager _player;

    [Header("Helper Fields")]
    [SerializeField] private GameObject _creatureDetailCardPrefab;
    [SerializeField] private GameObject _technologyDetailCardPrefab;
    [SerializeField] private GameObject _moneyDetailCardPrefab;
    [SerializeField] private Transform _gridAll;
    [SerializeField] private Transform _gridChosen;
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
    public void TargetShowCardCollection(NetworkConnection target, List<GameObject> cardObjects, List<CardInfo> cardInfos){
        for (var i=0; i<cardInfos.Count; i++) SpawnDetailCardObject(cardObjects[i], cardInfos[i]);
        _ui.ToggleView();
    }

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

    private void SpawnDetailCardObject(GameObject card, CardInfo cardInfo){
        _cache.Add(cardInfo, card);

        
        var detailCardObject = cardInfo.type switch{
            CardType.Creature => Instantiate(_creatureDetailCardPrefab) as GameObject,
            CardType.Technology => Instantiate(_technologyDetailCardPrefab) as GameObject,
            CardType.Money => Instantiate(_moneyDetailCardPrefab) as GameObject,
            _ => throw new System.Exception("Card type not found")
        };
        detailCardObject.transform.SetParent(_gridAll, false);

        var detailCard = detailCardObject.GetComponent<DetailCard>();
        detailCard.SetCardUI(cardInfo);
        _detailCards.Add(detailCard);
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

    public void ConfirmDiscard(){
        var cards = _selectedCards.Select(card => _cache[card]).ToList();
        _player.CmdDiscardSelection(cards);
    }

    [TargetRpc]
    public void TargetCheckPlayability(NetworkConnection target, int currentCash){
        foreach (var card in _detailCards) card.CheckPlayability(currentCash);
    }

    public void ConfirmPlay(){
        var card = _cache[_selectedCards[0]];
        _player.CmdPlayCard(card);

        // _detailCards.Remove(card.GetComponent<DetailCard>());
        _selectedCards.Clear();
        foreach (Transform child in _gridChosen) {
            _detailCards.Remove(child.gameObject.GetComponent<DetailCard>());
            Destroy(child.gameObject);
        }
    }

    public void ConfirmTrash(){
        var cards = _selectedCards.Select(card => _cache[card]).ToList();
        _player.CmdTrashSelection(cards);
    }

    
    #endregion

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
        foreach (Transform child in _gridAll) Destroy(child.gameObject);
        foreach (Transform child in _gridChosen) Destroy(child.gameObject);
        _detailCards.Clear();
        _selectedCards.Clear();
        _cache.Clear();
    }

    public void ToggleView() => _ui.ToggleView();
}
