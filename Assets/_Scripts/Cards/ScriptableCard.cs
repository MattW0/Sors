using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "New Card", menuName = "Card")]
public class ScriptableCard : ScriptableObject{

    [Header("Image")]
    public Sprite image; // Card image

    [Header("Properties")]
    public string hash;
    public bool isCreature;
    public int cost;
    public int attack;
    public int health;
    public string title;
    public string description;
}
