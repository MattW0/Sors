using UnityEngine;
using System.Collections.Generic;

public class CardHoverView : ModalWindow
{
    [Header("Prefabs")]
    [SerializeField] private DetailCardPreview _cardPreview;
    [SerializeField] private Transform _traitDetailContainer;
    [SerializeField] private GameObject _traitDetailPrefab;

    private bool _isVisible;

    private void Start()
    {
        _cardPreview.HideAll(true);
    }

    public void Show(CardInfo card)
    {
        UpdateContent(card);

        if (_isVisible)
            return;

        _isVisible = true;
        WindowIn();
    }

    public void UpdateContent(CardInfo card)
    {
        _cardPreview.ShowPreview(card, card.type != CardType.Money);
        StartTraitInspection(card.traits);
    }

    private void StartTraitInspection(List<Trait> traits)
    {
        foreach (Transform child in _traitDetailContainer) Destroy(child.gameObject);
        
        foreach (var trait in traits)
        {
            var traitDetail = Instantiate(_traitDetailPrefab, _traitDetailContainer);
            traitDetail.GetComponent<TraitDetailUI>().Initialize(trait);
        }
    }

    public void Hide()
    {
        _isVisible = false;
        WindowOut();
    }
}
