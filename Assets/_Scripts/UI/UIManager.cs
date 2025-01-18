using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class UIManager : NetworkBehaviour
{
    public static UIManager Instance { get; private set; }
    private TurnManager _turnManager;
    [SerializeField] private EndScreen _endScreen;
    [SerializeField] private AlertDialogue _quitDialog;
    [SerializeField] private GameObject _cardCollectionViewPrefab;
    [SerializeField] private Transform _spawnParentTransform;
    
    [SerializeField] private List<CardListInfo> _openCardLists = new();
    public static SorsColors ColorPalette { get; private set; }

    private void Awake()
    {
        if (!Instance) Instance = this;

        try { ColorPalette = Resources.Load<SorsColors>("ColorDefinitions/Sors Colors"); }
        catch { Debug.Log("No Sors Colors found at Resources/ColorDefinitions/Sors Colors. Assign it manually on " + gameObject.name, this); }

        CardPileClick.OnLookAtCardList += RequestCardList;
        CardListUI.OnCloseCardCollection += CloseCardList;

        PlayerInterfaceButtons.OnQuitButtonClicked += QuitDialog;
        
        AlertDialogue.OnAccept += AlertDialogueAccept;
        AlertDialogue.OnDecline += AlertDialogueDecline;
    }

    private void Start()
    {
        if(!isServer) return;
        _turnManager = TurnManager.Instance;
    }

    private void RequestCardList(CardListInfo listInfo)
    {        
        CmdPlayerOpensCardCollection(PlayerManager.GetLocalPlayer(), listInfo);
    }

    [Command(requiresAuthority = false)]
    private void CmdPlayerOpensCardCollection(PlayerManager player, CardListInfo listInfo)
    {
        print($"Player {player.PlayerName} opens collection {listInfo.location}, owns collection {listInfo.isMine}");

        if (_openCardLists.Contains(listInfo)) return;
        _openCardLists.Add(listInfo);

        var cardList = GetCardList(player, listInfo);

        cardList.OnUpdate += UpdateCardCollection;
        TargetOpenCardCollection(player.connectionToClient, cardList, listInfo);
    }

    [TargetRpc]
    public void TargetOpenCardCollection(NetworkConnection conn, List<CardStats> collection, CardListInfo listInfo)
    {
        print("Collection count on client: " + collection.Count);
        // TODO: Still show when empty?
        if (collection.Count == 0) return;

        var listView = Instantiate(_cardCollectionViewPrefab, _spawnParentTransform).GetComponent<CardListView>();
        listView.OpenCardCollection(collection, listInfo);
    }

    public void UpdateCardCollection(CardListInfo info, List<CardInfo> cards)
    {
        RpcUpdateCardCollection(cards);
    }

    [ClientRpc]
    public void RpcUpdateCardCollection(List<CardInfo> cards)
    {
        print("Updating card collection");
    }

    private void CloseCardList(CardListInfo listInfo)
    {
        CmdCloseCardList(PlayerManager.GetLocalPlayer(), listInfo);
    }

    [Command(requiresAuthority = false)]
    private void CmdCloseCardList(PlayerManager player, CardListInfo listInfo)
    {
        if (!_openCardLists.Contains(listInfo)) return;

        var cardList = GetCardList(player, listInfo);
        cardList.OnUpdate -= UpdateCardCollection;
        _openCardLists.Remove(listInfo);
    }

    private CardList GetCardList(PlayerManager player, CardListInfo listInfo)
    {
        var cardList = _turnManager.GetTrashedCards();
        if (listInfo.location == CardLocation.Discard)
            cardList = listInfo.isMine ? player.Cards.discard : _turnManager.GetOpponentPlayer(player).Cards.discard;
        
        return cardList;
    }

    [ClientRpc]
    public void RpcSetPlayerScore(PlayerManager player, int health, int score) => _endScreen.SetPlayerScore(player, health, score);

    [ClientRpc]
    public void RpcSetGameWinner(PlayerManager player) => _endScreen.SetGameWinner(player);

    [ClientRpc]
    internal void RpcSetDraw() => _endScreen.SetDraw();

    private void QuitDialog() => _quitDialog.WindowIn();

    private void AlertDialogueAccept(ModalWindowType type)
    {
        // TODO: Let other player win 
        Application.Quit();
    }

    private void AlertDialogueDecline(ModalWindowType type)
    {
        _quitDialog.WindowOut();
    }

    private void OnDestroy()
    {
        CardPileClick.OnLookAtCardList -= RequestCardList;
        CardListUI.OnCloseCardCollection -= CloseCardList;

        PlayerInterfaceButtons.OnQuitButtonClicked -= QuitDialog;

        AlertDialogue.OnAccept -= AlertDialogueAccept;
        AlertDialogue.OnDecline -= AlertDialogueDecline;
    }
}
