using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

[CreateAssetMenu(fileName = "InteractionStateConfig", menuName = "Sors/Interaction/InteractionStateConfig")]
public class InteractionStateConfig : ScriptableObject
{    
    [Header("Display Settings")]
    public TurnState turnState;
    public CardLocation interactionPile = CardLocation.Hand;
    public InteractionType interactionType;
    public bool isUpTo;
    
    [Header("Button Configuration")]
    public bool confirmButtonEnabled;
    public bool skipButtonVisible = true;
    public bool skipButtonEnabled = true;
    public bool resetButtonVisible = false;
    public bool resetButtonEnabled = false;
} 

public enum InteractionType {
    Select,
    Buy,
    Play,
    Combat
}