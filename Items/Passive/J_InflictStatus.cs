using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ConstEffect", menuName = "Item/ConstEffect", order = 1)]
public class J_InflictStatus : ItemData
{
    [SerializeField] List<StatusEffect> effect;
    public override void OnEnemySpawn(Entity entity)
    {
        entity.InflictStatus(effect);
    }
}
