using System;

//source for nature effects: https://bulbapedia.bulbagarden.net/wiki/Nature
public class NatureEffect
{
    
    private readonly Stat _boostedStat;
    private readonly Stat _reducedStat;

    public NatureEffect(Stat boostedStat, Stat reducedStat)
    {
        _boostedStat = boostedStat;
        _reducedStat = reducedStat;
    }

    public float GetMultiplier(int index)
    {
        Array stats = Enum.GetValues(typeof(Stat));
        Stat statToCheck = (Stat)stats.GetValue(index);

        if (_boostedStat == _reducedStat) return 1f;
        
        if (_boostedStat == statToCheck) return 1.1f;
        
        if (_reducedStat == statToCheck) return 0.9f;
        
        return 1f;
        
        

        /*switch (index)
        {
            case 1:
                return GetMultiplierFromStat(Stat.Strength);
            case 2: 
                return GetMultiplierFromStat(Stat.Defense);
            case 3: 
                return GetMultiplierFromStat(Stat.Intelligence);
            case 4: 
                return GetMultiplierFromStat(Stat.Resilience);
            case 5: 
                return GetMultiplierFromStat(Stat.Readiness);
            case 6:
                return GetMultiplierFromStat(Stat.Reflex);
            default:
                Debug.LogError("Tried to get nature multiplier for HP or an index higher than 7!!");
                return -1;
        }*/
    }

    /*private float GetMultiplierFromStat(Stat stat)
    {
        if (BoostedStat == stat) return 1.1f;
        if (ReducedStat == stat) return 0.9f;
        return 1f;
    }*/
}
