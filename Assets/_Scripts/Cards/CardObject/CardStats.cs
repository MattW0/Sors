using Mirror;


public class CardStats : NetworkBehaviour
{
    public PlayerManager owner { get; private set; }
    public CardInfo cardInfo;
    private CardUI _cardUI;

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
        
        _cardUI = gameObject.GetComponent<CardUI>();
    }

    [ClientRpc]
    public void RpcSetCardStats(CardInfo card){
        cardInfo = card;
        _cardUI.SetCardUI(card);
    }

    public void SetHighlight() => _cardUI.Highlight(IsInteractable, SorsColors.interactionHighlight);
}
