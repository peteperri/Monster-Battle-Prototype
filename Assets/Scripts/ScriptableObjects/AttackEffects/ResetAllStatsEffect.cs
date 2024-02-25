
using UnityEngine;

[CreateAssetMenu(fileName = "New Reset All Stats", menuName = "Attack Effect/Reset All Stats")]
public class ResetAllStatsEffect : AttackEffect
{
    public override void ExecuteSecondaryEffect(MonsterUnit thisMonsterUnit, MonsterUnit target, int damageDealt)
    {
        thisMonsterUnit.ResetStatModifiers();
        target.ResetStatModifiers();
        Battle.StaticMessage(Battle.GetCurrentMessage() + $"\nAll stat changes were reset!");
    }
}
