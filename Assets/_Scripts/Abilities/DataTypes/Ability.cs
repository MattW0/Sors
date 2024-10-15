[System.Serializable]
public struct Ability
{
    public Trigger trigger;
    public Effect effect;
    public Target target;
    public int amount;

    public override string ToString()
    {
        if (amount == 0 && target == Target.None)
            return trigger.ToString() + ": " + effect.ToString();
        else if (target == Target.None)
            return trigger.ToString() + ": " + amount.ToString() + " " + effect.ToString(); 
        else if (amount == 0)
            return trigger.ToString() + ": " + effect.ToString() + " to " + target.ToString();
        else
            return trigger.ToString() + ": " + amount.ToString() + " " + effect.ToString() + " to " + target.ToString();
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