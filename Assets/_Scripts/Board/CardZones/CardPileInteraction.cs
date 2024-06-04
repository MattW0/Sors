using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class CardPileInteraction : MonoBehaviour
{
    [SerializeField] private Transform _cardHolder;
    [SerializeField] private Image _background;
    [SerializeField] private Vector3 _positionInteractable = new Vector3(-200, 100, 0);
    [SerializeField] private Vector3 _scaleInteractable = new Vector3(1.2f, 1.2f, 0);

    public void StartInteraction()
    {
        _cardHolder.DOLocalMove(_positionInteractable, SorsTimings.cardPileRearrangement);
        _cardHolder.DOScale(_scaleInteractable, SorsTimings.cardPileRearrangement);
    }

    public void EndInteraction()
    {
        _cardHolder.DOLocalMove(Vector3.zero, SorsTimings.cardPileRearrangement);
        _cardHolder.DOScale(Vector3.one, SorsTimings.cardPileRearrangement);
    }
}
