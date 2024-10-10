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
    public void CreatureIdle(){
        // print("Creature is idle");
        attackerImage.color = SorsColors.creatureIdle;
    }
    public void ShowAsAttacker(){
        attackerImage.color = SorsColors.creatureAttacking;
    }
    public void ShowAsBlocker() => attackerImage.color = SorsColors.creatureBlocking;

    public void ResetHighlight(){
        attackerImage.color = SorsColors.creatureIdle;
        highlight.color = SorsColors.creatureHighlight;
        highlight.enabled = false;
    }

    // #region Tapping / Moving on board
    // private Transform _transform;
    // private readonly Vector3 _untappedPosition = new Vector3(0f, -30f, -1f);
    // private readonly Vector3 _tappedPosition = new Vector3(0f, 30f, -1f);
    // private const float TappingDuration = 0.3f;
    // private void Start(){
    //     _transform = gameObject.transform;
    //     // _transform.position = _untappedPosition;
    // }
    // public void TapCreature() {
    //     //  TODO: Could replace {} with action to do after tapping
    //     _transform.DOLocalMove(_tappedPosition, TappingDuration).OnComplete( () => {} );
    // }

    // public void UntapCreature() {
    //     _transform.DOLocalMove(_untappedPosition, TappingDuration).OnComplete( () => {} );
    //     ResetHighlight();
    // }

    // public void TapOpponentCreature() => _transform.DOLocalMove(_untappedPosition, TappingDuration);
    // public void UntapOpponentCreature(){
    //     _transform.DOLocalMove(_tappedPosition, TappingDuration);
    //     ResetHighlight();
    // }
    // #endregion
}
