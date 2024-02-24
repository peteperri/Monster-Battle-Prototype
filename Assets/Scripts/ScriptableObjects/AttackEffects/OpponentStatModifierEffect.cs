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
            //if the stat drop doesn't ALWAYS happen, say "How unlucky!"
            string luckMessage = percentageChance == 100 ? "" : " How unlucky!";

            target.ApplyStatModifier(statToEffect, stagesToAddOrRemove);
            Battle.StaticMessage(Battle.GetCurrentMessage() + $"\n{target.UnitName}'s {statToEffect} dropped to {target.GetStat(statToEffect)}!" + luckMessage);
        }
    }
}