using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

public class TypeEffect
{

    private readonly ElementalType[] _weaknesses;
    private readonly ElementalType[] _resistances;
    private readonly ElementalType[] _immunities;

    public TypeEffect(ElementalType[] weaknesses, ElementalType[] resistances, ElementalType[] immunities)
    {
        _weaknesses = weaknesses;
        _resistances = resistances;
        _immunities = immunities;
    }

    public bool IsWeakTo(ElementalType other)
    {
        return _weaknesses.Contains(other);
    }

    public bool Resists(ElementalType other)
    {
        return _resistances.Contains(other);
    }

    public bool IsImmuneTo(ElementalType other)
    {
        return _immunities.Contains(other);
    }


    //TODO: There has GOT to be a better way to do this.
    public static readonly ReadOnlyDictionary<ElementalType, TypeEffect> TypeEffectMap =
        new ReadOnlyDictionary<ElementalType, TypeEffect>(new Dictionary<ElementalType, TypeEffect>
        {
            { 
                ElementalType.Normal, new TypeEffect(
                    new ElementalType[]{ElementalType.Fighting}, 
                    new ElementalType[]{}, 
                    new ElementalType[]{ElementalType.Ghost}) 
            },

            
            {
                ElementalType.Fire, new TypeEffect( 
                    new ElementalType[]{ElementalType.Water, ElementalType.Ground, ElementalType.Rock}, 
                    new ElementalType[]{ElementalType.Fire, ElementalType.Grass, ElementalType.Ice, ElementalType.Bug, ElementalType.Steel, ElementalType.Fairy}, 
                    new ElementalType[]{})
            }, 
            
            {
                ElementalType.Water, new TypeEffect( 
                    new ElementalType[]{ElementalType.Grass, ElementalType.Electric}, 
                    new ElementalType[] { ElementalType.Fire, ElementalType.Water, ElementalType.Ice, ElementalType.Steel}, 
                    new ElementalType[]{})
            }, 
            
            {
                ElementalType.Grass, new TypeEffect( 
                    new ElementalType[]{ElementalType.Fire, ElementalType.Ice, ElementalType.Poison, ElementalType.Flying, ElementalType.Bug}, 
                    new ElementalType[]{ElementalType.Water, ElementalType.Electric, ElementalType.Grass, ElementalType.Ground}, 
                    new ElementalType[]{})
            }, 
            
            {
                ElementalType.Flying, new TypeEffect( 
                    new ElementalType[]{ElementalType.Electric, ElementalType.Ice, ElementalType.Rock}, 
                    new ElementalType[] { ElementalType.Grass, ElementalType.Fighting, ElementalType.Bug}, 
                    new ElementalType[]{ElementalType.Ground})
            }, 
            
            {
                ElementalType.Fighting, new TypeEffect( 
                    new ElementalType[]{ElementalType.Flying, ElementalType.Psychic, ElementalType.Fairy}, 
                    new ElementalType[]{ElementalType.Bug, ElementalType.Rock, ElementalType.Dark}, 
                    new ElementalType[]{})
            }, 
            
            {
                ElementalType.Poison, new TypeEffect( 
                    new ElementalType[]{ElementalType.Ground, ElementalType.Psychic}, 
                    new ElementalType[]{ElementalType.Grass, ElementalType.Fighting, ElementalType.Poison, ElementalType.Bug, ElementalType.Fairy}, 
                    new ElementalType[]{})
            }, 
            
            {
                ElementalType.Electric, new TypeEffect( 
                    new ElementalType[]{ElementalType.Ground}, 
                    new ElementalType[]{ElementalType.Electric, ElementalType.Flying, ElementalType.Steel}, 
                    new ElementalType[]{})
            }, 
            
            {
                ElementalType.Ground, new TypeEffect( 
                    new ElementalType[]{ElementalType.Water, ElementalType.Grass, ElementalType.Ice}, 
                    new ElementalType[]{ElementalType.Poison, ElementalType.Rock}, 
                    new ElementalType[]{ElementalType.Electric})
            }, 

            {
                ElementalType.Rock, new TypeEffect( 
                    new ElementalType[]{ElementalType.Water, ElementalType.Grass, ElementalType.Fighting, ElementalType.Ground, ElementalType.Steel}, 
                    new ElementalType[]{ElementalType.Normal, ElementalType.Fire, ElementalType.Poison, ElementalType.Flying}, 
                    new ElementalType[]{})
            }, 

            {
                ElementalType.Psychic, new TypeEffect( 
                    new ElementalType[]{ElementalType.Bug, ElementalType.Ghost, ElementalType.Dark}, 
                    new ElementalType[]{ElementalType.Fighting, ElementalType.Psychic}, 
                    new ElementalType[]{})
            }, 

            {
                ElementalType.Ice, new TypeEffect( 
                    new ElementalType[]{ElementalType.Fire, ElementalType.Fighting, ElementalType.Rock, ElementalType.Steel}, 
                    new ElementalType[]{ElementalType.Ice}, 
                    new ElementalType[]{})
            }, 

            {
                ElementalType.Bug, new TypeEffect( 
                    new ElementalType[]{ElementalType.Fire, ElementalType.Flying, ElementalType.Rock}, 
                    new ElementalType[]{ElementalType.Grass, ElementalType.Fighting, ElementalType.Ground}, 
                    new ElementalType[]{})
            }, 

            {
                ElementalType.Ghost, new TypeEffect( 
                    new ElementalType[]{ElementalType.Ghost, ElementalType.Dark}, 
                    new ElementalType[]{ElementalType.Poison, ElementalType.Bug}, 
                    new ElementalType[]{ElementalType.Normal, ElementalType.Fighting})
            }, 

            {
                ElementalType.Steel, new TypeEffect( 
                    new ElementalType[]{ElementalType.Fire, ElementalType.Fighting, ElementalType.Ground}, 
                    new ElementalType[]{ElementalType.Normal, ElementalType.Grass, ElementalType.Ice, ElementalType.Flying, ElementalType.Psychic, ElementalType.Bug, ElementalType.Rock, ElementalType.Dragon, ElementalType.Steel, ElementalType.Fairy}, 
                    new ElementalType[]{ElementalType.Poison})
            }, 

            {
                ElementalType.Dragon, new TypeEffect( 
                    new ElementalType[]{ElementalType.Ice, ElementalType.Dragon, ElementalType.Fairy}, 
                    new ElementalType[]{ElementalType.Fire, ElementalType.Water, ElementalType.Grass, ElementalType.Electric}, 
                    new ElementalType[]{})
            }, 

            {
                ElementalType.Dark, new TypeEffect( 
                    new ElementalType[]{ElementalType.Fighting, ElementalType.Bug, ElementalType.Fairy}, 
                    new ElementalType[]{ElementalType.Ghost, ElementalType.Dark}, 
                    new ElementalType[]{ElementalType.Psychic})
            }, 

            {
                ElementalType.Fairy, new TypeEffect( 
                    new ElementalType[]{ElementalType.Poison, ElementalType.Steel}, 
                    new ElementalType[]{ElementalType.Fighting, ElementalType.Bug, ElementalType.Dark}, 
                    new ElementalType[]{ElementalType.Dragon})
            }
        });
}