using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Opponent Stat Modifier", menuName = "Attack Effect/Opponent Stat Modifier")]
public class OpponentStatModifierEffect : AttackEffect
{
    [SerializeField] private Stat statToEffect;
    [SerializeField] private int stagesToAddOrRemove;
    [SerializeField] private int percentageChance;
    
    public override void ExecuteSecondaryEffect(MonsterUnit thisMonsterUnit, MonsterUnit target)
    {
        if (statToEffect == Stat.Health) return;

        int rand = Random.Range(1, 100);
        if (rand <= percentageChance)
        {
            target.ApplyStatModifier(statToEffect, stagesToAddOrRemove);
        }
    }
}