using UnityEngine;

[RequireComponent(typeof(DetailCardUI))]
public class DetailCard : MonoBehaviour
{
    private DetailCardUI _ui;
    private void Awake() => _ui = GetComponent<DetailCardUI>();
    public void SetCardUI(CardInfo card) => _ui.SetCardUI(card, card.cardSpritePath);
}