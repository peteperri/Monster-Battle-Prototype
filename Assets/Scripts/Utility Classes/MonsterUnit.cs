using System;
using AYellowpaper.SerializedCollections;
using UnityEngine;

//a monster unit is an individual, customized monster. only customizable/changeable traits are controlled by this class
[Serializable]
public class MonsterUnit
{
    private MonsterSpecies _species;
    private EffortValues _statInvestments;
    private Nature _nature;

    private Attack[] _knownAttacks;
    
    //just using a SerializedDictionary without actually serializing it so I can easily view it in the Debug inspector
    private SerializedDictionary<Stat, int> _statsBeforeModifiers;
    private SerializedDictionary<Stat, int> _statsAfterMultipliers;
    private SerializedDictionary<Stat, int> _statModifierStages;

    public MonsterUnit(MonsterSpecies species, EffortValues evs, Nature nature)
    {
        _species = species;
        _statInvestments = evs;
        _nature = nature;
        _statsBeforeModifiers = new SerializedDictionary<Stat, int>();
        _statsAfterMultipliers = new SerializedDictionary<Stat, int>();
        _statModifierStages = new SerializedDictionary<Stat, int>();
        _knownAttacks = new Attack[4];
        
        //TODO: COMMENT OUT THIS LOOP
        //currently gives every mon the first four moves in their learnset
        for (int i = 0; i < _knownAttacks.Length && i < _species.LearnSet.Length; i++)
        {
            _knownAttacks[i] = _species.LearnSet[i];
        }

        ComputeStartingStats();
    }
    
    //chained constructors
    public MonsterUnit(MonsterSpecies species) 
        : this(species, new EffortValues(), Nature.Hardy) {}
    public MonsterUnit(MonsterSpecies species, Nature nature) 
        : this(species, new EffortValues(), nature) {}

    public void ComputeStartingStats()
    {
        BaseStats baseStats = _species.BaseStats;
        int[] calculatedStats = CalcAllStats(baseStats, _statInvestments, _nature);
        Array statNames = Enum.GetValues(typeof(Stat));

        for (int i = 0; i < calculatedStats.Length; i++)
        {
            Stat statName = (Stat)statNames.GetValue(i);
            _statsBeforeModifiers.Add(statName, calculatedStats[i]);
            _statsAfterMultipliers.Add(statName, calculatedStats[i]);
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
            _statsAfterMultipliers[stat] = (int) newStat;
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

    public void UseAttack(int attackIndex, MonsterUnit[] targets)
    {
        if (attackIndex > _knownAttacks.Length || attackIndex < 0)
        {
            return;
        }
        
        

        Attack attack = _knownAttacks[attackIndex];
        foreach (MonsterUnit target in targets)
        {
            Debug.Log($"{_species.name} is using {attack.name} on {target._species.name}!");
            int damageToDeal = 0;
            if (attack.Category == AttackCategory.Physical)
            {
                float attackStat = this._statsAfterMultipliers[Stat.Strength];
                float defenseStat = target._statsAfterMultipliers[Stat.Defense];
                damageToDeal = CalculateDamage(attackStat, defenseStat, attack.BasePower);
                target.TakeDamage(damageToDeal);
            }
            else if (attack.Category == AttackCategory.Special)
            {
                float attackStat = this._statsAfterMultipliers[Stat.Intelligence];
                float defenseStat = target._statsAfterMultipliers[Stat.Resilience];
                damageToDeal = CalculateDamage(attackStat, defenseStat, attack.BasePower);
                target.TakeDamage(damageToDeal);
            }
            for (int i = 0; i < attack.SecondaryEffect.Length; i++)
            {
                attack.SecondaryEffect[i].ExecuteSecondaryEffect(this, target);
            }
        }
    }
    
    public void TakeDamage(int amount)
    {
        int currentHealth = _statsAfterMultipliers[Stat.Health];
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, _statsBeforeModifiers[Stat.Health]);
        _statsAfterMultipliers[Stat.Health] = currentHealth;
        if (currentHealth <= 0)
        {
            Debug.Log($"{this._species.name} has fainted!");
        }
    }

    public void Heal(float percentage)
    {
        int maxHealth = _statsBeforeModifiers[Stat.Health];
        percentage /= 100; //turns "50%" into "0.5"
        float healAmount = maxHealth * percentage;
        
        //ensure we do not heal above max HP
        healAmount = Mathf.Clamp(healAmount, 0, maxHealth);

        _statsAfterMultipliers[Stat.Health] += (int) healAmount;
    }



    public static int[] CalcAllStats(BaseStats baseStats, EffortValues evs, Nature nature, int iv = 31, int level = 100)
    {
        int[] calculatedStats = new int[7];
        int[] allBaseStats = baseStats.GetAllStats();
        int[] allEVs = evs.GetAllEVs();
        
        NatureEffect natureEffect = NatureHelper.NatureEffectsMap[nature];
        
        for (int i = 0; i < 7; i++)
        {
            if (i == 0)
            {
                calculatedStats[i] = CalcHealthStat(baseStats, evs);
            }
            else
            {
                float natureMultiplier = natureEffect.GetMultiplier(i);
                calculatedStats[i] = CalcOtherStat(allBaseStats[i], allEVs[i], natureMultiplier);
            }
        }
        return calculatedStats;
    }
    
    //formula source: "Generation III onward" https://bulbapedia.bulbagarden.net/wiki/Stat#Base_stat_values 
    public static int CalcHealthStat(BaseStats baseStat, EffortValues evs, int iv = 31, int level = 100)
    {
        return (((2 * baseStat.Health + iv + (evs.healthValues / 4)) * level) / 100) + level + 10;
    }

    public static int CalcOtherStat(float baseStat, float ev, float natureMultiplier, float iv = 31, float level = 100)
    {
        float rawStat = (((2 * baseStat + iv + (ev / 4)) * level) / 100) + 5;
        return (int) (rawStat * natureMultiplier);
    }

    //formula source: "Generation V onward" https://bulbapedia.bulbagarden.net/wiki/Damage
    public static int CalculateDamage(float myAttackingStat, float opponentsDefendingStat, float movePower, float multipliers = 1, int level = 100)
    {
        float levelFactor = ((2 * level) / 5 + 2);
        float statFactor = movePower * (myAttackingStat / opponentsDefendingStat);
        float rawDamage = (levelFactor * statFactor) / 50 + 2;
        return Mathf.RoundToInt(rawDamage * multipliers);
    }
}




