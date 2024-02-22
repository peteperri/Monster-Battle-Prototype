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
    }
}
