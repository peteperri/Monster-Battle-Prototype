using System;
using UnityEngine;

[Serializable]
public abstract class AttackEffect : ScriptableObject
{
    public virtual void ExecuteSecondaryEffect(MonsterUnit thisMonsterUnit, MonsterUnit target, int damageDealt)
    {
        Debug.Log("This attack has no secondary effect.");
    }
}

