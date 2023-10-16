using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ModifyBE", menuName = "Item/ModifyBE", order = 1)]
public class J_ModifyBulletEnemy : ItemData
{
    [SerializeField] BulletInfo info;
    public override void OnBulletEnemySpawn(BulletEnemy be)
    {
        be.ModifyStats(info);
    }
}
