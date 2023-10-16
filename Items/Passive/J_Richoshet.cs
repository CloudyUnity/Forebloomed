using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Richoshet", menuName = "Item/Richoshet", order = 1)]
public class J_Richoshet : ItemData
{
    [SerializeField] float _range;
    [SerializeField] LayerMask _enemyMask;

    public override void OnPickup()
    {
        if (Player.Instance.CurStats.Piercing == 0)
            Player.Instance.DefaultStats.Piercing = 1;
    }

    public override void OnLoadItem()
    {
        if (Player.Instance.CurStats.Piercing == 0)
            Player.Instance.DefaultStats.Piercing = 1;
    }

    public override void OnPlayerBulletCollide(BulletPlayer bullet, GameObject go)
    {
        if (go.tag != "Enemy")
            return;

        var hits = Physics2D.OverlapCircleAll(go.transform.position, _range, _enemyMask);

        foreach (var hit in hits)
        {
            if (hit.gameObject == go)
                continue;

            Vector2 dir = (hit.transform.position - bullet.transform.position).normalized;
            bullet.ChangeDirection(dir);
            break;
        }
    }
}
