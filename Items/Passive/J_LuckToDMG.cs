using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LuckDMG", menuName = "Item/LuckDMG", order = 1)]
public class J_LuckToDMG : ItemData
{
    public override void AfterTime()
    {
        AddPlayerStats.Damage = Player.Instance.CurStats.Luck * 0.1f;
        AddPlayerStats.BulletSize = Player.Instance.CurStats.Luck * 0.1f;
    }
}
