using Sirenix.Utilities;
using UnityEngine;

[CreateAssetMenu(fileName = "InteractionStateConfig", menuName = "Sors/Interaction/InteractionStateConfig")]
public class InteractionStateConfig : ScriptableObject
{    
    [Header("Display Settings")]
    public TurnState turnState;
    public CardLocation location;
    public InteractionType interactionType;
    public bool isUpTo;
    [SerializeField] private string displayText = "";

    [Header("Display Text Configuration")]
    [Tooltip("{0}: Interaction Verb, {1}: Number of cards, {2}: Card type")]
    [SerializeField] private string displayTextFormat = "{0} {1} {2}card(s)";
    [SerializeField] private CardType allowedCardType;
    [SerializeField] private bool includeMoneyOption;
    
    [Header("Button Configuration")]
    public bool confirmButtonEnabled;
    public bool skipButtonVisible = true;
    public bool skipButtonEnabled = true;
    public bool resetButtonVisible = false;
    public bool resetButtonEnabled = false;

    [Header("Information")]
    [TextArea(5,10)] public string Description = "Interaction state configuration: Display text, button settings, etc.\n" +
                                                  "The display text is created using these placeholders:\n" +
                                                  "{0}: Interaction Verb\n" +
                                                  "{1}: Number of cards (including up to option)\n" +
                                                  "{2}: Card type (including money option)\n";

    public string GetInteractionString(int nbCardsToSelectMax)
    {
        if (! displayText.IsNullOrWhitespace()) return displayText;

        string countText = isUpTo ? $"up to {nbCardsToSelectMax}" : nbCardsToSelectMax.ToString();
        return string.Format(displayTextFormat, interactionType.ToString(), countText, FormatCardTypeText());
    }

    private string FormatCardTypeText()
    {
        if (allowedCardType == CardType.All) return "";

        return allowedCardType.ToString() + (includeMoneyOption ? " or Money " : " ");
    }
} 

public enum InteractionType {
    Select,
    Buy,
    Combat
}