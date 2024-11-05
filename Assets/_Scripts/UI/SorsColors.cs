using UnityEngine;
// using System.Drawing;

public static class SorsColors
{
    // -- Players --
    public static Color player = new(0.35f, 0f, 0.5f);
    public static string playerHex = "#540080";
    public static Color opponent = new(0f, 0.08f, 0.54f);
    public static string opponentHex = "#0014DB";

    // -- UI --
    public static Color neutralDark = new(0.1f, 0.1f, 0.12f);
    public static Color neutral = new(0.15f, 0.15f, 0.16f);
    public static Color neutralLight = new(0.25f, 0.25f, 0.28f);
    public static Color text = new(240, 243, 219);
    // public static Color primary = new Color(27, 68, 28);
    public static Color primary = new(0.16f, 0.27f, 0.12f);
    public static Color primaryLight = new(172, 203, 124);
    // public static Color secondary = new Color(0.2f, 0.2f, 0.2f);
    public static Color warn = new(230, 197, 36);
    public static Color error = new(216, 56, 20);

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
    public static Color effectTrigger = new(210, 230, 200);
    public static Color buy = new(200, 200, 90);
    public static Color play = new(130, 130, 200);
    public static Color combatClash = new(150, 0, 0);


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