using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// using System.Drawing;

public static class SorsColors
{

    // load css from C:\Users\Matthias\Desktop\Sors\MirrorSors\Assets\Resources\colors.css
    // and get colors by calling GetColor() with a string from the css file



    // -- Players --
    public static Color playerOne = new Color(0f, 0.5f, 0.01f);
    public static Color playerTwo = new Color(0f, 0.08f, 0.54f);


    // -- Entities --
    public static Color creatureHighlight = Color.green;
    public static Color creatureAttacking = new Color( 0x00, 0x00, 0x00);
    public static readonly Color creatureIdle = new Color( 0x50, 0x50, 0x50 );

    // -- Kingdom Tiles --
    public static Color tileSelectable = Color.green;
    public static Color tileSelected = Color.blue;
    public static Color tilePreviouslySelected = Color.yellow;

    // -- Phases --
    public static Color phaseHighlight = new Color(1f, 1f, 0.7f, .6f);
    public static Color phaseSelected = new Color(150, 100, 0);

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
