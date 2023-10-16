using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Monk", menuName = "Item/Monk", order = 1)]
public class J_Monk : ItemData
{
    public override void OnCollectGold(int _)
    {
        A_EventManager.InvokeDealDamage(1, Player.Instance.transform.position, "Curse of Greed");
    }
}
