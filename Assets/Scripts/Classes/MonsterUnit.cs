using System;
using System.Linq;
using AYellowpaper.SerializedCollections;
using UnityEditor.Experimental.Rendering;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

//a monster unit is an individual, customized monster. only customizable/changeable traits are controlled by this class
[Serializable]
public class MonsterUnit
{
    private MonsterSpecies _species;
    private EffortValues _statInvestments;
    private Nature _nature;

    //by default, a monster unit's name will just be it's species name. however, nicknames are a feature that i want to support once customization is implemented
    public string UnitName { get; }
    public Attack[] KnownAttacks { get; }
    
    public bool Fainted { get; private set; }

    public BattlePosition PositionInBattle { get; set; }

    //just using a SerializedDictionary without actually serializing it so I can easily view it in the Debug inspector
    private SerializedDictionary<Stat, int> _statsBeforeModifiers;
    private SerializedDictionary<Stat, int> _statsAfterModifiers;
    private SerializedDictionary<Stat, int> _statModifierStages;

    public MonsterUnit(MonsterSpecies species, EffortValues evs, Nature nature, string nickname)
    {
        _species = species;
        _statInvestments = evs;
        _nature = nature;
        _statsBeforeModifiers = new SerializedDictionary<Stat, int>();
        _statsAfterModifiers = new SerializedDictionary<Stat, int>();
        _statModifierStages = new SerializedDictionary<Stat, int>();
        
        KnownAttacks = new Attack[4];
        UnitName = nickname;
        Fainted = false;
        PositionInBattle = null;
        
        //Fainted = Random.Range(1, 3) == 1;
       
        
        //currently gives every mon the first four moves in their learnset; needs to be removed if customization is implemented
        for (int i = 0; i < KnownAttacks.Length && i < _species.LearnSet.Length; i++)
        {
            KnownAttacks[i] = _species.LearnSet[i];
        }

        ComputeStartingStats();
    }
    
    //chained constructors
    public MonsterUnit(MonsterSpecies species) 
        : this(species, new EffortValues(), Nature.Hardy, species.GetName()) {}
    
    public MonsterUnit(MonsterSpecies species, Nature nature) 
        : this(species, new EffortValues(), nature, species.GetName()) {}

    public MonsterUnit(MonsterSpecies species, EffortValues evs, Nature nature)
        : this(species, evs, nature, species.GetName()) {}

    public void ComputeStartingStats()
    {
        BaseStats baseStats = _species.BaseStats;
        int[] calculatedStats = Calculator.CalcAllStats(baseStats, _statInvestments, _nature);
        Array statNames = Enum.GetValues(typeof(Stat));

        for (int i = 0; i < calculatedStats.Length; i++)
        {
            Stat statName = (Stat)statNames.GetValue(i);
            _statsBeforeModifiers.Add(statName, calculatedStats[i]);
            _statsAfterModifiers.Add(statName, calculatedStats[i]);
            _statModifierStages.Add(statName, 0);
        }
    }
    
    public void ComputeModifiedStats()
    {
        foreach (Stat stat in Enum.GetValues(typeof(Stat)))
        {
            if (stat == Stat.Health) continue;
            
            int modifierStages = _statModifierStages[stat];
            int multiplierNumerator = 2;
            int multiplierDenominator = 2;

            if (modifierStages > 0)
            {
                multiplierNumerator += modifierStages;
            }
            else if (modifierStages < 0)
            {
                multiplierDenominator += Math.Abs(modifierStages); //absolute value because stages are negative but we want the denominator to increase 
            }
            
            //Debug.Log($"{multiplierNumerator}/{multiplierDenominator}");
            
            float newStat = _statsBeforeModifiers[stat] * ((float) multiplierNumerator / multiplierDenominator);
            _statsAfterModifiers[stat] = (int) newStat;
        }
    }

    public MonsterSpecies GetSpecies()
    {
        return _species;
    }

    public void ApplyStatModifier(Stat statToModify, int byHowManyStages)
    {
        int currentStatModifiers = _statModifierStages[statToModify];
        bool statCannotGoLower = byHowManyStages < 0 && currentStatModifiers <= -6;
        bool statCannotGoHigher = byHowManyStages > 0 && currentStatModifiers >= 6;
        if (statCannotGoHigher || statCannotGoLower)
        {
            return;
        }

        int newStatModifier = currentStatModifiers + byHowManyStages;
        Mathf.Clamp(newStatModifier, -6, 6);

        _statModifierStages[statToModify] = newStatModifier;
        ComputeModifiedStats();
    }

    //this method takes in an array of targets for future double-battle functionality
    //at this time, this array will only ever have a size of one, because 
    //double battles are not yet implemented.
    public void UseAttack(int attackIndex, MonsterUnit[] targets, bool attackFailed, bool moveMissed = false)
    {
        if (attackIndex > KnownAttacks.Length || attackIndex < 0)
        {
            //Debug.Log($"{UnitName} tried to use a move outside of its range.");
            return;
        }

        if (Fainted)
        {
            //Debug.Log($"{UnitName} has fainted and cannot attack!");
            return;
        }
        
        Attack attack = KnownAttacks[attackIndex];
        Battle.StaticMessage($"{_species.name} used {attack.name}!");
        
        if (attackFailed)
        {
            Battle.StaticMessage($"{Battle.GetCurrentMessage()} ...But it failed!");
            return;
        }
        
        if (attack.Target == AttackTarget.Self)
        {
            ExecuteAllSecondaryEffects(attack, this, 0, moveMissed);
            return;
        }
        
        AttackAllTargets(attack, targets);

    }

    public void AttackAllTargets(Attack attack, MonsterUnit[] targets)
    {
        foreach (MonsterUnit target in targets)
        {
            AttackSingleTarget(attack, target);
        }
    }

    private void AttackSingleTarget(Attack attack, MonsterUnit target)
    {
        //Debug.Log($"{_species.name} used {attack.name} on {target._species.name}!");
           
        int damageDealt = 0;

        MonsterSpecies targetSpecies = target.GetSpecies();

        bool moveMissed = MoveMissed(attack);
        if (moveMissed)
        {
            Battle.StaticMessage($"{Battle.GetCurrentMessage()} ...But it missed!");
            MissRecoilCheck(attack);
            return;
        }

        float typingMultiplier = targetSpecies.GetTypeMultiplier(attack.Type);
        if (typingMultiplier == 0)
        {
            Battle.StaticMessage($"{Battle.GetCurrentMessage()} ...But the target was immune!");
            MissRecoilCheck(attack);
            return;
        }
        
        if (attack.Category != AttackCategory.Status)
        {
            if (typingMultiplier < 1)
            {
                Battle.StaticMessage($"{Battle.GetCurrentMessage()} ...It's not very effective.");
                SFX.Play(SoundEffect.HitNotVeryEffective);
            }
            else if (typingMultiplier > 1)
            {
                Battle.StaticMessage($"{Battle.GetCurrentMessage()} ...It was super effective!");
                SFX.Play(SoundEffect.HitSuperEffective);
            }
            else
            {
                SFX.Play(SoundEffect.HitNormal);
            }

            bool isCrit = IsCrit(attack);
            if (isCrit)
            {
                Battle.StaticMessage($"{Battle.GetCurrentMessage()} A critical hit!");
            }

            damageDealt = DealDamage(attack, target, isCrit, typingMultiplier);
        }
        ExecuteAllSecondaryEffects(attack, target, damageDealt, moveMissed);
    }

    private int DealDamage(Attack attack, MonsterUnit target, bool isCrit, float typingMultiplier)
    {
        float attackStat;
        float defenseStat;
        float defenseStatPreMods;
            
        if (attack.Category == AttackCategory.Physical)
        {
            attackStat = this._statsAfterModifiers[Stat.Strength];
            defenseStat = target._statsAfterModifiers[Stat.Defense];
            defenseStatPreMods = target._statsBeforeModifiers[Stat.Defense];
        }
        else
        {
            attackStat = this._statsAfterModifiers[Stat.Intelligence];
            defenseStat = target._statsAfterModifiers[Stat.Resilience];
            defenseStatPreMods = target._statsBeforeModifiers[Stat.Resilience];
        }
            
        //critical hits ignore defense boosts, but they do not ignore defense drops 
        if (isCrit && defenseStat > defenseStatPreMods)
        {
            defenseStat = defenseStatPreMods;
        }

        float stabMultiplier = IsStab(attack) ? 1.5f : 1;
        float critMultiplier = isCrit ? 1.5f : 1;
            
        //Debug.Log($"Stab multiplier? {stabMultiplier}");
        //Debug.Log($"Typing multiplier? {typingMultiplier}");
        //Debug.Log($"Crit multiplier? {critMultiplier}");
        float allMultipliers = typingMultiplier * stabMultiplier * critMultiplier;
            
        int damageToDeal = Calculator.CalculateDamage(attackStat, defenseStat, attack.BasePower, allMultipliers);
        target.TakeDamage(damageToDeal);
        return damageToDeal;
    }

    private void ExecuteAllSecondaryEffects(Attack attack, MonsterUnit target, int damageToDeal, bool moveMissed)
    {
        if (attack.SecondaryEffects == null || attack.SecondaryEffects.Length == 0) return;
        
        for (int i = 0; i < attack.SecondaryEffects.Length; i++)
        {
            attack.SecondaryEffects[i].ExecuteSecondaryEffect(this, target, damageToDeal, moveMissed);
        }
    }

    public void TakeDamage(int amount)
    {
        int currentHealth = _statsAfterModifiers[Stat.Health];
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, _statsBeforeModifiers[Stat.Health]);
        _statsAfterModifiers[Stat.Health] = currentHealth;
        if (currentHealth <= 0)
        {
            Fainted = true;
            Battle.StaticMessage($"{Battle.GetCurrentMessage()}\n{this._species.name} has fainted!");
            //Debug.Log($"{this._species.name} has fainted!");
        }

        PositionInBattle.UpdateStatus();
    }

    //returns true if healed, false if was at max HP already
    public bool Heal(float percentage)
    {
        int maxHealth = _statsBeforeModifiers[Stat.Health];
        int currentHealth = _statsAfterModifiers[Stat.Health];
        if (currentHealth >= maxHealth)
        {
            return false;
        }
        percentage /= 100; //turns "50" into "0.5"
        float healAmount = maxHealth * percentage;
        
        //ensure we do not heal above max HP
        healAmount = Mathf.Clamp(healAmount, 0, maxHealth);

        
        int newHealth = currentHealth + (int) healAmount;

        if (newHealth > maxHealth)
        {
            newHealth = maxHealth;
        }
        _statsAfterModifiers[Stat.Health] = newHealth;
        PositionInBattle.UpdateStatus();
        return true;
    }

    public int GetMaxHealth()
    {
        return _statsBeforeModifiers[Stat.Health];
    }

    public int GetStat(Stat stat)
    {
        return _statsAfterModifiers[stat];
    }

    private bool IsStab(Attack attack)
    { 
        ElementalType[] myTyping = _species.GetTyping();
        ElementalType attackType = attack.Type;
        
        //a move is stab is the attack type matches my first type or
        //if i have a second type, and it matches that type.
        return attackType == myTyping[0] || (myTyping.Length > 1 && attackType == myTyping[1]);
    }

    //Source: https://bulbapedia.bulbagarden.net/wiki/Critical_hit (probability/stage modifiers section)
    private static bool IsCrit(Attack attack)
    {
        CriticalRate rate = attack.CritRate;
        float denominator;
        switch (rate)
        {
            case CriticalRate.Guaranteed:
                //if the move has a guaranteed crit rate, then this was a crit!
                return true;
            case CriticalRate.High:
                //if the move has a high crit rate, increase the percentage chance by decreasing the denominator
                denominator = 8;
                break;
            case CriticalRate.Normal: 
            default:
                //normal pokemon crit rate past Gen 7 is 1/24
                denominator = 24;
                break;
        }
        float percentageChance = 1 / denominator;
        float rand = Random.Range(0f, 1f);
        return rand <= percentageChance;
    }

    private static bool MoveMissed(Attack attack)
    {
        int accuracy = attack.Accuracy;
        return Random.Range(1, 100) >= accuracy;
    }

    //must be called when a monster switches out, or when haze or a similar is used
    public void ResetStatModifiers()
    {
        foreach (Stat key in _statModifierStages.Keys.ToList())
        {
            _statModifierStages[key] = 0;
        }
        ComputeModifiedStats();
    }

    //some moves, like high jump kick, require their secondary effect to execute if and only if the attack does not conect.
    private void MissRecoilCheck(Attack attack)
    {
        AttackEffect[] attackEffects = attack.SecondaryEffects;
        if (attackEffects != null && attackEffects.Length > 0 && attackEffects[0] is MissRecoilDamageEffect)
        {
            attack.SecondaryEffects[0].ExecuteSecondaryEffect(this, null, 0, true);
        }
    }
    
}




