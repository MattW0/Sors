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
    public void CanAct(bool isTrue) => highlight.enabled = isTrue;
    public void CreatureIdle() => attackerImage.color = SorsColors.creatureIdle;
    public void ShowAsAttacker(){
        // TODO: Why this no work ???
        // print($"show as attacker, color : {SorsColors.creatureAttacking}");
        attackerImage.color = SorsColors.creatureAttacking;
    }
    public void ShowAsBlocker() => attackerImage.color = SorsColors.creatureBlocking;
    public void CombatHighlight() => attackerImage.color = SorsColors.creatureClashing;
    public void ResetHighlight(){
        attackerImage.color = SorsColors.creatureIdle;
        highlight.color = SorsColors.creatureHighlight;
        highlight.enabled = false;
    }

    #region Tapping / Moving on board
    private Transform _transform;
    private readonly Vector3 _untappedPosition = new Vector3(0f, -30f, -1f);
    private readonly Vector3 _tappedPosition = new Vector3(0f, 30f, -1f);
    private const float TappingDuration = 0.3f;
    private void Start(){
        _transform = gameObject.transform;
        // _transform.position = _untappedPosition;
    }
    public void TapCreature() {
        //  TODO: Could replace {} with action to do after tapping
        _transform.DOLocalMove(_tappedPosition, TappingDuration).OnComplete( () => {} );
    }

    public void UntapCreature(bool highlight) {
        _transform.DOLocalMove(_untappedPosition, TappingDuration).OnComplete( () => {} );
        ResetHighlight();
    }

    public void TapOpponentCreature() => _transform.DOLocalMove(_untappedPosition, TappingDuration);
    public void UntapOpponentCreature(){
        _transform.DOLocalMove(_tappedPosition, TappingDuration);
        ResetHighlight();
    }
    #endregion
}
