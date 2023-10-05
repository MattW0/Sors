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
    [SerializeField] private Image _icon;
    [SerializeField] private Graphic outline;
    [SerializeField] private Image playerChoice;
    [SerializeField] private Image opponentChoice;


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
            outline.CrossFadeAlpha(0f, 1f, false);
            _mesh.SetActive(false);
        } else if (_phaseTitle == "Selection"){
            // nothing to do right now
        } else { // All selectable phases
            _phase = (Phase) System.Enum.Parse(typeof(Phase), gameObject.name);

            PhasePanel.OnPhaseSelectionStarted += StartSelection;
            PhasePanel.OnPhaseSelectionConfirmed += Reset;
            PhasePanelUI.OnPhaseSelectionConfirmed += ShowOpponentSelection;
        }

        outline.color = SorsColors.phaseHighlight;
    }

    public void StartSelection(){
        _selectable = true;
        IsSelected = false;
        opponentChoice.enabled = false;
        outline.CrossFadeAlpha(0.5f, 1f, false);
    }

    public void StartCombatPhase(){
        // _icon.color = SorsColors.phaseHighlight;
        _mesh.SetActive(true);
        _selectable = true;
    }

    public void Reset(){
        _selectable = false;
        outline.CrossFadeAlpha(0f, 1f, false);
        if(_mesh) _mesh.SetActive(false);
    }

    public void OnPointerClick(PointerEventData data){
        if (!_selectable) return;

        if(_phase != Phase.Combat) {
            _phasePanel.UpdateSelectedPhase(_phase);
            IsSelected = !_isSelected;
        } else {
            _phasePanel.PlayerPressedCombatButton();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_selectable) return;

        playerChoice.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_selectable || _phaseTitle != "") return;

        if(!_isSelected) playerChoice.enabled = false;
    }

    private void ShowOpponentSelection(Phase phase){
        if(phase == _phase) opponentChoice.enabled = true;
    }

    private void OnDestroy(){
        PhasePanel.OnPhaseSelectionStarted -= StartSelection;
        PhasePanel.OnPhaseSelectionConfirmed -= Reset;
        PhasePanelUI.OnPhaseSelectionConfirmed -= ShowOpponentSelection;
    }
}