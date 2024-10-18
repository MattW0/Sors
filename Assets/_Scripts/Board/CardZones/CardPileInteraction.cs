using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CardsPileSors))]
public class CardPileInteraction : MonoBehaviour
{
    [SerializeField] private Transform _cardHolder;
    [SerializeField] private Transform _transformDefault;
    [SerializeField] private Transform _transformInteractable;
    [SerializeField] private Vector3 _scaleDefault = Vector3.one;
    [SerializeField] private Vector3 _scaleInteractable = new Vector3(1.2f, 1.2f, 1f);
    private CardsPileSors _pile;

    private void Start()
    {
        _pile = GetComponent<CardsPileSors>();
    }

    public void StartInteraction()
    {
        _cardHolder.DOMove(_transformInteractable.position, SorsTimings.cardPileRearrangement);
        _cardHolder.DOScale(_scaleInteractable, SorsTimings.cardPileRearrangement);
        _pile.StartInteraction();
    }

    public void EndInteraction()
    {
        _cardHolder.DOMove(_transformDefault.position, SorsTimings.cardPileRearrangement);
        _cardHolder.DOScale(_scaleDefault, SorsTimings.cardPileRearrangement);
        _pile.EndInteraction();
    }
}
