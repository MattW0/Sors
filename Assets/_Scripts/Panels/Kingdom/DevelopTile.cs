using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

[System.Serializable]
public class DevelopTile : MonoBehaviour
{
    private Kingdom _kingdom;
    private KingdomUI _ui;
    public CardInfo cardInfo;
    
    private bool _isDevelopable;
    public bool Developable {
        get => _isDevelopable;
        set {
            _isDevelopable = value;
            Highlight(value);
        }
    }
    private bool _isSelected;
    
    // UI
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text cost;
    public int Cost => int.Parse(cost.text); // for access in Kingdom
    // [SerializeField] private TMP_Text attack;
    // [SerializeField] private TMP_Text defense;
    // [SerializeField] private TMP_Text points;
    [SerializeField] private Image highlight;
    // [SerializeField] private Image image;


    private void Awake(){
        _kingdom = Kingdom.Instance;
        _ui = KingdomUI.Instance;
        Kingdom.OnDevelopPhaseEnded += EndDevelopPhase;
    }

    public void SetTile(CardInfo card)
    {
        cardInfo = card;
        title.text = card.title;
        cost.text = card.cost.ToString();
        // points.text = card.points.ToString();
    }

    public void SetDevelopBonus(int priceReduction){
        cost.text = (int.Parse(cost.text) - priceReduction).ToString();
    }

    public void UndoDevelopBonus(int priceReduction){
        cost.text = (int.Parse(cost.text) + priceReduction).ToString();
    }

    public void OnDevelopTileClick(){
        if (!_isDevelopable) return;

        // Selecting / deselectin
        if (!_isSelected) {
            _ui.SelectDevelopCard(this);
            highlight.color = Color.blue;
        } else {
            _ui.DeselectDevelopCard(this);
            highlight.color = Color.green;
        }
        _isSelected = !_isSelected;
    }

    private void Highlight(bool active)
    {
        highlight.enabled = active;
    }
    
    private void EndDevelopPhase(){
        _isDevelopable = false;
        _isSelected = false;
        highlight.enabled = false;
        highlight.color = Color.green;
    }

    private void OnDestroy(){
        Kingdom.OnDevelopPhaseEnded -= EndDevelopPhase;
    }
}
