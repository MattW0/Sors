using UnityEngine;
// using System.Drawing;

public static class SorsColors
{
    // -- Players --
    public static Color player = new Color(0.35f, 0f, 0.5f);
    public static string playerHex = "#540080";
    public static Color opponent = new Color(0f, 0.08f, 0.54f);
    public static string opponentHex = "#0014DB";

    // -- UI --
    public static Color neutralDark = new Color(0.1f, 0.1f, 0.12f);
    public static Color neutral = new Color(0.15f, 0.15f, 0.16f);
    public static Color neutralLight = new Color(0.25f, 0.25f, 0.28f);
    public static Color text = new Color(240, 243, 219);
    // public static Color primary = new Color(27, 68, 28);
    public static Color primary = new Color(0.16f, 0.27f, 0.12f);
    public static Color primaryLight = new Color(172, 203, 124);
    // public static Color secondary = new Color(0.2f, 0.2f, 0.2f);
    public static Color warn = new Color(230, 197, 36);
    public static Color error = new Color(216, 56, 20);

    // -- Play Board --
    public static Color creature = new Color(0.34f, 0f, 0f);
    public static Color technology = new Color(0.65f, 0.45f, 0.25f);
    public static Color trash = new Color(0.3f, 0.3f, 0.3f);
    public static Color cash = new Color(0.77f, 0.95f, 1f);
    public static Color costValue = new Color(1f, 0.9f, 0.3f);
    public static Color attackValue = new Color(0.96f, 0.27f, 0.25f);
    public static Color healthValue = new Color(0.2f, 0.44f, 0.92f);
    public static Color pointsValue = new Color(0.37f, 0.77f, 0.29f);
    public static Color moneyValue = Color.grey;
    public static Color prevailColor = new Color(0.7f, 0.9f, 0.5f);

    // -- Entities --
    public static Color targetColor = Color.magenta;
    public static Color triggerHighlight = new Color(1f, 1f, 0.7f, .6f); // When ability triggers
    public static Color abilityHighlight = Color.white; // While choosing target(s) and during execution
    public static Color defaultHighlight = Color.green;
    // public static Color creatureAttacking = new Color(200, 80, 50);
    // public static Color creatureBlocking = new Color(0, 101, 138);
    public static Color combatClash = new Color(150, 0, 0);
    public static Color creatureIdle = new Color(200, 200, 200);

    // -- Market Tiles --
    public static Color tileSelectable = Color.green;
    public static Color tileSelected = Color.blue;

    // -- Phases --
    public static Color phaseHighlight = new Color(1f, 1f, 0.7f, .6f);

    // -- Highlight --
    public static Color standardHighlight = Color.white;
    public static Color interactionHighlight = Color.green;
    public static Color discardHighlight = Color.yellow;
    public static Color playableHighlight = Color.cyan;
    public static Color trashHighlight = Color.red;

    // -- Log Messages --
    public static string standardLog = "#999999";
    public static string detail = "#666666";
    public static string effectTrigger = "#FFFFFF";
    public static string turnChange = "#CCCCCC";
    public static string buy = "#DDDD80";
    public static string play = "#8080FF";
    public static string combat = "#FF8080";
    public static string combatAttacker = "#AA6060";
    public static string combatBlocker = "#6060AA";

    public static Color HexToColor(string hexColor){
        ColorUtility.TryParseHtmlString(hexColor, out Color color);
        return color;
    }

    public static string ColorToHex(Color color){
        return "#" + ColorUtility.ToHtmlStringRGB(color);
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