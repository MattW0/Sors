using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Ability
{
    public Trigger trigger;
    public Effect effect;
    public EffectTarget target;
    public int amount;

    public Ability(Trigger trigger, Effect effect, EffectTarget target, int amount)
    {
        this.trigger = trigger;
        this.effect = effect;
        this.target = target;
        this.amount = amount;
    }

    public Ability(Trigger trigger, Effect effect, EffectTarget target)
    {
        this.trigger = trigger;
        this.effect = effect;
        this.target = target;
        this.amount = 0;
    }

    public Ability(Trigger trigger, Effect effect, int amount)
    {
        this.trigger = trigger;
        this.effect = effect;
        this.target = EffectTarget.None;
        this.amount = amount;
    }

    public override string ToString()
    {
        if (amount == 0 && target == EffectTarget.None)
            return trigger.ToString() + " -> " + effect.ToString();
        else if (amount == 0)
            return trigger.ToString() + " -> " + effect.ToString() + " to " + amount.ToString();
        else if (target == EffectTarget.None)
            return trigger.ToString() + " -> " + effect.ToString() + ", " + amount.ToString();
        else
            return trigger.ToString() + " -> " + effect.ToString() + " to " + target.ToString() + ", " + amount.ToString();
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        Ability other = (Ability)obj;
        return (this.trigger == other.trigger) && (this.effect == other.effect) && (this.target == other.target) && (this.amount == other.amount);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}

public enum Trigger
{
    None = 0,
    // At the beginning of [PHASE]
    Beginning_Turn = 1,
    Beginning_Draw = 2,
    Beginning_Invent = 3,
    Beginning_Develop = 4,
    Beginning_Combat = 5,
    Beginning_Recruit = 6,
    Beginning_Deploy = 7,
    Beginning_Prevail = 8,
    Beginning_CleanUp = 9,
    // Beginning_when_you_gain_the_initiative
    
    // When triggers
    When_enters_the_battlefield = 20,
    When_dies = 21,
    When_attacks = 22,
    When_blocks = 23,
    When_gets_blocked = 24,
    When_takes_damage = 25,
    When_deals_damage = 26,
    When_deals_combat_damage = 27,
    When_deals_damage_to_a_player = 28,
    When_becomes_a_target = 29,

    // Whenever triggers (reflexive) ?

}

public enum Effect
{
    None = 0,
    CardDraw = 1,
    Damage = 5,
    LifeGain = 6,
    Destroy = 7,
    MoneyGain = 10,
    PriceReduction = 11,
}

public enum EffectTarget
{
    None = 0,
    Any = 1,
    Player = 2,
    Opponent = 3,
    AnyPlayer = 4,
    Self = 7,
    Entity = 8,
    Creature = 9,
    Technology = 10,
    Card = 20,
}
