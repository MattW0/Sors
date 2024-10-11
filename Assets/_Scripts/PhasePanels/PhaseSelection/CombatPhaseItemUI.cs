using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class CombatPhaseItemUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject _mesh;
    [SerializeField] private GameObject _tooltip;
    private bool _selectable;
    public static event Action OnPressedCombatButton;
    public void IsSelectable()
    {
        _mesh.SetActive(true);
        _selectable = true;
    }

    public void OnPointerClick(PointerEventData data)
    {
        if (!_selectable) return;
        
        OnPressedCombatButton?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData) => _tooltip.SetActive(true);
    public void OnPointerExit(PointerEventData eventData) => _tooltip.SetActive(false);

    public void Reset()
    {
        _selectable = false;
        _mesh.SetActive(false);
        _tooltip.SetActive(false);
    }

}
