using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Hazard Removal (Both Sides) Effect", menuName = "Attack Effect/Remove Hazards (Both Sides)")]


public class RemoveAllHazardsEffect : RemoveSelfSideHazardsEffect
{
    //to remove hazards on all sides of the field, simply use the effect that removes hazards from one side, but on both sides!
    public override void ExecuteSecondaryEffect(MonsterUnit thisMonsterUnit, MonsterUnit target, int damageDealt, bool moveMissed)
    {
        base.ExecuteSecondaryEffect(thisMonsterUnit, target, damageDealt, moveMissed);
        base.ExecuteSecondaryEffect(target, thisMonsterUnit, damageDealt, moveMissed);
    }

}
