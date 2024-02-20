using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Self Heal Effect", menuName = "Attack Effect/Self Heal")]
public class SelfHealEffect : AttackEffect
{
    [SerializeField] private int healPercent = 50;
    
    public override void ExecuteSecondaryEffect(MonsterUnit thisMonsterUnit, MonsterUnit target)
    {
        //TODO: WRITE THIS METHOD  
    }
}