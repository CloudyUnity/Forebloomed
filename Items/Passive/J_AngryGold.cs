using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AngryGold", menuName = "Item/AngryGold", order = 1)]
public class J_AngryGold : ItemData
{
    [SerializeField] BulletInfo info;

    public override void OnGoldSpawn(Vector2 pos)
    {
        A_Factory.Instance.MakeEnemyBullet(pos, info);
    }
}
