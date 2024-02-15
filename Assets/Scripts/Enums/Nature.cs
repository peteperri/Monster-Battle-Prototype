using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Random = UnityEngine.Random;

//natures are 10% boosts or 10% reductions to stats. bane/boon. customizable across monster units
//source for natures and their effects: https://bulbapedia.bulbagarden.net/wiki/Nature
public enum Nature
{
    //boost str/atk
    Hardy, Lonely, Brave, Adamant, Naughty, 
    
    //boost def
    Bold, Docile, Relaxed, Impish, Lax, 
    
    //boost readiness/speed
    Timid, Hasty, Serious, Jolly, 
    
    //boost int/sp. atk
    Naive, Modest, Mild, Quiet, Bashful, Rash,
    
    //boost res/spdef
    Calm, Gentle, Sassy, Careful, Quirky
    
    //TODO: ADD NATURES TO BOOST/REDUCE REFLEX
    
}

//utility class for actions/constants relating to natures
public static class NatureHelper
{
    
    //some natures boost and reduce the same stat; these are the neutral natures
    public static readonly ReadOnlyDictionary<Nature, NatureEffect> NatureEffectsMap = new ReadOnlyDictionary<Nature, NatureEffect>(new Dictionary<Nature, NatureEffect>
        {
            //TODO: Is there a better way to do this? 
            
            //all natures that boost strength (phys atk)
            { Nature.Hardy, new NatureEffect(Stat.Strength, Stat.Strength) },
            { Nature.Lonely, new NatureEffect(Stat.Strength, Stat.Defense) },
            { Nature.Brave, new NatureEffect(Stat.Strength, Stat.Readiness) },
            { Nature.Adamant, new NatureEffect(Stat.Strength, Stat.Intelligence) },
            { Nature.Naughty, new NatureEffect(Stat.Strength, Stat.Resilience) },
            
            
            //all natures that boost def 
            { Nature.Bold, new NatureEffect(Stat.Defense, Stat.Strength) },
            { Nature.Docile, new NatureEffect(Stat.Defense, Stat.Defense) },
            { Nature.Relaxed, new NatureEffect(Stat.Defense, Stat.Readiness) },
            { Nature.Impish, new NatureEffect(Stat.Defense, Stat.Intelligence) },
            { Nature.Lax, new NatureEffect(Stat.Defense, Stat.Resilience) },
            
            //all natures that boost readiness (speed)
            { Nature.Timid, new NatureEffect(Stat.Readiness, Stat.Strength) },
            { Nature.Hasty, new NatureEffect(Stat.Readiness, Stat.Defense) },
            { Nature.Serious, new NatureEffect(Stat.Readiness, Stat.Readiness) },
            { Nature.Jolly, new NatureEffect(Stat.Readiness, Stat.Intelligence) },
            { Nature.Naive, new NatureEffect(Stat.Readiness, Stat.Resilience) },
            
            //all natures that boost int (sp. atk)
            { Nature.Modest, new NatureEffect(Stat.Intelligence, Stat.Strength) },
            { Nature.Mild, new NatureEffect(Stat.Intelligence, Stat.Defense) },
            { Nature.Quiet, new NatureEffect(Stat.Intelligence, Stat.Readiness) },
            { Nature.Bashful, new NatureEffect(Stat.Intelligence, Stat.Intelligence) },
            { Nature.Rash, new NatureEffect(Stat.Intelligence, Stat.Resilience) },
            
            //all natures that boost res (sp. def)
            { Nature.Calm, new NatureEffect(Stat.Resilience, Stat.Strength) },
            { Nature.Gentle, new NatureEffect(Stat.Resilience, Stat.Defense) },
            { Nature.Sassy, new NatureEffect(Stat.Resilience, Stat.Readiness) },
            { Nature.Careful, new NatureEffect(Stat.Resilience, Stat.Intelligence) },
            { Nature.Quirky, new NatureEffect(Stat.Resilience, Stat.Resilience) },
            
            //TODO: add natures to boost/reduce reflex
        });
    
    public static Nature GetRandomNature()
    {
        Array natures = Enum.GetValues(typeof(Nature));
        int randIndex = Random.Range(0, natures.Length - 1);
        return (Nature) natures.GetValue(randIndex);
    }
}



