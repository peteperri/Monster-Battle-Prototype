using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Recoil Damage", menuName = "Attack Effect/Recoil Damage")]
public class RecoilDamageEffect : AttackEffect
{
    [SerializeField] private int recoilPercentage = 33;

    public override void ExecuteSecondaryEffect(MonsterUnit thisMonsterUnit, MonsterUnit target, int damageDealt)
    {
        float recoilFactor = recoilPercentage / 100f;
        float recoilDamage = damageDealt * recoilFactor;
        Debug.Log($"Damage dealt: {damageDealt}");
        Debug.Log($"Recoil to take: {recoilDamage}");
        Battle.StaticMessage(Battle.GetCurrentMessage() + $"\n{thisMonsterUnit.UnitName} was damaged by the recoil!");
        thisMonsterUnit.TakeDamage(Mathf.RoundToInt(recoilDamage));
    }
}