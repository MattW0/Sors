using UnityEngine;
using DG.Tweening;

public class CardPileInteraction : MonoBehaviour
{
    [SerializeField] private Transform _cardHolder;
    [SerializeField] private Transform _transformDefault;
    [SerializeField] private Transform _transformInteractable;
    private Vector3 _scaleInteractable = new Vector3(1.2f, 1.2f, 1f);

    public void StartInteraction()
    {
        // _cardHolder.DOLocalMove(_positionInteractable, SorsTimings.cardPileRearrangement);
        _cardHolder.DOMove(_transformInteractable.position, SorsTimings.cardPileRearrangement);
        _cardHolder.DOScale(_scaleInteractable, SorsTimings.cardPileRearrangement);
    }

    public void EndInteraction()
    {
        _cardHolder.DOMove(_transformDefault.position, SorsTimings.cardPileRearrangement);
        _cardHolder.DOScale(Vector3.one, SorsTimings.cardPileRearrangement);
    }
}
