using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Self Stat Modifier", menuName = "Attack Effect/Self Stat Modifier")]
public class SelfStatModifierEffect : AttackEffect
{
    [SerializeField] private Stat statToEffect;
    [SerializeField] private int stagesToAddOrRemove;

    public override void ExecuteSecondaryEffect(MonsterUnit thisMonsterUnit, MonsterUnit target)
    {
        if (statToEffect == Stat.Health) return;
        thisMonsterUnit.ApplyStatModifier(statToEffect, stagesToAddOrRemove);

        if (stagesToAddOrRemove > 0)
        {
            Battle.StaticMessage(Battle.GetCurrentMessage() + $"\nIt raised its {statToEffect} to {thisMonsterUnit.GetStat(statToEffect)}!");
        }
        else if (stagesToAddOrRemove < 0)
        {
            Battle.StaticMessage(Battle.GetCurrentMessage() + $"\nIts {statToEffect} dropped to {thisMonsterUnit.GetStat(statToEffect)}!");
        }
    }
}
