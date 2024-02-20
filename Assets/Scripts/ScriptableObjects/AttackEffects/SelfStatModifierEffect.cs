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
    }
}
