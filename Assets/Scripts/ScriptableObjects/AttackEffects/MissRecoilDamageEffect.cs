using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Miss Recoil Damage Effect", menuName = "Attack Effect/Miss Recoil Damage")]
public class MissRecoilDamageEffect : AttackEffect
{
    [SerializeField] protected int recoilPercentage = 50;
    
    //if the move misses/fails for any reason, the target should take damage
    public override void ExecuteSecondaryEffect(MonsterUnit thisMonsterUnit, MonsterUnit target, int damageDealt, bool moveMissed)
    {
        float recoilFactor = recoilPercentage / 100f;
        float recoilDamage = thisMonsterUnit.GetMaxHealth() * recoilFactor;
        Battle.StaticMessage(Battle.GetCurrentMessage() + $"\n{thisMonsterUnit.UnitName} kept going, and crashed! It took recoil damage!");
        thisMonsterUnit.TakeDamage(Mathf.RoundToInt(recoilDamage));
    }
}
