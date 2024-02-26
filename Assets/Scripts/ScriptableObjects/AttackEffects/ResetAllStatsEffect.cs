
using UnityEngine;

[CreateAssetMenu(fileName = "New Reset All Stats", menuName = "Attack Effect/Reset All Stats")]
public class ResetAllStatsEffect : AttackEffect
{
    public override void ExecuteSecondaryEffect(MonsterUnit thisMonsterUnit, MonsterUnit target, int damageDealt, bool moveMissed)
    {
        MonsterUnit[] allMonsters = Battle.GetAllMonstersBattling();

        foreach (MonsterUnit monster in allMonsters)
        {
            monster.ResetStatModifiers();
        }

        Battle.StaticMessage(Battle.GetCurrentMessage() + $"\nAll stat changes were reset!");
    }
}
