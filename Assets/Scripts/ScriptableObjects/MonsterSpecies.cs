using UnityEngine;

//a monster species is not an individual, it is a species.
[CreateAssetMenu(fileName = "New Monster Species", menuName = "Monster Species")]
public class MonsterSpecies : ScriptableObject
{
    [SerializeField] private string speciesName;

    [SerializeField] private ElementalType[] typing;
    [field: SerializeField] public BaseStats BaseStats { get; private set; }
    
    //source for all pokemon placeholder sprites: https://www.serebii.net/pokedex-sv/ 
    [field: SerializeField] public Sprite Sprite { get; private set; }

    [field: SerializeField] public Attack[] LearnSet { get; private set; }

    /*getter methods because trying to turn the formerly private serialized
     fields into auto-properties was going to break their serialized data*/
    public string GetName()
    {
        return speciesName;
    }

    public ElementalType[] GetTyping()
    {
        return typing;
    }

    public float GetTypeMultiplier(ElementalType otherType)
    {
        float firstMultiplier = GetSingleTypeMultiplier(typing[0], otherType);
        if (typing.Length == 1)
        {
            return firstMultiplier;
        }

        float secondMultiplier = GetSingleTypeMultiplier(typing[1], otherType);
        return firstMultiplier * secondMultiplier;
        
    }

    private float GetSingleTypeMultiplier(ElementalType myType, ElementalType otherType)
    {
        TypeEffect type = TypeEffect.TypeEffectMap[myType];

        if (type.IsWeakTo(otherType))
        {
            return 2;
        }
        else if (type.Resists(otherType))
        {
            return 0.5f;
        }
        else if (type.IsImmuneTo(otherType))
        {
            return 0;
        }
        return 1;
    }

}


