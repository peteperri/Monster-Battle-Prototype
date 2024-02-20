using System;
using UnityEngine;

//TODO: figure out if this should be an abstract class, interface, etc.
//Does it need to be serialized? Don't know yet.
[Serializable]
public abstract class AttackEffect : ScriptableObject
{
    public virtual void ExecuteSecondaryEffect(MonsterUnit thisMonsterUnit, MonsterUnit target)
    {
        Debug.Log("This attack has no secondary effect.");
    }
}
