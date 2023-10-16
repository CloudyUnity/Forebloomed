using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Random", menuName = "Item/Random", order = 1)]
public class J_Random : ItemData
{
    public override void AfterTime()
    {
        AddPlayerStats.BulletSpeed = Random.Range(-0.2f, 0.4f);
        AddPlayerStats.Damage = Random.Range(-0.2f, 0.4f);
        AddPlayerStats.BulletSize = AddPlayerStats.Damage;
        AddPlayerStats.Spread = Random.Range(-6, 3);
    }
}
