using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FullHP", menuName = "Item/FullHP", order = 1)]
public class J_FullHP : ItemData
{
    public override void AfterTime()
    {
        if (Player.Instance.SoftStats.CurHealth == Player.Instance.SoftStats.MaxHealth)
        {
            AddPlayerStats.Damage = 0.5f;
            return;
        }
        AddPlayerStats.Damage = 0;
    }
}
