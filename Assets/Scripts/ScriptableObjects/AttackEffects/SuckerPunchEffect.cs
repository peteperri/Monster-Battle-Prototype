using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "New Sucker Punch Effect", menuName = "Attack Effect/Sucker Punch")]
public class SuckerPunchEffect : AttackEffect
{
    //this is completely empty because sucker punch doesn't have a true secondary effect that would execute
    //AFTER damage, rather it has a unique precondition that is checked for in the Battle script.
}