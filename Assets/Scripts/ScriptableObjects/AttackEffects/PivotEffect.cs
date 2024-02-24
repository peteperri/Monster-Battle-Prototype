using UnityEngine;

[CreateAssetMenu(fileName = "New Pivot", menuName = "Attack Effect/Pivot")]
public class PivotEffect : AttackEffect
{
    
    public override void ExecuteSecondaryEffect(MonsterUnit thisMonsterUnit, MonsterUnit target)
    {
        Debug.Log("Pivot Secondary Effect!");
        
        //using the monster's position in battle, get its player. 
        Trainer myTrainer = thisMonsterUnit.PositionInBattle.Player;

        //is my trainer player 1 or player 2?
        int playerNum = myTrainer.PlayerNum;

        //there is only one battle... so we can use FindObjectOfType
        Battle battle = FindObjectOfType<Battle>();

        battle.Pivot(playerNum);

    }
}
