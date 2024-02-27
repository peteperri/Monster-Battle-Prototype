/*TODO: To reduce repeated code, refactor this out into multiple classes: one for applying a status condition, and another for
  applying any secondary effect, but only with a certain percentage chance.
 */

using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Apply Status Condition Effect", menuName = "Attack Effect/Apply Status Condition")]
public class ApplyStatusConditionEffect : AttackEffect
{
    [SerializeField] private StatusCondition statusToApply = StatusCondition.Paralyzed;
    [SerializeField] private int percentageChance;
    
    public override void ExecuteSecondaryEffect(MonsterUnit thisMonsterUnit, MonsterUnit target, int damageDealt, bool moveMissed)
    {
        if (moveMissed || target.Fainted) return;
        
        //applying "none" should only be used for healing, which should be a different effect entirely.
        //status applying moves should also fail if the target already has a status.
        //and, at the end of it all, these conditions should only matter (and show "but it failed!") if this is a guaranteed status move
        if ((statusToApply == StatusCondition.None || target.StatusCondition != StatusCondition.None) && percentageChance == 100)
        {
            Battle.StaticMessage(Battle.GetCurrentMessage() + "\nBut it failed!");
            return;
        }

        /*don't bother doing the random check if the opponent has a status condition already
        separate from the above condition because that one shows a "But it failed!" message.
        this causes a silent exit from the method, specifically for moves with a random secondary chance to apply status,
        like thunderbolt */
        if (target.StatusCondition != StatusCondition.None)
        {
            return;
        }

        int rand = Random.Range(1, 100);
        if (rand <= percentageChance)
        {
            //if the status condition doesn't ALWAYS happen, say "How unlucky!"
            string luckMessage = percentageChance == 100 ? "" : " How unlucky!";

            target.ApplyStatus(statusToApply);
            
            //TODO: This message will need to be updated for future status effects, just for grammar/flavor. For example, "fell asleep" instead of "became asleep"
            Battle.StaticMessage(Battle.GetCurrentMessage() + $"\n{target.UnitName} became {statusToApply}!" + luckMessage);
        }
    }
}
