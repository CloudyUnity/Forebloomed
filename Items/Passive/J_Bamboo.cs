using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Bamboo", menuName = "Item/Bamboo", order = 1)]
public class J_Bamboo : ItemData
{
    GameObject _lastBullet;

    public override void OnBulletSpawn(BulletPlayer bullet)
    {
        if (_lastBullet != null)
        {
            bullet.SetTarget(_lastBullet);
        }
        _lastBullet = bullet.gameObject;
    }
}
