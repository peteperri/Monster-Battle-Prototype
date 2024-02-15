using UnityEngine;
using System;

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

    public EffortValues()
    {
        this.healthValues = 85;
        this.strengthValues = 85;
        this.defenseValues = 85;
        this.intelligenceValues = 85;
        this.resilienceValues = 85;
        this.readinessValues = 85;
        this.reflexValues = 85;
    }
    
    public int[] GetAllEVs()
    {
        return new[] { healthValues, strengthValues, defenseValues, intelligenceValues, resilienceValues, readinessValues, reflexValues };
    }

}
