using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Hazard Removal (Self Side) Effect", menuName = "Attack Effect/Remove Hazards (Self Side)")]

public class RemoveSelfSideHazardsEffect : AttackEffect
{
    public override void ExecuteSecondaryEffect(MonsterUnit thisMonsterUnit, MonsterUnit target, int damageDealt, bool moveMissed)
    {
        Battle.StaticMessage(Battle.GetCurrentMessage() + $"\nAll hazards were removed from Player {thisMonsterUnit.PositionInBattle.Player.PlayerNum}'s side of the field!");
        thisMonsterUnit.PositionInBattle.ClearHazards();
    }
}
