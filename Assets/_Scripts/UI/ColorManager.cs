using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorManager : MonoBehaviour
{
    [Header("Players")]
    public static Color playerOne = Color.blue;
    public static Color playerTwo = Color.yellow;


    [Header("Entities")]
    public static Color creatureHighlight = Color.green;
    public static Color creatureAttacking = new Color( 0xFF, 0x00, 0x50 );
    public static readonly Color creatureIdle = new Color( 0x50, 0x50, 0x50 );

    [Header("Kingdom Tiles")]
    public static Color tileSelectable = Color.green;
    public static Color tileSelected = Color.blue;
    public static Color tilePreviouslySelected = Color.yellow;

    [Header("Phases")]
    public static Color phaseHighlight = new Color(147, 147, 147);
    public static Color phaseSelected = new Color(150, 100, 200);

    [Header("Highlights")]
    public static Color standardHighlight = Color.white;
    public static Color interactionHighlight = Color.green;
    public static Color discardHighlight = Color.yellow;
    public static Color deployHighlight = Color.cyan;
    public static Color trashHighlight = Color.red;

    [Header("Log Messages")]
    public static string standardLog = "#000000";
    public static string effectTrigger = "#1118BA";
    public static string turnChange = "#000142";
    public static string phase = "#383838";
    public static string creatureBuy = "#4f2d00";
    public static string combat = "#420028";
    public static string combatDamage = combat;
    public static string combatClash = combat;

}
