using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Self Heal Effect", menuName = "Attack Effect/Self Heal")]
public class SelfHealEffect : AttackEffect
{
    [SerializeField] private int healPercent = 50;
    
    public override void ExecuteSecondaryEffect(MonsterUnit thisMonsterUnit, MonsterUnit target, int damageDealt)
    {
        if (thisMonsterUnit.Heal(healPercent))
        {
            Battle.StaticMessage(Battle.GetCurrentMessage() + "\nIt restored HP!");
        }
        else
        {
            Battle.StaticMessage(Battle.GetCurrentMessage() + "\n...but its health was already full!");
        }
    }
}