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

        Highlight(true, true);
        _kingdom.CardToRecruitSelected(this);
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
