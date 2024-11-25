using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class CombatPhaseItemUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject _mesh;
    private TooltipWindow _tooltip;
    public bool IsSelectable { get; set; }
    public static event Action OnPressedCombatButton;
    private void Start()
    {
        _tooltip = GetComponentInChildren<TooltipWindow>();
    }

    public void OnPointerClick(PointerEventData data)
    {
        if (!IsSelectable) return;
        
        OnPressedCombatButton?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData) => _tooltip.WindowIn();
    public void OnPointerExit(PointerEventData eventData) => _tooltip.WindowOut();

    public void Reset()
    {
        IsSelectable = false;
        _mesh.SetActive(false);
    }

}
