using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Stat Modifier Attack Effect", menuName = "Stat Modifier Attack Effect")]
public class StatModifierEffect : AttackEffect
{
    [SerializeField] private Stat statToEffect;
    [SerializeField] private int stagesToAddOrRemove;

    public override void ExecuteSecondaryEffect(MonsterUnit unit)
    {
        if (statToEffect == Stat.Health) return;
        unit.ApplyStatModifier(statToEffect, stagesToAddOrRemove);
    }
}
