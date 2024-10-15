public enum Trigger
{
    None = 0,
    // At the beginning of [PHASE]
    PhaseSelection = 1,
    Draw = 2,
    Invent = 3,
    Develop = 4,
    Combat = 5,
    Recruit = 6,
    Deploy = 7,
    Prevail = 8,
    CleanUp = 9,
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
