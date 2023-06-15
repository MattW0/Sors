using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CreatureEntityUI : MonoBehaviour
{
    private Transform _transform;
    private readonly Vector3 _untappedPosition = new Vector3(0f, -30f, -1f);
    private readonly Vector3 _tappedPosition = new Vector3(0f, 30f, -1f);
    private const float TappingDuration = 0.3f;
    
    [Header("Entity UI")]
    [SerializeField] private Image highlight;
    [SerializeField] private Image attackerImage;
    [SerializeField] private Color highlightColor;
    [SerializeField] private Color combatColor;
    private readonly Color _idleColor = new Color32( 0x50, 0x50, 0x50, 0xFF );

    private void Start(){
        _transform = gameObject.transform;
        _transform.position = _untappedPosition;
    }

    public void ShowAsAttacker(bool active) => attackerImage.color = active ? combatColor : _idleColor;

    public void TapCreature() {
        _transform.DOLocalMove(_tappedPosition, TappingDuration).OnComplete(
            () => Highlight(false)
            );
    }

    public void UntapCreature(bool highlight) {
        _transform.DOLocalMove(_untappedPosition, TappingDuration).OnComplete(
            () => Highlight(highlight)
            );
        ShowAsAttacker(false);
    }

    public void TapOpponentCreature() => _transform.DOLocalMove(_untappedPosition, TappingDuration);
    public void UntapOpponentCreature() => _transform.DOLocalMove(_tappedPosition, TappingDuration);
    public void CombatHighlight(){
        highlight.color = combatColor;
        highlight.enabled = true;
    }
    public void Highlight(bool active) => highlight.enabled = active;
    public void ResetHighlight(){
        highlight.color = highlightColor;
        highlight.enabled = false;
    }
}
