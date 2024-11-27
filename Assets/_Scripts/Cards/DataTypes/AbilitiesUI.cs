using UnityEngine;
using TMPro;
using UnityUtils;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

[RequireComponent(typeof(LayoutGroup))]
public class AbilitiesUI : MonoBehaviour
{
    [SerializeField] private GameObject _abilityPrefab;
    private float _height;
    private float _maxHeight;
    private float _padding;

    private void Start()
    {
        _height = GetComponent<RectTransform>().rect.height;
        _padding = GetComponent<LayoutGroup>().padding.top;

        _maxHeight = _abilityPrefab.GetComponent<RectTransform>().rect.height - 2*_padding;
    }

    public void SetAbilities(List<Ability> abilities, bool withText)
    {
        transform.DestroyChildren();

        var height = Math.Min(_maxHeight, _height / abilities.Count);

        foreach (var ability in abilities)
        {
            var abilityItem = Instantiate(_abilityPrefab, transform);            
            abilityItem.GetComponent<AbilityUI>().Init(ability, height, withText);
        }
    }
}
