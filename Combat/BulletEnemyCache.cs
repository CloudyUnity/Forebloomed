using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletEnemyCache : MonoBehaviour
{
    public static BulletEnemy LLStart;
    public static BulletEnemy LLEnd;

    public static void Push(BulletEnemy bullet)
    {
        if (LLEnd == null)
        {
            LLEnd = bullet;
            LLStart = bullet;
            return;
        }

        LLEnd.NextInList = bullet;
        LLEnd = bullet;
    }

    public static BulletEnemy Pop()
    {
        BulletEnemy start = LLStart;
        LLStart = LLStart.NextInList;
        if (LLEnd == start)
            LLEnd = null;
        return start;
    }
}
