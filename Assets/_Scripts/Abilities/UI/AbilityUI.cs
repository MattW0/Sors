using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _description;
    private RectTransform _rectTransform;

    [Header("Pictos")]
    [SerializeField] private Image _trigger;
    [SerializeField] private TMP_Text _amount;
    [SerializeField] private Image _effect;
    [SerializeField] private Image _target;
    [SerializeField] private RectTransform _pictosParent;
    private const string BASE_PATH = "Sprites/UI/Icons/Ability/";

    public void Init(Ability ability, float height, bool withText)
    {
        SetPictos(ability);

        _rectTransform = GetComponent<RectTransform>();
        if (height == _rectTransform.rect.height) return;

        // Resize the ability UI, keeping the same aspect ratio
        var ratio = height / _rectTransform.rect.height;
        _rectTransform.sizeDelta = new Vector2(_rectTransform.sizeDelta.x * ratio, height);

        if (!withText) return;
        
        _description.text = String.IsNullOrEmpty(ability.text) ? ability.ToString() : ability.text;
        _pictosParent.sizeDelta = new Vector2(_pictosParent.sizeDelta.x * ratio, _pictosParent.sizeDelta.y);
    }
    
    private void SetPictos(Ability ability)
    {
        SetTrigger(ability.trigger);
        SetAmount(ability.amount);
        SetEffect(ability.effect);
        SetTarget(ability.target);
    }

    private void SetTrigger(Trigger trigger)
    {
        var path = BASE_PATH + "Trigger/";
        path += trigger.ToString().StartsWith("When") ?  "When/" : "AtTheBeginningOf/";
        path += (int) trigger;
       
        // print("Load trigger: " + trigger + " from path: " + path);
        _trigger.sprite = Resources.Load<Sprite>(path);
    }

    private void SetAmount(int amount)
    {
        if (amount == 0)
            _amount.text = "";
        else
            _amount.text = $": {amount}";
    }

    private void SetEffect(Effect effect)
    {
        _effect.sprite = Resources.Load<Sprite>(BASE_PATH + "Effect/" + (int) effect);
    }

    private void SetTarget(Target target)
    {
        _target.sprite = Resources.Load<Sprite>(BASE_PATH + "Target/" + (int) target);
    }
}
