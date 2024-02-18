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

    private Attack[] _knownMoves;
    
    //just using a SerializedDictionary without actually serializing it so I can easily view it in the Debug inspector
    private SerializedDictionary<Stat, int> _statsBeforeModifiers;
    
    public SerializedDictionary<Stat, int> StatsAfterMultipliers { get; }
    public SerializedDictionary<Stat, int> StatModifierStages { get; }

    public MonsterUnit(MonsterSpecies species, EffortValues evs, Nature nature)
    {
        _species = species;
        _statInvestments = evs;
        _nature = nature;
        _statsBeforeModifiers = new SerializedDictionary<Stat, int>();
        StatsAfterMultipliers = new SerializedDictionary<Stat, int>();
        StatModifierStages = new SerializedDictionary<Stat, int>();
        _knownMoves = new Attack[4];
        
        //TODO: COMMENT OUT THIS LOOP
        //currently gives every mon the first four moves in their learnset
        for (int i = 0; i < _knownMoves.Length && i < _species.LearnSet.Length; i++)
        {
            _knownMoves[i] = _species.LearnSet[i];
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
            StatsAfterMultipliers.Add(statName, calculatedStats[i]);
        }
    }
    
    public void ComputeModifiedStats()
    {
        foreach (Stat stat in Enum.GetValues(typeof(Stat)))
        {
            if (stat == Stat.Health) continue;
            
            int modifierStages = StatModifierStages[stat];
            int multiplierNumerator = 2;
            int multiplierDenominator = 2;

            if (modifierStages > 0)
            {
                multiplierNumerator += modifierStages;
            }
            else if (modifierStages < 0)
            {
                multiplierDenominator += modifierStages;
            }

            float newStat = _statsBeforeModifiers[stat] * ((float) multiplierNumerator / multiplierDenominator);
            StatsAfterMultipliers[stat] = (int) newStat;
        }
    }

    public MonsterSpecies GetSpecies()
    {
        return _species;
    }

    public void ApplyStatModifier(Stat statToModify, int byHowManyStages)
    {
        int currentStatModifiers = StatModifierStages[statToModify];
        bool statCannotGoLower = byHowManyStages < 0 && currentStatModifiers <= -6;
        bool statCannotGoHigher = byHowManyStages > 0 && currentStatModifiers >= 6;
        if (statCannotGoHigher || statCannotGoLower)
        {
            return;
        }

        int newStatModifier = currentStatModifiers + byHowManyStages;
        Mathf.Clamp(newStatModifier, -6, 6);

        StatModifierStages[statToModify] = newStatModifier;
        ComputeModifiedStats();

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

}




