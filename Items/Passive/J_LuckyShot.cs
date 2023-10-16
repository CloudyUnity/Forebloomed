using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Crit", menuName = "Item/Crit", order = 1)]
public class J_LuckyShot : ItemData
{
    [SerializeField] float _baseChance;

    public override void OnBulletSpawn(BulletPlayer bullet)
    {
        if (Random.Range(0, 100f) <= _baseChance + Player.Instance.CurStats.Luck)
        {
            bullet.IncreaseSpeed(bullet.Speed);
            bullet._dmg *= 1.4f;
        }
    }

    public override void OnPlayerSliceSpawn(B_Slash bullet)
    {
        if (Random.Range(0, 100f) <= _baseChance + Player.Instance.CurStats.Luck)
        {
            bullet.Dmg *= 1.4f;
        }
    }

    public override void OnPlayerBurSpawn(B_Knife bullet)
    {
        if (Random.Range(0, 100f) <= _baseChance + Player.Instance.CurStats.Luck)
        {
            bullet._dmg *= 1.4f;
        }
    }

    public override void OnPlayerBoomerSpawn(Boomerang bullet)
    {
        if (Random.Range(0, 100f) <= _baseChance + Player.Instance.CurStats.Luck)
        {
            bullet._dmg *= 1.4f;
        }
    }
}
