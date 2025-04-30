
// Each state is defined uniquely by the path to the SO state config
public interface IInteractionState { 
    public string ConfigName { get; }
    public InteractionStateConfig Config { get; }
    // public void Initialize(InteractionPileUI[] piles);
    public void Initialize(InteractionPileUI[] piles);
    public void StartState();
    public void EndState();
}
