using UnityEngine;
// using System.Drawing;

public static class SorsColors
{
    // -- Players --
    public static Color player = new Color(0f, 0.5f, 0.01f);
    public static Color opponent = new Color(0f, 0.08f, 0.54f);

    // -- UI --
    public static Color neutral_dark = new Color(0.05f, 0.05f, 0.05f);

    // -- Play Board --
    public static Color creature = new Color(0.34f, 0f, 0f);
    // public static Color technology = new Color(0f, 0f, 0.34f);
    public static Color technology = new Color(0.65f, 0.45f, 0.25f);
    // public static Color cash = new Color(0.1f, 0.5f, 0.35f);
    public static Color trash = new Color(0.3f, 0.1f, 0.3f);
    public static Color cash = new Color(0.9f, 0.75f, 0.1f);
    public static Color costValue = new Color(1f, 0.9f, 0.3f);
    public static Color attackValue = new Color(0.96f, 0.27f, 0.25f);
    public static Color healthValue = new Color(0.2f, 0.44f, 0.92f);
    public static Color pointsValue = new Color(0.37f, 0.77f, 0.29f);
    public static Color moneyValue = Color.grey;
    public static Color prevailColor = new Color(0.7f, 0.9f, 0.5f);

    // -- Entities --
    public static Color creatureHighlight = Color.green;
    public static Color creatureClashing = new Color( 150, 0, 0);
    public static readonly Color creatureIdle = new Color( 0x50, 0x50, 0x50 );

    // -- Kingdom Tiles --
    public static Color tileSelectable = Color.green;
    public static Color tileSelected = Color.blue;
    public static Color tilePreviouslySelected = Color.yellow;

    // -- Phases --
    public static Color phaseHighlight = new Color(1f, 1f, 0.7f, .6f);

    // -- Highlight --
    public static Color standardHighlight = Color.white;
    public static Color interactionHighlight = Color.green;
    public static Color discardHighlight = Color.yellow;
    public static Color deployHighlight = Color.cyan;
    public static Color trashHighlight = Color.red;

    // -- Log Messages --
    public static string standardLog = "#000000";
    public static string effectTrigger = "#1118BA";
    public static string turnChange = "#000142";
    public static string phase = "#383838";
    public static string creatureBuy = "#4f2d00";
    public static string combat = "#420028";
    public static string combatDamage = combat;
    public static string combatClash = combat;

    public static Color GetColor(string hexColor){
        ColorUtility.TryParseHtmlString(hexColor, out Color color);
        return color;
    }
}


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
}