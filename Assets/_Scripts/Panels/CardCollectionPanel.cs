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
    [SerializeField] private GameObject _detailCardPrefab;
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
    public void RpcPrepareCardCollectionPanel(int nbCardsToDiscard)
    {
        _ui.PrepareCardCollectionPanelUi(nbCardsToDiscard);
        _player = PlayerManager.GetLocalPlayer();
    }

    [TargetRpc]
    public void TargetShowCardCollection(NetworkConnection target, List<GameObject> cardObjects, List<CardInfo> cardInfos)
    {
        for (var i=0; i<cardInfos.Count; i++) SpawnDetailCardObject(cardObjects[i], cardInfos[i]);
        _ui.Open();
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

        var detailCardObject = Instantiate(_detailCardPrefab) as GameObject;
        detailCardObject.transform.SetParent(_gridAll, false);

        var detailCard = detailCardObject.GetComponent<DetailCard>();
        detailCard.SetCardUI(cardInfo);
        _detailCards.Add(detailCard);
    }

    #region States
    [ClientRpc]
    public void RpcBeginState(TurnState state){
        foreach(var card in _detailCards) card.SetCardState(state);

        if (state == TurnState.Discard) _ui.BeginDiscard();
        else if (state == TurnState.Deploy) _ui.BeginDeploy();
    }

    [TargetRpc]
    public void TargetBeginTrash(NetworkConnection conn, int nbCardsToTrash)
    {
        foreach(var card in _detailCards) card.SetCardState(TurnState.Trash);

        _ui.BeginTrash(nbCardsToTrash);
    }

    public void ConfirmDiscard(){
        var cards = _selectedCards.Select(card => _cache[card]).ToList();
        _player.CmdDiscardSelection(cards);
    }

    [TargetRpc]
    public void TargetCheckDeployability(NetworkConnection target, int currentCash){
        foreach (var card in _detailCards) card.CheckDeployability(currentCash);
    }

    public void ConfirmDeploy(){
        var card = _cache[_selectedCards[0]];
        _player.CmdDeployCard(card);

        // _detailCards.Remove(card.GetComponent<DetailCard>());
        foreach (Transform child in _gridChosen) {
            _selectedCards.Clear();
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
        foreach (Transform child in _gridChosen) child.SetParent(_gridAll, false);
        _selectedCards.Clear();
        _ui.ResetPanelUI(false);
    }

    public void ClearPanel(){
        foreach (Transform child in _gridAll) Destroy(child.gameObject);
        foreach (Transform child in _gridChosen) Destroy(child.gameObject);
        _detailCards.Clear();
        _selectedCards.Clear();
        _cache.Clear();
    }
}
