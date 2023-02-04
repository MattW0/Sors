using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

[System.Serializable]
public class KingdomCard : MonoBehaviour
{
    private Kingdom _kingdom;
    private KingdomUI _ui;
    public CardInfo cardInfo;
    
    private bool _isRecruitable;
    public bool Recruitable {
        get => _isRecruitable;
        set {
            _isRecruitable = value;
            Highlight(value);
        }
    }
    private bool _isSelected;
    
    // UI
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text cost;
    public int Cost => int.Parse(cost.text); // for access in Kingdom
    [SerializeField] private TMP_Text attack;
    [SerializeField] private TMP_Text defense;
    [SerializeField] private TMP_Text points;
    [SerializeField] private Image highlight;

    private void Awake(){
        _kingdom = Kingdom.Instance;
        _ui = KingdomUI.Instance;
        Kingdom.OnRecruitPhaseEnded += EndRecruitPhase;
    }

    public void SetCard(CardInfo card)
    {
        cardInfo = card;
        title.text = card.title;
        cost.text = card.cost.ToString();
        attack.text = card.attack.ToString();
        defense.text = card.health.ToString();
        points.text = card.points.ToString();
    }

    public void OnKingdomCardClick(){
        if (!_isRecruitable) return;

        // Selecting / deselectin
        if (!_isSelected) {
            _ui.SelectCard(this);
            highlight.color = Color.blue;
        } else {
            _ui.DeselectCard(this);
            highlight.color = Color.green;
        }
        _isSelected = !_isSelected;
    }

    private void Highlight(bool active)
    {
        highlight.enabled = active;
    }
    
    private void EndRecruitPhase(){
        _isRecruitable = false;
        _isSelected = false;
        highlight.enabled = false;
        highlight.color = Color.green;
    }

    private void OnDestroy(){
        Kingdom.OnRecruitPhaseEnded -= EndRecruitPhase;
    }
}
