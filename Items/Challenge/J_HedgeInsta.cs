using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InstaHedge", menuName = "Item/Chall/InstaHedge", order = 1)]
public class J_HedgeInsta : ItemData
{
    public override void OnPlayerBulletCollide(BulletPlayer bullet, GameObject go)
    {
        if (go.tag == "Tile")
        {
            go.GetComponent<TileWall>().TakeDamage(8);
        }
    }
}
