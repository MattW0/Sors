using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

[System.Serializable]
public class RecruitTile : MonoBehaviour
{
    private Kingdom _kingdom;
    private KingdomUI _ui;
    public CardInfo cardInfo;
    private int _cost;
    public int Cost => int.Parse(cost.text);
    
    private bool _isRecruitable;
    public bool Recruitable {
        get => _isRecruitable;
        set {
            _isRecruitable = value;
            Highlight(value);
        }
    }
    private bool _isSelected;
    private bool IsSelected {
        get => _isSelected;
        set {
            _isSelected = value;
            if(value) {
                _ui.SelectRecruitTile(this);
                highlight.color = Color.blue;
            } else {
                _ui.DeselectRecruitTile(this);
                highlight.color = Color.green;
            }
        }
    }
    private bool _alreadyRecruited;

    // UI
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text description;
    [SerializeField] private TMP_Text cost;
    [SerializeField] private TMP_Text attack;
    [SerializeField] private TMP_Text defense;
    [SerializeField] private TMP_Text points;
    [SerializeField] private Image highlight;

    private void Awake(){
        _kingdom = Kingdom.Instance;
        _ui = KingdomUI.Instance;
        Kingdom.OnRecruitPhaseEnded += EndRecruitPhase;
    }

    public void SetTile(CardInfo card)
    {
        cardInfo = card;
        title.text = card.title;
        _cost = card.cost;
        attack.text = card.attack.ToString();
        defense.text = card.health.ToString();
        points.text = card.points.ToString();
        description.text = string.Join(" ", cardInfo.keyword_abilities.ConvertAll(f => f.ToString()));
    }

    public void SetRecruitBonus(int priceReduction){
        cost.text = (_cost - priceReduction).ToString();
    }

    public void OnRecruitTileClick(){
        if (_alreadyRecruited || !_isRecruitable) return;
        IsSelected = !_isSelected;
    }

    public void ShowAsRecruited()
    {
        highlight.color = Color.yellow;
        _alreadyRecruited = true;
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
        _alreadyRecruited = false;

        _cost = cardInfo.cost;
        cost.text = _cost.ToString();
    }

    private void OnDestroy(){
        Kingdom.OnRecruitPhaseEnded -= EndRecruitPhase;
    }
}
