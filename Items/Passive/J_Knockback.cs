using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Item/KnockBack", order = 1)]
public class J_Knockback : ItemData
{
    [SerializeField] float _power;
    public override void OnPlayerBulletCollide(BulletPlayer bullet, GameObject go)
    {
        if (go.TryGetComponent(out Entity e))
        {
            Vector2 dir = (e.transform.position - bullet.transform.position).normalized;
            e.KnockBack(dir * _power);
        }
    }
}
