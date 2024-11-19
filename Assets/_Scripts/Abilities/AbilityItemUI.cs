using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using Unity.VisualScripting;

public class AbilityItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _effectDescription;
    [SerializeField] private Image _image;
    [SerializeField] private Image _highlight;

    public void SetUI(CardInfo cardInfo, Ability ability)
    {
        _titleText.text = cardInfo.title;
        // _cost.text = cardInfo.cost.ToString();
        _image.sprite = Resources.Load<Sprite>(cardInfo.cardSpritePath);

        // Entity properties
        // _health.text = cardInfo.health.ToString();
        _effectDescription.text = ability.ToString();
    }

    internal void SetActive()
    {
        _highlight.enabled = true;
    }
}
