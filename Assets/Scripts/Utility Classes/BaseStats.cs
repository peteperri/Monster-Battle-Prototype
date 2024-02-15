using UnityEngine;


//base stats are constant across species. a MonsterUnit's base stats are not customizable, but its EVs are 
[System.Serializable]
public struct BaseStats
{
    [field: SerializeField] public int Health { get; private set; }
    [field: SerializeField] public int Strength { get; private set; }
    [field: SerializeField] public int Defense { get; private set; }
    [field: SerializeField] public int Intelligence { get; private set; }
    [field: SerializeField] public int Resilience { get; private set; }
    [field: SerializeField] public int Readiness { get; private set; }
    [field: SerializeField] public int Reflex { get; private set; }

    //in stat calculation, we need to iterate through all of the base stats, so this helper method returns an array of them
    public int[] GetAllStats()
    {
        return new[] { Health, Strength, Defense, Intelligence, Resilience, Readiness, Reflex };
    }
}
