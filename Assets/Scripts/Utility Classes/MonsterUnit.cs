using System;
using AYellowpaper.SerializedCollections;
using UnityEngine;

[Serializable]
public class MonsterUnit
{
    [SerializeField] private MonsterSpecies species;
    private SerializedDictionary<Stat, int> _stats;

    public MonsterUnit(MonsterSpecies species)
    {
        this.species = species;
        _stats = new SerializedDictionary<Stat, int>
        {
            //add further calculation later
            {Stat.Health, species.BaseStats.Health},
            {Stat.Strength, species.BaseStats.Strength},
            {Stat.Defense, species.BaseStats.Defense},
            {Stat.Intelligence, species.BaseStats.Intelligence},
            {Stat.Resilience, species.BaseStats.Resilience},
            {Stat.Readiness, species.BaseStats.Readiness},
            {Stat.Reflex, species.BaseStats.Reflex}
        };
    }

    public MonsterSpecies GetSpecies()
    {
        return species;
    }
}


/*[Serializable]
public class MonsterSpecies
{
    [SerializeField] private string name;
    [SerializeField] private BaseStats baseStats;
    

    public MonsterSpecies(string name)
    {
        this.name = name;
        
    }

    public MonsterSpecies(string name, BaseStats baseStats)
    {
        this.name = name;
        this.baseStats = baseStats;
    }
}*/




