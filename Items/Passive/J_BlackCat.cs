using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Cat", menuName = "Item/Cat", order = 1)]
public class J_BlackCat : ItemData
{
    public override void OnRecycleItem()
    {
        A_Factory.Instance.MakeItem(0, Player.Instance.transform.position + new Vector3(-0.25f, -0.7f), ItemPool.BossBonus);
        A_Factory.Instance.MakeItem(0, Player.Instance.transform.position + new Vector3(0.25f, -0.7f), ItemPool.BossBonus);
    }
}
