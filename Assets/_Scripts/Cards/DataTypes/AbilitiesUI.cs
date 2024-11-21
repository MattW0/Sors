using UnityEngine;
using TMPro;
using UnityUtils;
using System.Collections.Generic;

public class AbilitiesUI : MonoBehaviour
{
    [SerializeField] private GameObject _abilityPrefab;
    [SerializeField] private TMP_Text _abilitiesText;
    
    public void SetAbilities(List<Ability> abilities)
    {
        transform.DestroyChildren();
        foreach (var ability in abilities)
        {
            var abilityItem = Instantiate(_abilityPrefab, transform);
            abilityItem.GetComponent<AbilityUI>().SetPictos(ability);
        }
    }
}
