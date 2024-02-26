using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Stealth Rock Effect", menuName = "Attack Effect/Set Stealth Rocks")]
public class SetStealthRocksEffect : AttackEffect
{
    public override void ExecuteSecondaryEffect(MonsterUnit thisMonsterUnit, MonsterUnit target, int damageDealt, bool moveMissed)
    {
        if (target.PositionInBattle.HasStealthRocks)
        {
            Battle.StaticMessage(Battle.GetCurrentMessage() + $"\n...But pointed stones were already on {target.PositionInBattle.Player.PlayerNum}'s side of the field!");
            return;
        }

        Battle.StaticMessage(Battle.GetCurrentMessage() + $"\nPointed stones surrounded Player {target.PositionInBattle.Player.PlayerNum}'s side of the field!");
        target.PositionInBattle.SetStealthRock();
    }
}
