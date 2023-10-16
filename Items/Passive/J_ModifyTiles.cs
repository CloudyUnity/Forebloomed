using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ModifyTile", menuName = "Item/ModifyTile", order = 1)]
public class J_ModifyTiles : ItemData
{
    [SerializeField] float HitPointReduction;
    public override void OnTileSpawn(Tile tile)
    {
        if (tile is TileWall wall) wall.HitPoints -= HitPointReduction;
    }
}
