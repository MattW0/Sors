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
    public CardInfo cardInfo;
    private int _cost;
    public int Cost{
        get => _cost;
        set{
            _cost = value;
            costText.text = value.ToString();
        }
    }
    
    private bool _isDevelopable;
    public bool Developable {
        get => _isDevelopable;
        set {
            _isDevelopable = value;
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
    
    // UI
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private GameObject moneyValue;
    [SerializeField] private TMP_Text moneyValueText;
    [SerializeField] private GameObject defense;
    [SerializeField] private TMP_Text defenseText;
    [SerializeField] private Image highlight;
    // [SerializeField] private Image image;


    private void Awake(){
        _kingdom = Kingdom.Instance;
        Kingdom.OnDevelopPhaseEnded += EndDevelopPhase;
    }

    public void SetTile(CardInfo card){
        cardInfo = card;
        title.text = card.title;
        Cost = card.cost;

        if(card.type == CardType.Money){
            moneyValueText.text = card.moneyValue.ToString();
            defense.SetActive(false);
        } else if (card.type == CardType.Development){
            defenseText.text = card.health.ToString();
            moneyValue.SetActive(false);
        }
        // points.text = card.points.ToString();
    }

    public void SetDevelopBonus(int priceReduction){
        Cost -= priceReduction;
    }

    public void OnDevelopTileClick(){
        if (!_isDevelopable) return;
        IsSelected = !_isSelected;
    }

    public void Highlight(bool active, Color color = default(Color)){
        highlight.enabled = active;
        highlight.color = color;
    }
    
    private void EndDevelopPhase(){
        Developable = false;
        _isSelected = false;

        // Reset cost (after bonus)
        Cost = cardInfo.cost;
    }

    private void OnDestroy(){
        Kingdom.OnDevelopPhaseEnded -= EndDevelopPhase;
    }
}
