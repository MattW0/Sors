using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class KingdomTile : MonoBehaviour
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
    
    private bool _isInteractable;
    public bool Interactable {
        get => _isInteractable;
        set {
            _isInteractable = value;
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
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text moneyValueText;
    [SerializeField] private TMP_Text defenseText;
    [SerializeField] private TMP_Text pointsText;
    [SerializeField] private TMP_Text attackText;
    [SerializeField] private TMP_Text description;
    [SerializeField] private Image highlight;
    // [SerializeField] private Image image;

    private void Awake(){
        _kingdom = Kingdom.Instance;
    }

    public void SetTile(CardInfo card){
        cardInfo = card;
        title.text = card.title;
        Cost = card.cost;

        if(card.type == CardType.Money){
            moneyValueText.text = card.moneyValue.ToString();
            Kingdom.OnDevelopPhaseEnded += EndDevelopPhase;
            return;
        }

        // Creature or Development
        defenseText.text = card.health.ToString();
        pointsText.text = card.points.ToString();

        if (card.type == CardType.Development){
            Kingdom.OnDevelopPhaseEnded += EndDevelopPhase;
            return;
        }

        // Creature
        attackText.text = card.attack.ToString();
        description.text = string.Join(" ", cardInfo.keywordAbilities.ConvertAll(f => f.ToString()));
        Kingdom.OnRecruitPhaseEnded += EndRecruitPhase;
    }

    public void SetBonus(int priceReduction){
        if(Cost - priceReduction <= 0) Cost = 0;
        else Cost -= priceReduction;
    }

    public void OnTileClick(){
        if (!_isInteractable) return;
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
    
    private void EndDevelopPhase() => ResetTile();

    private void EndRecruitPhase(){
        _alreadyRecruited = false;
        ResetTile();
    }

    private void ResetTile(){
        Interactable = true;
        _isSelected = false;
        // Reset cost (after bonus)
        Cost = cardInfo.cost;
    }

    private void OnDestroy(){
        Kingdom.OnDevelopPhaseEnded -= EndDevelopPhase;
        Kingdom.OnRecruitPhaseEnded -= EndRecruitPhase;
    }
}
