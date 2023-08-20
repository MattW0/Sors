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
            return "Ability: " + trigger.ToString() + " -> " + effect.ToString();
        else if (amount == 0)
            return "Ability: " + trigger.ToString() + " -> " + effect.ToString() + " to " + amount.ToString();
        else if (target == EffectTarget.None)
            return "Ability: " + trigger.ToString() + " -> " + effect.ToString() + ", " + amount.ToString();
        else
            return "Ability: " + trigger.ToString() + " -> " + effect.ToString() + " to " + amount.ToString() + ", " + target.ToString();
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        Ability other = (Ability)obj;
        return (this.trigger == other.trigger) && (this.effect == other.effect) && (this.amount == other.amount);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}

public enum Trigger
{
    None,
    // When triggers
    When_enters_the_battlefield,
    When_dies,
    When_attacks,
    When_blocks,
    When_gets_blocked,
    When_takes_damage,
    When_deals_damage,
    When_deals_combat_damage,
    When_deals_damage_to_a_player,
    When_becomes_a_target,

    // Whenever triggers (reflexive) ?

    // At the beginning of [PHASE]
    Beginning_Turn,
    Beginning_Draw,
    Beginning_Invent,
    Beginning_Develop,
    Beginning_Combat,
    Beginning_Recruit,
    Beginning_Deploy,
    Beginning_Prevail,
    // Beginning_when_you_gain_the_initiative
}

public enum Effect
{
    None,
    CardDraw,
    PriceReduction,
    MoneyGain,
    LifeGain,
    Damage,
    Removal
}

public enum EffectTarget{
    None,
    Player,
    Opponent,
    AnyPlayer,
    Any,
    Self,
    Entity,
    Creature,
    Technology,
    Card,
}
