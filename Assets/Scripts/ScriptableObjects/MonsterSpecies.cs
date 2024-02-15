using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Monster Species", menuName = "Monster Species")]
public class MonsterSpecies : ScriptableObject
{
    [SerializeField] private string speciesName;
    [SerializeField] private ElementalType[] typing;
    [field: SerializeField] public BaseStats BaseStats { get; private set; }
    
    //source for all pokemon sprites: 
    [SerializeField] private Sprite sprite;

    public Sprite GetSprite()
    {
        return sprite;
    }
}


