using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FireIce", menuName = "Item/FireIce", order = 1)]
public class J_FireAndIce : ItemData
{
    [SerializeField] StatusEffect _ice;
    [SerializeField] StatusEffect _fire;
    public override void AfterTime()
    {
        if (A_TimeManager.Instance == null)
            return;

        if (A_TimeManager.Instance.TimePercent > 1)
        {
            AddPlayerStats.Damage = 0;
            AddPlayerStats.BulletSize = 0;
            AddPlayerStats.FireRate = 0;
            return;
        }

        bool night = A_TimeManager.Instance.TimePercent >= 0.5f;
        //StatusEffects[0] = night ? _ice : _fire;
        AddPlayerStats.Damage = night ? -0.05f : 0.15f;
        AddPlayerStats.BulletSize = AddPlayerStats.Damage;
        AddPlayerStats.FireRate = night ? -3 : 1.5f;
    }
}
