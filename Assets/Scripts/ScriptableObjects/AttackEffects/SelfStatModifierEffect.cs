using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Self Stat Modifier", menuName = "Attack Effect/Self Stat Modifier")]
public class SelfStatModifierEffect : AttackEffect
{
    [SerializeField] private Stat statToEffect;
    [SerializeField] private int stagesToAddOrRemove;

    public override void ExecuteSecondaryEffect(MonsterUnit thisMonsterUnit, MonsterUnit target, int damageDealt)
    {
        if (statToEffect == Stat.Health) return;
        
        int statBeforeDrop = thisMonsterUnit.GetStat(statToEffect);
        thisMonsterUnit.ApplyStatModifier(statToEffect, stagesToAddOrRemove);

        if (stagesToAddOrRemove > 0)
        {
            Battle.StaticMessage(Battle.GetCurrentMessage() + $"\n{thisMonsterUnit.UnitName} raised its {statToEffect} from {statBeforeDrop} to {thisMonsterUnit.GetStat(statToEffect)}!");
        }
        else if (stagesToAddOrRemove < 0)
        {
            Battle.StaticMessage(Battle.GetCurrentMessage() + $"\n{thisMonsterUnit.UnitName}'s {statToEffect} dropped from {statBeforeDrop} to {thisMonsterUnit.GetStat(statToEffect)}!");
        }
    }
}
