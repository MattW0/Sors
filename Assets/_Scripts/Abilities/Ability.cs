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
        else if (target == EffectTarget.None)
            return trigger.ToString() + " -> " + amount.ToString() + " " + effect.ToString(); 
        else if (amount == 0)
            return trigger.ToString() + " -> " + effect.ToString() + " to " + target.ToString();
        else
            return trigger.ToString() + " -> " + amount.ToString() + " " + effect.ToString() + " to " + target.ToString();
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
    BeginningTurn = 1,
    BeginningDraw = 2,
    BeginningInvent = 3,
    BeginningDevelop = 4,
    BeginningCombat = 5,
    BeginningRecruit = 6,
    BeginningDeploy = 7,
    BeginningPrevail = 8,
    BeginningCleanUp = 9,
    // Beginning_when_you_gain_the_initiative
    
    // When triggers
    WhenYouBuy = 19,
    WhenYouPlay = 20,
    WhenDies = 21,
    WhenAttacks = 22,
    WhenBlocks = 23,
    WhenGetsBlocked = 24,
    WhenTakesDamage = 25,
    WhenDealsDamage = 26,
    // When_deals_combat_damage = 27,
    // When_deals_damage_to_a_player = 28,
    // When_becomes_a_target = 29,

    // Whenever triggers (reflexive) ?

}

public enum Effect
{
    None = 0,
    CardDraw = 1,
    Damage = 5,
    LifeGain = 6,
    // Destroy = 7,
    MoneyGain = 10,
    PriceReduction = 11,
}

public enum EffectTarget
{
    None = 0,
    Any = 1,
    You = 2,
    Opponent = 3,
    AnyPlayer = 4,
    Self = 7,
    Entity = 8,
    Creature = 9,
    Technology = 10,
    // Card = 20,
}
