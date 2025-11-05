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
    private float _prefabHeight;
    private float _maxHeight;
    private float _padding;

    private void Start()
    {
        _padding = GetComponent<LayoutGroup>().padding.top;
        _maxHeight = GetComponent<RectTransform>().rect.height - 2*_padding;
        _prefabHeight = _abilityPrefab.GetComponent<RectTransform>().rect.height;
    }

    public void SetAbilities(List<Ability> abilities)
    {
        transform.DestroyChildren();
        if(abilities.Count == 0) return;

        var height = Math.Min(_maxHeight, _prefabHeight / abilities.Count);
        foreach (var ability in abilities)
        {
            var abilityItem = Instantiate(_abilityPrefab, transform);            
            abilityItem.GetComponent<AbilityUI>().Init(ability, height);
        }
    }
}
