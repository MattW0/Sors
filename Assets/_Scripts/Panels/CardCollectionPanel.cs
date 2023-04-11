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
    [SerializeField] private GameObject _container;
    [SerializeField] private Transform _gridAll;
    [SerializeField] private Transform _gridChosen;
    private List<CardInfo> _detailCards = new();
    private List<CardInfo> _chosenCards = new();
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
    public void TargetShowCardCollection(NetworkConnection target, List<GameObject> cardObjects, List<CardInfo> cards){
        for (var i=0; i<cards.Count; i++)
        {
            var cardInfo = cards[i];
            // print("CardInfos: " + cardInfo.title);
            _cache.Add(cardInfo, cardObjects[i]);

            var detailCardObject = Instantiate(_detailCardPrefab) as GameObject;
            detailCardObject.transform.SetParent(_gridAll, false);

            var detailCard = detailCardObject.GetComponent<DetailCard>();
            detailCard.SetCardUI(cardInfo);
            detailCard.isChoosable = true;
            _detailCards.Add(cardInfo);
        }

        _container.SetActive(true);
    }

    public void AddCardToChosen(Transform t, CardInfo card)
    {
        t.SetParent(_gridChosen, false);
        _chosenCards.Add(card);
        _ui.UpdateDiscardPanel(_chosenCards.Count);
    }

    public void RemoveCardFromChosen(Transform t, CardInfo card)
    {
        t.transform.SetParent(_gridAll, false);
        _chosenCards.Remove(card);
        _ui.UpdateDiscardPanel(_chosenCards.Count);
    }

    public void CloseView()
    {
        ClearPanel();
        _container.SetActive(false);
    }
    private void ClearPanel(){
        foreach (Transform child in _gridAll) Destroy(child.gameObject);
        foreach (Transform child in _gridChosen) Destroy(child.gameObject);
        _detailCards.Clear();
        _chosenCards.Clear();
        _cache.Clear();
    }

    #region Phases
    [ClientRpc]
    public void RpcBeginDiscard()
    {
        _ui.BeginDiscard();
    }

    public void ConfirmDiscard(){
        var cards = _chosenCards.Select(card => _cache[card]).ToList();
        _player.CmdDiscardSelection(cards);
    }

    [ClientRpc]
    public void RpcFinishDiscard()
    {
        _ui.FinishDiscard();
        CloseView();
    }

    [TargetRpc]
    public void TargetBeginTrash(NetworkConnection conn, int nbCardsToTrash)
    {
        _ui.BeginTrash(nbCardsToTrash);
    }

    [ClientRpc]
    public void RpcFinishTrash()
    {
        _ui.FinishTrash();
        CloseView();
    }

    [ClientRpc]
    public void RpcBeginDeploy()
    {
        _ui.BeginDeploy();
    }

    [ClientRpc]
    public void RpcFinishDeploy()
    {
        _ui.FinishDeploy();
    }
    #endregion

}
