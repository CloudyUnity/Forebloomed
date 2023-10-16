using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Digger", menuName = "Item/Digger", order = 1)]
public class J_Digger : ItemData
{
    [SerializeField] float _size;
    [SerializeField] LayerMask _wallTileLayer;
    public override void AfterTime()
    {
        var hits = Physics2D.OverlapCircleAll(Player.Instance.transform.position, _size, _wallTileLayer);
        for (int i = hits.Length - 1; i >= 0; i--)
        {
            Collider2D hit = hits[i];
            var wall = hit.GetComponent<TileWall>();
            wall.TakeDamage(0.1f);
        }
    }
}
