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
    public CardInfo cardInfo;
    private int _cost;
    public int Cost{
        get => _cost;
        set{
            _cost = value;
            costText.text = value.ToString();
        }
    }
    
    private bool _isRecruitable;
    public bool Recruitable {
        get => _isRecruitable;
        set {
            _isRecruitable = value;
            Highlight(value, Color.green);
        }
    }
    private bool _isSelected;
    private bool IsSelected {
        get => _isSelected;
        set {
            _isSelected = value;
            if(value) {
                _kingdom.PlayerSelectsTile(cardInfo);
                Highlight(true, Color.blue);
            } else {
                _kingdom.PlayerDeselectsTile(cardInfo);
                Highlight(true, Color.green);
            }
        }
    }
    private bool _alreadyRecruited;

    // UI
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text description;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text attack;
    [SerializeField] private TMP_Text defense;
    [SerializeField] private TMP_Text points;
    [SerializeField] private Image highlight;

    private void Awake(){
        _kingdom = Kingdom.Instance;
        Kingdom.OnRecruitPhaseEnded += EndRecruitPhase;
    }

    public void SetTile(CardInfo card)
    {
        cardInfo = card;
        title.text = card.title;
        Cost = card.cost;
        
        attack.text = card.attack.ToString();
        defense.text = card.health.ToString();
        points.text = card.points.ToString();
        description.text = string.Join(" ", cardInfo.keyword_abilities.ConvertAll(f => f.ToString()));
    }

    public void SetRecruitBonus(int priceReduction){
        Cost -= priceReduction;
    }

    public void OnRecruitTileClick(){
        if (_alreadyRecruited || !_isRecruitable) return;
        IsSelected = !_isSelected;
    }

    public void ShowAsRecruited(){
        Highlight(true, Color.yellow);
        _alreadyRecruited = true;
    }

    public void Highlight(bool active, Color color = default(Color)){
        highlight.enabled = active;
        highlight.color = color;
    }
    
    private void EndRecruitPhase(){
        Recruitable = false;
        _isSelected = false;
        _alreadyRecruited = false;

        Cost = cardInfo.cost;
    }

    private void OnDestroy(){
        Kingdom.OnRecruitPhaseEnded -= EndRecruitPhase;
    }
}
