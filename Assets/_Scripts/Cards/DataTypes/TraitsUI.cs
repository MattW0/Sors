using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityUtils;
using UnityEngine.UI;

public class TraitsUI : MonoBehaviour
{
    [SerializeField] private GameObject _traitPrefab;
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
            traitItem.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/UI/Icons/Traits/" + (int) trait);
        }
    }
}
