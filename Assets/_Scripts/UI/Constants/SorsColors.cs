using UnityEngine;

[CreateAssetMenu(fileName = "New Sors Colors", menuName = "Sors/New Sors Colors")]
public class SorsColors : ScriptableObject
{
    public bool enableDynamicUpdate = true;

    // -- Players --
    public Color player = new(0.65f, 0.3f, 0.95f, 1f);
    public Color opponent = new(0.1f, 0.26f, 0.78f, 1f);

    // -- Cards Types --
    public Color creature = new(0.34f, 0f, 0f, 1f);
    public Color technology = new(0.65f, 0.45f, 0.25f, 1f);
    public Color money = new(.35f, .35f, .35f, 1f);

    // -- Cards & Entities --
    public Color costValue = new(1f, 0.9f, 0.3f, 1f);
    public Color attackValue = new(0.96f, 0.27f, 0.25f, 1f);
    public Color healthValue = new(0.2f, 0.44f, 0.92f, 1f);
    public Color pointsValue = new(0.37f, 0.77f, 0.29f, 1f);

    // -- Highlights --
    public Color defaultHighlight = new(.95f, .95f, 0f, .8f);
    public Color interactionPositiveHighlight = new (.2f, .9f, .2f, .8f);
    public Color interactionNegativeHighlight = new(1f, .2f, .2f, .8f);
    public Color targetHighlight = new(1f, .45f, .05f, .8f);
    public Color triggerHighlight = new(1f, 1f, .7f, .6f); // When ability triggers
    public Color abilityHighlight = new(1f, 1f, 1f, .8f); // While choosing target(s) and during execution
    public Color selectedHighlight = new(.5f, .5f, .8f);


    // -- UI Elements --
    public Color callToAction = new(.95f, .95f, .6f, 1f);
    public Color neutralDark = new(0.1f, 0.1f, 0.12f, 1f);
    public Color neutral = new(0.15f, 0.15f, 0.16f, 1f);
    public Color neutralLight = new(0.25f, 0.25f, 0.28f, 1f);

    // -- Utility --
    public static readonly Color transparent = new(0, 0, 0, 0);

    // -- Log Messages --
    public static string text = "#F0F0DC";
    public static string textLight = "#C0C0CC";
    public static string warn = "#F06688";
    public static string abilityExecution = "#8282C8";
    public static string buy = "#C7C759";
    public static string play = "#C19759";
    public static string combatClash = "#C87272";

    public static string AddColorByType(string msg, LogType type)
    {
        var color = type switch{
            LogType.AbilityExecution => SorsColors.abilityExecution,
            LogType.TurnChange => SorsColors.text,
            LogType.Phase => SorsColors.textLight,
            LogType.Buy => SorsColors.buy,
            LogType.Play => SorsColors.play,
            LogType.Combat => SorsColors.warn,
            LogType.CombatClash => SorsColors.combatClash,
            LogType.Standard => SorsColors.textLight,
            _ => SorsColors.text
        };

        return msg.AddColorFromHex(color);
    }
}

// Never change order of these
public enum ColorType : byte
{
    Player = 0,
    Opponent = 1,
    Creature = 5,
    Technology = 6,
    Money = 7,
    Cost = 10,
    Attack = 11,
    Health = 12,
    Points = 13,
    MoneyValue = 14,
    Neutral = 20,
    NeutralDark = 21,
    NeutralLight = 22,
    CallToAction = 30,
}

public enum HighlightType : byte
{
    None = 0,
    Default = 1,
    InteractionPositive = 2,
    InteractionNegative = 3,
    Trigger = 4,
    Ability = 5,
    Target = 6,
    Selected = 7,
    Playable = 8,
}