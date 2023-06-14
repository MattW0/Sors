using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class KingdomTile : MonoBehaviour
{
    private Kingdom _kingdom;
    public CardInfo cardInfo;
    [SerializeField] private KingdomTileUI _ui;
    private int _cost;
    public int Cost{
        get => _cost;
        set{
            _cost = value;
            _ui.SetCost(value);
        }
    }
    
    private bool _isInteractable;
    public bool Interactable {
        get => _isInteractable;
        set {
            _isInteractable = value;
            _ui.Highlight(value, Color.green);
        }
    }
    private bool _isSelected;
    private bool IsSelected {
        get => _isSelected;
        set {
            _isSelected = value;
            if(value) {
                _kingdom.PlayerSelectsTile(cardInfo);
                _ui.Highlight(true, Color.blue);
            } else {
                _kingdom.PlayerDeselectsTile(cardInfo);
                _ui.Highlight(true, Color.green);
            }
        }
    }

    private bool _alreadyRecruited;

    private void Awake(){
        _kingdom = Kingdom.Instance;
    }

    public void SetTile(CardInfo card){
        cardInfo = card;
        Cost = card.cost;
        _ui.SetTileUI(card);

        if(card.type == CardType.Money || card.type == CardType.Development){
            Kingdom.OnDevelopPhaseEnded += EndDevelopPhase;
        } else if (card.type == CardType.Creature){
            Kingdom.OnRecruitPhaseEnded += EndRecruitPhase;
        }
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
        _ui.Highlight(true, Color.yellow);
        _alreadyRecruited = true;
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
