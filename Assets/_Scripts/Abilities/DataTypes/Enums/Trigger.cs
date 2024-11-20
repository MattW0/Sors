public enum Trigger
{
    None = 0,
    // At the beginning of [PHASE]
    PhaseSelection = 1,
    Draw = 2,
    Invent = 3,
    Develop = 4,
    Attackers = 5,
    Blockers = 6,
    Recruit = 7,
    Deploy = 8,
    Prevail = 9,
    CleanUp = 10,
    // Beginning_when_you_gain_the_initiative
    
    // When triggers
    WhenYouBuy = 20,
    WhenYouPlay = 21,
    WhenDies = 22,
    WhenAttacks = 23,
    WhenBlocks = 24,
    WhenGetsBlocked = 25,
    // When_becomes_a_target,

    // Whenever triggers
    WheneverTakesDamage = 26,
    WheneverDealsDamage = 27,
    // When_deals_combat_damage,
    // When_deals_damage_to_a_player,

    // Whenever triggers (reflexive) ?

}
