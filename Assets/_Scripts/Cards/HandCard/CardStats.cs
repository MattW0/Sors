using Mirror;


public class CardStats : NetworkBehaviour
{
    public PlayerManager owner { get; private set; }
    public CardInfo cardInfo;
    private HandCardUI _cardUI;

    private bool _interactable;
    public bool IsInteractable { 
        get => _interactable;
        set {
            _interactable = value;
            _cardUI.Highlight(value, SorsColors.interactionHighlight);
        }
    }
    
    private void Awake()
    {
        var networkIdentity = NetworkClient.connection.identity;
        owner = networkIdentity.GetComponent<PlayerManager>();
        
        _cardUI = gameObject.GetComponent<HandCardUI>();
    }

    [ClientRpc]
    public void RpcSetCardStats(CardInfo card)
    {
        // Will be set active by cardMover, once card is spawned correctly in UI
        gameObject.SetActive(false);

        cardInfo = card;
        _cardUI.SetCardUI(card);
        gameObject.GetComponent<CardClick>().SetCardInfo(card);
    }
}
