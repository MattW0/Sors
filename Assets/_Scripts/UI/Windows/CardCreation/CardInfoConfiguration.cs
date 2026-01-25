using System.Collections.Generic;

public struct Configuration
{
    public static List<CardType> CardTypeOptions = new() {
        CardType.Creature,
        CardType.Technology,
        CardType.Money
    };

    public static List<Trait> TraitOptions = new() {
        Trait.Trample,
        Trait.Deathtouch,
        Trait.Lifelink,
        Trait.Offensive,
        Trait.Defensive,
    };

    public static readonly Dictionary<CardType, HashSet<string>> VisibleFields = new() {
        {
            CardType.Creature, new() {
                CardFieldId.Type,
                CardFieldId.Title,
                CardFieldId.Cost,
                CardFieldId.Health,
                CardFieldId.Attack,
                CardFieldId.Traits,
                CardFieldId.Description,
                CardFieldId.FlavourText
            }
        },
        {
            CardType.Technology, new() {
                CardFieldId.Type,
                CardFieldId.Title,
                CardFieldId.Cost,
                CardFieldId.Health,
                CardFieldId.Points,
                CardFieldId.Description
                }
        },
        {
            CardType.Money, new(){
                CardFieldId.Type,
                CardFieldId.Title,
                CardFieldId.Cost,
                CardFieldId.MoneyValue,
                CardFieldId.FlavourText
            }
        }
    };
}
