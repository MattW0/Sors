using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityUtils;
using UnityEngine.UI;

public class TraitsUI : MonoBehaviour
{
    [SerializeField] private GameObject _traitPrefab;
    [SerializeField] private float iconDimension = 40f;
    [SerializeField] private float iconPadding = 5f;

    [ExecuteInEditMode]
    private void OnEnable()
    {
        if (_traitPrefab == null) return;

        var outerDim = iconDimension + 2*iconPadding;
        GetComponent<RectTransform>().sizeDelta = new Vector2(outerDim, outerDim);
        GetComponent<HorizontalLayoutGroup>().padding.top = (int) iconPadding;
        GetComponent<HorizontalLayoutGroup>().padding.bottom = (int) iconPadding;
    }

    public void SetTraits(List<Traits> traits)
    {
        if(traits == null) return;
        if(traits.Count == 0) {
            gameObject.SetActive(false);
            return;
        }

        transform.DestroyChildren();
        gameObject.SetActive(true);

        foreach (var trait in traits)
        {
            var traitItem = Instantiate(_traitPrefab, transform);
            traitItem.GetComponent<RectTransform>().sizeDelta = new Vector2(iconDimension, iconDimension);

            traitItem.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/UI/Icons/Traits/" + (int) trait);
        }
    }
}
