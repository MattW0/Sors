using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class KingdomCard : MonoBehaviour
{
    private Kingdom _kingdom;
    public bool isRecruitable;
    public CardInfo cardInfo;

    // UI
    [SerializeField] private TMP_Text _title;
    [SerializeField] private TMP_Text _cost;
    [SerializeField] private TMP_Text _attack;
    [SerializeField] private TMP_Text _defense;
    [SerializeField] private Image _highlight;

    private void Awake(){
        _kingdom = Kingdom.Instance;
        Kingdom.OnRecruitPhaseStarted += StartRecruitPhase;
        PlayerManager.OnCashChanged += UpdateRecruitability;
    }

    public void SetCard(CardInfo _card)
    {   
        cardInfo = _card;
        _title.text = _card.title;
        _cost.text = _card.cost.ToString();
        _attack.text = _card.attack.ToString();
        _defense.text = _card.health.ToString();
    }

    private void StartRecruitPhase(){
        isRecruitable = false;
    }

    private void UpdateRecruitability(int currentCash){
        if (currentCash >= int.Parse(_cost.text)){
            isRecruitable = true;
            Highlight(true);
        }
    }

    public void OnKingdomCardClick(){
        if (!isRecruitable) return;

        print("Recruiting " + _title.text);
        Highlight(true, true);
        _kingdom.CardToRecruitSelected(this);
    }

    private void Highlight(bool active, bool chosen=false){
        _highlight.enabled = active;

        if (!chosen) return;
        _highlight.color = Color.blue;
    }

    private void OnDestroy(){
        Kingdom.OnRecruitPhaseStarted -= StartRecruitPhase;
    }
}
