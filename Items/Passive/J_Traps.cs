using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Item/traps", order = 1)]
public class J_Traps : ItemData
{
    public override void OnPlayerBulletMiss(BulletPlayer bullet)
    {
        if (bullet.Missed)
            return;

        bullet.NotDissolveUntilRange = true;
        bullet.Range += 5;
        bullet.Speed = 0.1f;
        bullet.ChangeDirection(Vector2.zero);

        if (bullet.NextInList != null)
            bullet.NextInList.Deactivate();
        bullet.NextInList = null;
    }
}
