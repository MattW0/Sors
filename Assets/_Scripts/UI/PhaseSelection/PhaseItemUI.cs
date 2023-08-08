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
    [SerializeField] private Image playerChoice;

    private bool _selectable;
    private bool _isSelected;
    private bool IsSelected{
        get => _isSelected;
        set{
            _isSelected = value;
            playerChoice.enabled = value;
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

        outline.color = SorsColors.phaseHighlight;
        _mesh.SetActive(false);
    }

    public void StartSelection(){
        _selectable = true;
        IsSelected = false;

        outline.CrossFadeAlpha(0.5f, 1f, false);
    }

    public void StartCombatPhase(){
        _icon.color = SorsColors.phaseHighlight;
        _mesh.SetActive(true);
        _selectable = true;
    }

    public void Reset(){
        _selectable = false;
        // IsSelected = false;

        outline.CrossFadeAlpha(0f, 1f, false);
        _mesh.SetActive(false);
    }

    public void OnPointerClick(PointerEventData data){
        if (!_selectable) return;

        if(_phase != Phase.Combat) {
            _phasePanel.UpdateSelectedPhase(_phase);
            IsSelected = !_isSelected;
        } else {
            _phasePanel.PlayerPressedCombatButton();
            _mesh.GetComponent<Image>().color = SorsColors.phaseSelected;
        }
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