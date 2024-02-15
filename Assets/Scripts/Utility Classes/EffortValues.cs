using UnityEngine;
using System;


//effort values are the investment that trainers can put into various stats to customize their monster's build. a MonsterUnit's base stats are not customizable, but its EVs are 
[Serializable]
public class EffortValues
{
    [Range(0, 252)] [Header("Health EVs")] 
    public int healthValues;
    
    [Range(0, 252)] [Header("Strength EVs")] 
    public int strengthValues;
    
    [Range(0, 252)] [Header("Defense EVs")] 
    public int defenseValues;
    
    [Range(0, 252)] [Header("Intelligence EVs")] 
    public int intelligenceValues;
    
    [Range(0, 252)] [Header("Resilience EVs")] 
    public int resilienceValues;
    
    [Range(0, 252)] [Header("Readiness EVs")] 
    public int readinessValues;
    
    [Range(0, 252)] [Header("Reflex EVs")] 
    public int reflexValues;

    
    //will add parameterized constructor for use when customizing EVs later.
    public EffortValues()
    {
        healthValues = 85;
        strengthValues = 85;
        defenseValues = 85;
        intelligenceValues = 85;
        resilienceValues = 85;
        readinessValues = 85;
        reflexValues = 85;
    }
    
    //in stat calculation, we need to iterate through all of the EVs, so this helper method returns an array of them
    public int[] GetAllEVs()
    {
        return new[] { healthValues, strengthValues, defenseValues, intelligenceValues, resilienceValues, readinessValues, reflexValues };
    }
}
