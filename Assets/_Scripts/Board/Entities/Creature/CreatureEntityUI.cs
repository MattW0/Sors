using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CreatureEntityUI : MonoBehaviour
{
    [Header("Entity UI")]
    [SerializeField] private Image highlight;
    [SerializeField] private Image attackerImage;

    private void Awake() => attackerImage.color = SorsColors.creatureIdle;
    public void Highlight(bool b) => highlight.enabled = b;
    public void CreatureIdle()
    {
        // print("Creature is idle");
        // attackerImage.color = SorsColors.creatureIdle;
    }
    public void ShowAsAttacker()
    {
        // attackerImage.color = SorsColors.creatureAttacking;
    }
    public void ShowAsBlocker() {}  // => attackerImage.color = SorsColors.creatureBlocking;

    public void ResetHighlight(){
        attackerImage.color = SorsColors.creatureIdle;
        highlight.color = SorsColors.defaultHighlight;
        highlight.enabled = false;
    }
}
