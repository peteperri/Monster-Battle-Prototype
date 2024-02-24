using System;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

//a monster unit is an individual, customized monster. only customizable/changeable traits are controlled by this class
[Serializable]
public class MonsterUnit
{
    private MonsterSpecies _species;
    private EffortValues _statInvestments;
    private Nature _nature;

    //by default, a monster unit's name will just be it's species name. however, nicknames are a feature that i want to support once customization is implemented
    public string UnitName { get; }
    public Attack[] KnownAttacks { get; }
    
    public bool Fainted { get; private set; }

    public BattlePosition PositionInBattle { get; set; }

    //just using a SerializedDictionary without actually serializing it so I can easily view it in the Debug inspector
    private SerializedDictionary<Stat, int> _statsBeforeModifiers;
    private SerializedDictionary<Stat, int> _statsAfterModifiers;
    private SerializedDictionary<Stat, int> _statModifierStages;

    public MonsterUnit(MonsterSpecies species, EffortValues evs, Nature nature, string nickname)
    {
        _species = species;
        _statInvestments = evs;
        _nature = nature;
        _statsBeforeModifiers = new SerializedDictionary<Stat, int>();
        _statsAfterModifiers = new SerializedDictionary<Stat, int>();
        _statModifierStages = new SerializedDictionary<Stat, int>();
        
        KnownAttacks = new Attack[4];
        UnitName = nickname;
        Fainted = false;
        PositionInBattle = null;
        
        //Fainted = Random.Range(1, 3) == 1;
       
        
        //currently gives every mon the first four moves in their learnset; needs to be removed if customization is implemented
        for (int i = 0; i < KnownAttacks.Length && i < _species.LearnSet.Length; i++)
        {
            KnownAttacks[i] = _species.LearnSet[i];
        }

        ComputeStartingStats();
    }
    
    //chained constructors
    public MonsterUnit(MonsterSpecies species) 
        : this(species, new EffortValues(), Nature.Hardy, species.GetName()) {}
    
    public MonsterUnit(MonsterSpecies species, Nature nature) 
        : this(species, new EffortValues(), nature, species.GetName()) {}

    public MonsterUnit(MonsterSpecies species, EffortValues evs, Nature nature)
        : this(species, evs, nature, species.GetName()) {}

    public void ComputeStartingStats()
    {
        BaseStats baseStats = _species.BaseStats;
        int[] calculatedStats = Calculator.CalcAllStats(baseStats, _statInvestments, _nature);
        Array statNames = Enum.GetValues(typeof(Stat));

        for (int i = 0; i < calculatedStats.Length; i++)
        {
            Stat statName = (Stat)statNames.GetValue(i);
            _statsBeforeModifiers.Add(statName, calculatedStats[i]);
            _statsAfterModifiers.Add(statName, calculatedStats[i]);
            _statModifierStages.Add(statName, 0);
        }
    }
    
    public void ComputeModifiedStats()
    {
        foreach (Stat stat in Enum.GetValues(typeof(Stat)))
        {
            if (stat == Stat.Health) continue;
            
            int modifierStages = _statModifierStages[stat];
            int multiplierNumerator = 2;
            int multiplierDenominator = 2;

            if (modifierStages > 0)
            {
                multiplierNumerator += modifierStages;
            }
            else if (modifierStages < 0)
            {
                multiplierDenominator += Math.Abs(modifierStages); //absolute value because stages are negative but we want the denominator to increase 
            }
            
            //Debug.Log($"{multiplierNumerator}/{multiplierDenominator}");
            
            float newStat = _statsBeforeModifiers[stat] * ((float) multiplierNumerator / multiplierDenominator);
            _statsAfterModifiers[stat] = (int) newStat;
        }
    }

    public MonsterSpecies GetSpecies()
    {
        return _species;
    }

    public void ApplyStatModifier(Stat statToModify, int byHowManyStages)
    {
        int currentStatModifiers = _statModifierStages[statToModify];
        bool statCannotGoLower = byHowManyStages < 0 && currentStatModifiers <= -6;
        bool statCannotGoHigher = byHowManyStages > 0 && currentStatModifiers >= 6;
        if (statCannotGoHigher || statCannotGoLower)
        {
            return;
        }

        int newStatModifier = currentStatModifiers + byHowManyStages;
        Mathf.Clamp(newStatModifier, -6, 6);

        _statModifierStages[statToModify] = newStatModifier;
        ComputeModifiedStats();

    }

    //this method takes in an array of targets for future double-battle functionality
    //at this time, this array will only ever have a size of one, because 
    //double battles are not yet implemented.
    public void UseAttack(int attackIndex, MonsterUnit[] targets)
    {
        if (attackIndex > KnownAttacks.Length || attackIndex < 0)
        {
            Debug.Log($"{UnitName} tried to use a move outside of its range.");
            return;
        }

        if (Fainted)
        {
            Debug.Log($"{UnitName} has fainted and cannot attack!");
            return;
        }

        Attack attack = KnownAttacks[attackIndex];
        foreach (MonsterUnit target in targets)
        {
            Debug.Log($"{_species.name} used {attack.name} on {target._species.name}!");
            Battle.StaticMessage($"{_species.name} used {attack.name}!");
            int damageToDeal = 0;

            MonsterSpecies targetSpecies = target.GetSpecies();
            
            float typingMultiplier = targetSpecies.GetTypeMultiplier(attack.Type);
            if (typingMultiplier == 0)
            {
                Battle.StaticMessage($"{Battle.GetCurrentMessage()}... But the target was immune!");
                continue;
            }
            else if (typingMultiplier < 1)
            {
                Battle.StaticMessage($"{Battle.GetCurrentMessage()}... It's not very effective.");
            }
            else if (typingMultiplier > 1)
            {
                Battle.StaticMessage($"{Battle.GetCurrentMessage()}... It was super effective!");
            }

            if (attack.Category != AttackCategory.Status)
            {
                float attackStat;
                float defenseStat;
                
                if (attack.Category == AttackCategory.Physical)
                {
                    attackStat = this._statsAfterModifiers[Stat.Strength];
                    defenseStat = target._statsAfterModifiers[Stat.Defense];
                }
                else
                {
                    attackStat = this._statsAfterModifiers[Stat.Intelligence];
                    defenseStat = target._statsAfterModifiers[Stat.Resilience];
                }

                float stabMultiplier = IsStab(attack) ? 1.5f : 1;
                Debug.Log($"Stab multiplier? {stabMultiplier}");
                Debug.Log($"Typing multiplier? {typingMultiplier}");
                float allMultipliers = typingMultiplier * stabMultiplier;
                
                damageToDeal = Calculator.CalculateDamage(attackStat, defenseStat, attack.BasePower, allMultipliers);
                target.TakeDamage(damageToDeal);
            }
            
            for (int i = 0; i < attack.SecondaryEffect.Length; i++)
            {
                attack.SecondaryEffect[i].ExecuteSecondaryEffect(this, target);
            }
        }
    }

    public void TakeDamage(int amount)
    {
        int currentHealth = _statsAfterModifiers[Stat.Health];
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, _statsBeforeModifiers[Stat.Health]);
        _statsAfterModifiers[Stat.Health] = currentHealth;
        if (currentHealth <= 0)
        {
            Fainted = true;
            Battle.StaticMessage($"{Battle.GetCurrentMessage()}\n{this._species.name} has fainted!");
            Debug.Log($"{this._species.name} has fainted!");
        }

        PositionInBattle.UpdateStatus();
    }

    public void Heal(float percentage)
    {
        int maxHealth = _statsBeforeModifiers[Stat.Health];
        percentage /= 100; //turns "50%" into "0.5"
        float healAmount = maxHealth * percentage;
        
        //ensure we do not heal above max HP
        healAmount = Mathf.Clamp(healAmount, 0, maxHealth);

        int currentHealth = _statsAfterModifiers[Stat.Health];
        int newHealth = currentHealth + (int) healAmount;

        if (newHealth > maxHealth)
        {
            newHealth = maxHealth;
        }
        _statsAfterModifiers[Stat.Health] = newHealth;
        
        PositionInBattle.UpdateStatus();
    }

    public int GetMaxHealth()
    {
        return _statsBeforeModifiers[Stat.Health];
    }

    public int GetStat(Stat stat)
    {
        return _statsAfterModifiers[stat];
    }

    private bool IsStab(Attack attack)
    { 
        ElementalType[] myTyping = _species.GetTyping();
        ElementalType attackType = attack.Type;
        return attackType == myTyping[0] || attackType == myTyping[1];
    }
}




