using UnityEngine;

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

    public int[] GetAllStats()
    {
        return new[] { Health, Strength, Defense, Intelligence, Resilience, Readiness, Reflex };
    }
}
