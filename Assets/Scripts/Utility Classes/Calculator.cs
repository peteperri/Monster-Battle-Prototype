using UnityEngine;

public static class Calculator
{
    public static int[] CalcAllStats(BaseStats baseStats, EffortValues evs, Nature nature, int iv = 31, int level = 100)
    {
        int[] calculatedStats = new int[7];
        int[] allBaseStats = baseStats.GetAllStats();
        int[] allEVs = evs.GetAllEVs();
        
        NatureEffect natureEffect = NatureHelper.NatureEffectsMap[nature];
        
        for (int i = 0; i < 7; i++)
        {
            if (i == 0)
            {
                calculatedStats[i] = CalcHealthStat(baseStats, evs);
            }
            else
            {
                float natureMultiplier = natureEffect.GetMultiplier(i);
                calculatedStats[i] = CalcOtherStat(allBaseStats[i], allEVs[i], natureMultiplier);
            }
        }
        return calculatedStats;
    }
    
    //formula source: "Generation III onward" https://bulbapedia.bulbagarden.net/wiki/Stat#Base_stat_values 
    //tested and verified against results from https://play.pokemonshowdown.com/teambuilder 
    private static int CalcHealthStat(BaseStats baseStat, EffortValues evs, int iv = 31, int level = 100)
    {
        return (((2 * baseStat.Health + iv + (evs.healthValues / 4)) * level) / 100) + level + 10;
    }

    //formula source: "Generation III onward" https://bulbapedia.bulbagarden.net/wiki/Stat#Base_stat_values 
    //tested and verified against results from https://play.pokemonshowdown.com/teambuilder 
    private static int CalcOtherStat(float baseStat, float ev, float natureMultiplier, float iv = 31, float level = 100)
    {
        float rawStat = (((2 * baseStat + iv + (ev / 4)) * level) / 100) + 5;
        return (int) (rawStat * natureMultiplier);
    }

    //formula source: "Generation V onward" https://bulbapedia.bulbagarden.net/wiki/Damage
    //tested and verified against results from https://calc.pokemonshowdown.com/ 
    public static int CalculateDamage(float myAttackingStat, float opponentsDefendingStat, float movePower, float multipliers = 1, int level = 100)
    {
        float levelFactor = ((2 * level) / 5 + 2);
        float statFactor = movePower * (myAttackingStat / opponentsDefendingStat);
        float rawDamage = (levelFactor * statFactor) / 50 + 2;
        
        /*according to bulbapedia, "all divisions and multiplications past the initial
         base damage calculation are rounded to the nearest integer if the result is not an integer 
         (rounding down at 0.5).
         in practice this means round damage BEFORE applying multipliers */
        int damageRounded = Mathf.RoundToInt(rawDamage);
        
        //round again because we have to :)
        return Mathf.RoundToInt(damageRounded * multipliers);
    }
}
