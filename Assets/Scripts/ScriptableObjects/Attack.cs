using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Attack", menuName = "Attack")]
public class Attack : ScriptableObject
{
    [field: SerializeField] public string AttackName { get; private set; }
    [field: SerializeField] public int BasePower { get; private set; }
    [field: SerializeField] public int Accuracy { get; private set; }
    [field: SerializeField] public int PowerPoints { get; private set; }
    [field: SerializeField] public int Priority { get; private set; }
    [field: SerializeField] public ElementalType Type { get; private set; }
    [field: SerializeField] public AttackCategory Category { get; private set; }
    [field: SerializeField] public bool MakesContact { get; private set; }
    [field: SerializeField] public AttackEffect[] SecondaryEffect { get; private set; }
    
}
