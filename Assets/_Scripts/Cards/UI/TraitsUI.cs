using System.Collections.Generic;
using UnityEngine;
using UnityUtils;
using UnityEngine.UI;
using System;

public class TraitsUI : MonoBehaviour
{
    [SerializeField] private GameObject _traitPrefab;
    [SerializeField] private float iconDimension = 40f;
    [SerializeField] private float iconPadding = 5f;

    private List<Trait> _traits;
    public List<Trait> Traits { 
        get => _traits;
        set {
            if(value == null || value.Count == 0) {
                gameObject.SetActive(false);
                return;
            }

            transform.DestroyChildren();
            gameObject.SetActive(true);

            foreach (var trait in value)
            {
                var traitItem = Instantiate(_traitPrefab, transform);
                traitItem.GetComponent<RectTransform>().sizeDelta = new Vector2(iconDimension, iconDimension);

                traitItem.GetComponent<Image>().sprite = trait.GetSprite();
            }

            _traits = value;
        }
    }

    public static event Action<List<Trait>> OnInspectTraits;
    public static event Action OnClearTraitsInspection;

    [ExecuteInEditMode]
    private void OnEnable()
    {
        if (_traitPrefab == null) return;

        var outerDim = iconDimension + 2*iconPadding;
        GetComponent<RectTransform>().sizeDelta = new Vector2(outerDim, outerDim);
        GetComponent<HorizontalLayoutGroup>().padding.top = (int) iconPadding;
        GetComponent<HorizontalLayoutGroup>().padding.bottom = (int) iconPadding;
    }

    public void InspectTraits()
    {
        if (!gameObject.activeSelf || Traits.Count == 0) return;

        OnInspectTraits?.Invoke(Traits);
    }

    public void ClearTraits() => OnClearTraitsInspection?.Invoke();
}
