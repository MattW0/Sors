using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityUI : MonoBehaviour
{
    [SerializeField] private Image _trigger;
    [SerializeField] private TMP_Text _amount;
    [SerializeField] private Image _effect;
    [SerializeField] private Image _target;
    [SerializeField] private TMP_Text _description;

    private const string BASE_PATH = "Sprites/UI/Icons/Ability/";
    public void SetPictos(Ability ability)
    {
        SetTrigger(ability.trigger);
        SetAmount(ability.amount);
        SetEffect(ability.effect);
        SetTarget(ability.target);

        _description.text = ability.ToString();
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
