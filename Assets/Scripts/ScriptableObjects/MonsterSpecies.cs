using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Monster Species", menuName = "Monster Species")]
public class MonsterSpecies : ScriptableObject
{
    [SerializeField] private string speciesName;
    [SerializeField] private ElementalType[] typing;
    [field: SerializeField] public BaseStats BaseStats { get; private set; }
    [SerializeField] private Sprite sprite;

    public Sprite GetSprite()
    {
        return sprite;
    }
}

[Serializable]
public struct BaseStats
{
    [field: SerializeField] public int Health { get; private set; }
    [field: SerializeField] public int Strength { get; private set; }
    [field: SerializeField] public int Defense { get; private set; }
    [field: SerializeField] public int Intelligence { get; private set; }
    [field: SerializeField] public int Resilience { get; private set; }
    [field: SerializeField] public int Readiness { get; private set; }
    [field: SerializeField] public int Reflex { get; private set; }
}
