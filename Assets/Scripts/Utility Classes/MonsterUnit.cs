using System;
using AYellowpaper.SerializedCollections;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

[Serializable]
public class MonsterUnit
{
    private MonsterSpecies species;
    private EffortValues statInvestments;
    private Nature nature;
    
    //just using a SerializedDictionary without actually serializing it so I can easily view it in the Debug inspector
    private SerializedDictionary<Stat, int> _stats;

    public MonsterUnit(MonsterSpecies species, EffortValues evs, Nature nature)
    {
        this.species = species;
        this.statInvestments = evs;
        this.nature = nature;
        _stats = new SerializedDictionary<Stat, int>();
        ComputeStats();
    }
    
    //chained constructors
    public MonsterUnit(MonsterSpecies species) 
        : this(species, new EffortValues(), Nature.Hardy) {}
    public MonsterUnit(MonsterSpecies species, Nature nature) 
        : this(species, new EffortValues(), nature) {}

    public void ComputeStats()
    {

        BaseStats baseStats = species.BaseStats;
        int[] calculatedStats = CalcAllStats(baseStats, statInvestments, nature);
        Array statNames = Enum.GetValues(typeof(Stat));

        for (int i = 0; i < calculatedStats.Length; i++)
        {
            Stat statName = (Stat)statNames.GetValue(i);
            _stats.Add(statName, calculatedStats[i]);
        }

        /*_stats = new SerializedDictionary<Stat, int>
        {
            //add further calculation later
            {Stat.Health, species.BaseStats.Health},
            {Stat.Strength, species.BaseStats.Strength},
            {Stat.Defense, species.BaseStats.Defense},
            {Stat.Intelligence, species.BaseStats.Intelligence},
            {Stat.Resilience, species.BaseStats.Resilience},
            {Stat.Readiness, species.BaseStats.Readiness},
            {Stat.Reflex, species.BaseStats.Reflex}
        };*/
    }

    public MonsterSpecies GetSpecies()
    {
        return species;
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




