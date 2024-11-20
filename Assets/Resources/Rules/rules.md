# Sors

>## Setting
>In the cyberpunk dystopia of Nox City you are a prominent mob boss in the constant struggle of underground shenanigans. The fierce competition with corrupt business politicals and rival gang leaders forces you to confront your opponents in 1-on-1 battles. An army of thugs lurks in the streets of Nox city, ready to do your vicious bidding. Loyalty is a fleeting currency, and you know that as long as the payments keep flowing, their allegiance remains. Maybe the market's next-gen technology allows you to gain the edge over your opponents? Your choices echo in the neon-lit streets, where money rules, trust is fragile, and the line between victory and defeat is razor-thin. The city's pulse beats with tension as you contemplate the next moves in this deadly game for control.

## Goal
The first who reduces thier opponent's life total to zero wins the game! You build a deck of cards during playtime to beat your opponent in creature based battle. Both players take actions simultaneously. To improve your deck and the chances to win, get rid of the bad initial cards and spend money to buy stronger ones from the market. Enhance your limited resources with technologies and play your creatures to attack your opponent.

---
## How To
### Initial deck and resources
Players start with a deck of 10 cards. During gameplay, you improve your deck by buying **Creature** cards and **Technology** cards using **Buys**. You can later play these cards using **Plays**. Buying and playing cards costs money, which you pay with **Cash** earned by playing **Money** cards. _Cash_, _Buys_, _Plays_ and **Prevails** are resources you gain during a turn. _Prevails_ grant you special actions as described in TODO: ADD REF. All resources empty during _Clean Up_ or, in other words, they are set to 0 at the end of the turn. Cash also empties at the end of each phase. For example, at the beginning of the phase _Invent_ you get _+1 Buy_, which allows you to buy one Technology or one Creature during this turn.

<figure>
    <img src="Images/initialDeck.PNG"
         alt="TurnPhases">
    <figcaption>The initial Deck: 6 <i>Money</i> cards: 6 x <i>Paper Money</i>, 2 <i>Technology</i> cards: <i>Junk</i> and <i>Clue</i>, 2 <i>Creature</i> cards: <i>Thug</i> and <i>Worker</i></figcaption>
</figure>

### The Phases
A turn of _Sors_ has nine phases, three of which are mandatory and six are optional. At the beginning of the turn, players choose two out of the six optional phases. Both players always play all mandatory phases and, in addition, ALL phases that ANY player picked. At the beginning of a phase, each player gets an effect described in the table below. Everyone who picked an optional phase, also gets the bonus at the beginning of that phase. Most phases are split into distinct steps, more to those detail later (TODO: Add ref). The phases are played in the following order, skipping the optional ones that NEITHER player has picked:

| Phase    | Optional | Effect | Bonus |  Steps
| -------- | ------- | ------- | ------- | -------
| **Phase Selection** | :x: | +2 Cards, choose 2 phases for this turn | - |  2  
| **Card Draw** | :heavy_check_mark: | +2 Cards, discard 1 | +2 Cards | 2
| **Invent** | :heavy_check_mark: | Buy Technologies from the market | +1 Buy, Technologies cost 1 less | 1+
| **Develop** | :heavy_check_mark: | Play Technologies | +1 Play, +2 Cash | 1+
| **Combat** | :x: | Creatures may deal damgage (Attack step) and block opposing Attackers (Defend step) | - | 4
| **Recruit** | :heavy_check_mark: | Buy Creatures from the market | +1 Buy, Creatures cost 1 less | 1+
| **Deploy** | :heavy_check_mark: | Play Creatures | +1 Play, +2 Cash | 1+
| **Prevail** | :heavy_check_mark: | +1 Prevail | +2 Prevail | 1-4
| **Clean Up** | :x: | The board resets and players get ready for the next turn. | - | 1  

<figure>
    <img src="Images/phases.PNG"
         alt="TurnPhases">
    <figcaption>At the beginning of the turn, each player picks two of the six optional phases. The phases <i>Phase Selection</i>, <i>Combat</i> and <i>Clean Up</i> happen every turn.</figcaption>
</figure>

### Cards and Entities
When you play Technologies and Creatures from your hand, they become **Entities** on the **Battlefield**. Entities only exist on the Battlefield. A card and the corresponding entity are coupled and share the same base stats amongst **Cost**, **Health**, **Abilities**, a title and a description. Money cards do not have an equivalent entity but have a **Value**, producing that amount of cash when you play them. The stats of a card may vary temporarily due to a phase bonus or abilities. Similary, entities may have their stats changed, for example when taking damage. This does not change the base stats of the corresponding card. Many abilities only trigger while the entity is on the Battlefield, although there are exceptions (TODO: ADD REF).

An entity **dies** when its health reaches zero. Then, it leaves the Battlefield and the corresponding card moves to your discard pile. A Creature's **Attack** determines how much damage it deals to other entities during _Combat_. Creatures may also have **Traits** that influence how that damage is dealt (TODO: ADD REF). Technologies on the other hand have a **Points** value, that doesn't do shit rn TODO. 

<figure>
    <img src="Images/CardsEntities.PNG"
         alt="CardsEntities">
    <figcaption> Stats of cards determine what you get from playing them. The top row shows an example card of each type (Money, Creature and Technology) and the bottom row shows the corresponding entities and how they look on the Battlefield. </figcaption>
</figure>

### The Board
To play a Technology or Creature card, you have to spend a _Play_. Doing so moves the card to the **Play Zone** and creates the corresponding entity on the Battlefield. It consists of two zones with six slots for Creature and Technology entities respectively. This means that you can have up to six Creatures and up to six Technologies in play simultaneously. Cards in the _Play Zone_ can't be interacted with and stay there until the corresponding entity dies. Once that happens, the entity is destroyed and the card moves to your **Discard** pile.

You can only play cards from your **Hand**. Money cards move to the **Money Zone** and are discarded at the end of each phase. If you have unspent cash and if the difference allows it, Money cards are returned to your hand. You draw cards from your **Deck**, moving them to your _Hand_. Whenever you attempt to draw, but your _Deck_ is empty, you shuffle your _Discard_ pile, move these cards to your _Deck_ and continue drawing. If you still can't draw more cards, you skip that draw instead. As a special action, you can trash cards from you _Hand_. Doing so moves the cards to the **Trash** zone, which normally can't be interacted with. The _Trash_ zone is the only one shared between both players.

The **Player/Opponent Stats** shows your and your opponent's stats, including the player name, current **Life Total**, **Points** and all resources gathered during the turn.

<figure>
    <img src="Images/board.PNG"
         alt="Board">
    <figcaption> The board and various zones where cards can be. Entities only exist on the Battlefield. Your opponent has the same zones but not all are visible.</figcaption>
</figure>




### The Phases in detail
>**1. Phase Selection**
> Each player draws 2 cards and then chooses 2 phases. Ability trigger: "At the beginning of the turn"

>**3. Invent**
>To buy a card, you spend a _Buy_ and pay an amount of _Cash_ equal to that card's _Cost_. Playing _Technologies_ and _Creatures_ also requires you to pay that cost and uses a _Play_.  Playing a money card is a special action and does not require to spend a _Play_ (you can play unlimited money cards from your hand).

#### Combat

During _Combat_ you can **attack** your opponent and/or their Technologies with your Creatures on the Battlefield. In an additional step, you choose how to **block** you opponent's attacking Creatures with your own.

### Abilities and Traits
Cards and entities have abilities. An ability has this structure 
>	**Trigger : Effect, Target**

#### Triggers

At the beginning of [[Phases]]
```
// At the beginning of [Phase]
PhaseSelection = 1,
Draw = 2,
Invent = 3,
Develop = 4,
Combat = 5,
Recruit = 6,
Deploy = 7,
Prevail = 8,
CleanUp = 9,
```
When ... - State changes and actions of [[Card]] / [[Entity]]
```
// When triggers
WhenYouBuy,
WhenYouPlay = 20,
WhenDies,
WhenAttacks,
WhenBlocks,
WhenGetsBlocked,
WhenTakesDamage,
WhenDealsDamage,
```
#### Effects

```
CardDraw = 1,
Damage = 5,
Life = 6,
Cash = 10,
PriceReduction = 11,
Destroy,
```
#### Targets 

```
Any = 1,
You = 2,
Opponent = 3,
AnyPlayer = 4,
Self = 7,
Entity = 8,
Creature = 9,
Technology = 10,
```
#### Traits

Changes how entity behaves during [[Combat]]. [[Creature]] only (?)
```
Deathtouch = 1,
FirstStrike = 2,
Trample = 3,
Lifelink,
Defender,
Attacker,
DoubleStrike,
Flying,
Reach,
Hexproof,
Shroud,
Menace,
```