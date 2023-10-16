using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Virus", menuName = "Item/Virus", order = 1)]
public class J_Virus : ItemData
{
    [SerializeField] float HealthReduce;
    public override void OnEnemySpawn(Entity entity)
    {
        entity.HitPoints *= HealthReduce;
    }
}
