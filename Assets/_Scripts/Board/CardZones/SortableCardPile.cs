using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;

public class SortableCardPile : MonoBehaviour
{
    [SerializeField] private CardDragHandler _movingCard;
    [SerializeField] private GameObject slotPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private int cardsToSpawn = 7;
    public List<CardDragHandler> cards;

    bool isCrossing = false;
    [SerializeField] private bool tweenCardReturn = true;

    void Start()
    {
        for (int i = 0; i < cardsToSpawn; i++)
        {
            Instantiate(slotPrefab, transform);
        }

        cards = GetComponentsInChildren<CardDragHandler>().ToList();

        foreach (CardDragHandler card in cards)
        {
            card.BeginDragEvent.AddListener(BeginDrag);
            card.EndDragEvent.AddListener(EndDrag);
        }

        StartCoroutine(Frame());

        IEnumerator Frame()
        {
            yield return new WaitForSecondsRealtime(.1f);
            for (int i = 0; i < cards.Count; i++)
            {
                cards[i].UpdateIndex();
            }
        }
    }

    private void BeginDrag(CardDragHandler card) => _movingCard = card;
    void EndDrag(CardDragHandler card)
    {
        if (_movingCard == null) return;

        var endValue = _movingCard.selected ? new Vector3(0,_movingCard.selectionOffset,0) : Vector3.zero;
        var duration = tweenCardReturn ? .15f : 0;

        _movingCard.transform.DOLocalMove(endValue, duration).SetEase(Ease.OutBack);
        _movingCard = null;
    }

    void LateUpdate()
    {
        if (_movingCard == null || isCrossing) return;
        CheckSlotPosition();
    }

    private void CheckSlotPosition()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            if (_movingCard.transform.position.x > cards[i].transform.position.x)
            {
                if (_movingCard.ParentIndex() < cards[i].ParentIndex())
                {
                    Swap(i);
                    break;
                }
            }

            if (_movingCard.transform.position.x < cards[i].transform.position.x)
            {
                if (_movingCard.ParentIndex() > cards[i].ParentIndex())
                {
                    Swap(i);
                    break;
                }
            }
        }
    }

    private void Swap(int index)
    {
        isCrossing = true;

        Transform focusedParent = _movingCard.transform.parent;
        Transform crossedParent = cards[index].transform.parent;

        cards[index].transform.SetParent(focusedParent);
        cards[index].transform.localPosition = cards[index].selected ? new Vector3(0, cards[index].selectionOffset, 0) : Vector3.zero;
        _movingCard.transform.SetParent(crossedParent);

        isCrossing = false;

        int swapDirection = cards[index].ParentIndex() > _movingCard.ParentIndex() ? -1 : 1;
        cards[index].Swap(swapDirection);

        //Updated Visual Indexes
        foreach (CardDragHandler card in cards)
        {
            card.UpdateIndex();
        }
    }

}
