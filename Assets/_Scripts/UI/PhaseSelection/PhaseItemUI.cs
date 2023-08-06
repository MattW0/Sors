using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class PhaseItemUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private PhasePanel _phasePanel;
    private Phase _phase;
    [SerializeField] private string _phaseTitle;
    [SerializeField] private GameObject _mesh;
    [SerializeField] private TMP_Text _phaseTitleUI;
    [SerializeField] private TMP_Text _phaseTitleMesh;
    [SerializeField] private Image _icon;
    [SerializeField] private Graphic outline;

    private bool _selectable;
    private bool _isSelected;
    private bool IsSelected{
        get => _isSelected;
        set{
            _isSelected = value;
            if(_isSelected) outline.color = SorsColors.phaseSelected;
            else outline.color = SorsColors.phaseHighlight;
        }
    }

    private void Start(){
        _phasePanel = PhasePanel.Instance;

        if(_phaseTitle != "") {
            _phase = Phase.Combat;
            _phaseTitleUI.text = _phaseTitle;
            _phaseTitleMesh.text = _phaseTitle;
        } else {
            _phase = (Phase) System.Enum.Parse(typeof(Phase), gameObject.name);
            _phaseTitleUI.text = _phase.ToString();
            _phaseTitleMesh.text = _phase.ToString();

            PhasePanel.OnPhaseSelectionStarted += StartSelection;
            PhasePanel.OnPhaseSelectionConfirmed += Reset;
        }

        _mesh.SetActive(false);
    }

    public void StartSelection(){
        outline.color = SorsColors.phaseHighlight;
        _selectable = true;
        outline.CrossFadeAlpha(1f, 0f, false);
    }

    public void StartCombatPhase(){
        _icon.color = SorsColors.phaseHighlight;
        _mesh.SetActive(true);
        _selectable = true;
    }

    public void Reset(){
        _selectable = false;
        IsSelected = false;

        outline.CrossFadeAlpha(0f, 1f, false);
        _mesh.SetActive(false);
    }

    public void OnPointerClick(PointerEventData data){
        if (!_selectable) return;
        IsSelected = !_isSelected;

        if(_phase == Phase.Combat) {
            _phasePanel.PlayerPressedCombatButton();
            _mesh.GetComponent<Image>().color = SorsColors.phaseSelected;
            return;
        }
        
        _phasePanel.UpdateSelectedPhase(_phase);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_selectable) return;
        _mesh.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_selectable || _phaseTitle != "") return;
        _mesh.SetActive(false);
    }

    private void OnDestroy(){
        PhasePanel.OnPhaseSelectionStarted -= StartSelection;
        PhasePanel.OnPhaseSelectionConfirmed -= Reset;
    }
}