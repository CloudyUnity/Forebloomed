using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Dermal", menuName = "Item/Dermal", order = 1)]
public class J_DermalEmblem : ItemData
{
    public override void OnPickup()
    {
        Player.Instance.FreeHits++;
    }

    public override void OnLoadItem()
    {
        Player.Instance.FreeHits++;
    }
}
