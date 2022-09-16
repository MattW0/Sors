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
    public CardInfo cardInfo;
    
    public bool isRecruitable;
    private bool isSelected;
    
    // UI
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text cost;
    [SerializeField] private TMP_Text attack;
    [SerializeField] private TMP_Text defense;
    [SerializeField] private Image highlight;

    private void Awake(){
        _kingdom = Kingdom.Instance;
        Kingdom.OnRecruitPhaseEnded += EndRecruitPhase;
        PlayerManager.OnCashChanged += UpdateRecruitability;
    }

    public void SetCard(CardInfo card)
    {   
        cardInfo = card;
        title.text = card.title;
        cost.text = card.cost.ToString();
        attack.text = card.attack.ToString();
        defense.text = card.health.ToString();
    }

    private void EndRecruitPhase(){
        isRecruitable = false;
    }

    private void UpdateRecruitability(int currentCash){
        if (currentCash >= int.Parse(cost.text)){
            isRecruitable = true;
            Highlight(true);
        } else {
            isRecruitable = false;
            Highlight(false);
        }
    }

    public void OnKingdomCardClick(){
        if (!isRecruitable) return;
        
        // Deselecting
        if (isSelected) {
            _kingdom.CardToRecruitClicked(false, this);
            Highlight(true, false);
            isSelected = false;
            return;
        }

        isSelected = true;
        Highlight(true, true);
        _kingdom.CardToRecruitClicked(true, this);
    }

    private void Highlight(bool active, bool chosen=false)
    {
        highlight.enabled = active;
        highlight.color = !chosen ? Color.green : Color.blue;
    }

    private void OnDestroy(){
        Kingdom.OnRecruitPhaseEnded -= EndRecruitPhase;
    }
}
