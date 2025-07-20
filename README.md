# Rogue Trader Operative Rework

A mod for Warhammer 40k: Rogue Trader that overhauls the Operative class to increase its powerlevel and streamline its play pattern to be fully competitive with other T1 archetypes.

The mod is fully playable in its existing state, however the balancing is currently a work in progress and certain aspects may be under or overpowered.

## Core class design

The reworked Operative is based around providing light team support paired with a crippling debuff and massive damage through Expose Weakness. Exploits are no longer consumed on hit and provide a small generic damage boost that the entire party can make use of, however the actual damage the Operative gains from them is fairly minor compared to base. When the Operative consumes these stacks, rather than trading damage for armor penetration they now combine the team damage boost of the exploits and the armor/dodge reduction of Expose Weakness for that turn, but the Operative's next hit vs the target is massively boosted and gains bonus effects based on the number of stacks removed:

- 3+ stacks makes the attack auto-hit
- 6+ stacks makes the attack auto-crit and ignore deflection
- 10+ stacks gives the Operative an extra attack and restores 2 AP

All of the Operative's abilities and many of their talents have also been updated to improve scaling or overall power level, and in particular the reworked Continuous Analysis talent and the ability to stack exploits and Expose Weakness to much higher levels provides real incentive to give extra turns to an Operative. The end result is that the Operative revolves around providing helpful team support and immense single target damage and debuffing to eliminate priority targets; they still have fairly limited ability to hit multiple targets and poor mobility, but now they have the single target damage to compensate for their limited ability to kill chaff.

## Full changelist

- Exploit: No longer removed on hit. Stacks increase all damage taken by 5%, including for damage over time and from allies. The operative deals +5 + PER% per stack instead. Exploits from multiple Operatives still stack on the same enemy, however Operative's only gain the +PER% damage vs their own exploits and cannot consume exploits from other Operatives through Expose Weakness or Tactical Knowledge.
- Expose Weakness: Now increases damage by 5% per stack, debuff scaling changed to a flat -5% armor, parry, block, and dodge. Casts of Expose Weakness now stack between uses by the same Operative, although duration is not increased by subsequent casts. After using Expose Weakness, the Operative's next attack vs the target deals an additional +(10 + stacks) x PER% damage and provides further boosts based on the number of stacks removed:

  - 3+ stacks: The attack auto-hits
  - 6+ stacks: The attack auto-crits and ignores deflection
  - 10+ stacks: The operative regains 2 AP and can make an extra attack this turn

- Joint Analysis: No longer removes exploits, instead adds an exploit when allies hit an enemy, but limited to one proc per enemy per turn.
- Precise Attack: No longer requires exploits to work. Scaling 15 + 3 _ exploits% -> 10 + 2 _ PER%. Now also reduces dodge. Now additionally applies 2 exploits.
- Informed Hit: Critical hits with Precise Attack apply an additional stack of exploit. Auto-crit threshold 10 - INT -> 6 - INT/2.
- Insightful Precision: Scaling changed to flat +5 perception when using Precise Attack
- Intimidation: Scaling 5 + PER/2% -> 15 + PER%, but no longer doubles vs low armor. Also reduces MP by 3.
- Terrifying Presence: Killing a target with Intimidation now also extends the range to LoS.
- Perfect Spot: Applies exploits to all enemies in LoS at the start of the Operative's turn.
- Tactical Knowledge: No longer removes all stacks when cast vs a single target. Armor scaling 1% -> 1 + INT/5%. Damage scaling 2% -> 2 + INT/2%. Stacks between multiple casts.
- Improved Tactics: New effect, adds a stack of Tactical Knowledge at the start of combat and grants +1 additional stacks when using Tactical Knowledge.
- Dismantling Attack: Applies 6 stacks of Exploit Weakness instead of applying a -30% penalty to armor, dodge, parry, and block and gains the first hit damage bonus of Exploit Weakness. Dismantling Attack also makes Exploit Weakness no longer expire from the target.
- Comprehensive Analysis: New effect, analyze enemy distributes half the stacks of exploit on enemies within 5 tiles. If there are no enemies within 5 tiles those exploits are placed on the main target.
- Continuous Analysis: New effect, applies exploits on combat start and on extra turns in addition to at the start of normal turns.
- Inflict Despair: Triggers if enemy has exploit and expose weakness -> triggers if enemy has expose weakness.
- Joint Offense: Scaling INT/2% -> 2 + INT/2%
- Passive Learning: New effect, grants +1 intelligence when an enemy with an exploit dies.
- Reactive Study: Doesn't require an exploit to proc (not a change in behavior, in-game tooltip doesn't reflect actual behavior)
- Sharpshooter: Scaling every 5 tiles -> every 4 tiles. Works only on single shots -> works on all attacks, but only on the first attack of bursts.
- Tide of Excellence: Gains stacks when hitting a target with an exploit -> gains stacks when removing exploits or killing a target with an exploit.
- Aeldari Ranger Sight: Doesn't inflict permanent expose weakness, extends the duration of expose weakness by 1 round.
- Omnissiah's Providence: Inflict 1 exploit and expose weakness after attacking -> cast exploit weakness on targets before attacking
