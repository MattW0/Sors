using UnityEngine;

public static class SorsColors
{
    // -- Players --
    public static Color player = new(0.65f, 0.3f, 0.95f);
    public static Color opponent = new(0.1f, 0.26f, 0.78f);

    // -- UI --
    public static Color neutralDark = new(0.1f, 0.1f, 0.12f);
    public static Color neutral = new(0.15f, 0.15f, 0.16f);
    public static Color neutralLight = new(0.25f, 0.25f, 0.28f);
    public static Color primary = new(0.16f, 0.27f, 0.12f);
    public static Color primaryLight = new(172, 203, 124);
    // public static Color secondary = new Color(0.2f, 0.2f, 0.2f);


    // -- Play Board --
    public static Color creature = new(0.34f, 0f, 0f);
    public static Color technology = new(0.65f, 0.45f, 0.25f);
    public static Color trash = new(0.3f, 0.3f, 0.3f);
    public static Color cash = new(0.77f, 0.95f, 1f);
    public static Color costValue = new(1f, 0.9f, 0.3f);
    public static Color attackValue = new(0.96f, 0.27f, 0.25f);
    public static Color healthValue = new(0.2f, 0.44f, 0.92f);
    public static Color pointsValue = new(0.37f, 0.77f, 0.29f);
    public static Color prevailColor = new(0.7f, 0.9f, 0.5f);

    // -- Entities --
    public static Color targetColor = Color.magenta;
    public static Color triggerHighlight = new(1f, 1f, 0.7f, .6f); // When ability triggers
    public static Color abilityHighlight = Color.white; // While choosing target(s) and during execution
    public static Color defaultHighlight = Color.green;
    public static Color creatureIdle = new(200, 200, 200);

    // -- Market Tiles --
    public static Color tileSelectable = Color.green;
    public static Color tileSelected = Color.blue;

    // -- Phases --
    public static Color phaseHighlight = new(1f, 1f, 0.7f, .6f);

    // -- Highlight --
    public static Color standardHighlight = Color.white;
    public static Color interactionHighlight = Color.green;
    public static Color discardHighlight = Color.yellow;
    public static Color playableHighlight = Color.cyan;
    public static Color trashHighlight = Color.red;

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
    Creature,
    Technology,
    Cash,
    Cost,
    Attack,
    Health,
    Points,
    MoneyValue,
    Neutral,
    Trash,
    PrevailOption,
    Player,
    Opponent,
    NeutralDark,
    NeutralLight,
    Highlight,
}