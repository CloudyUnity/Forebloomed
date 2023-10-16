using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Keys", menuName = "Item/Keys", order = 1)]
public class J_KeyMaster : ItemData
{
    public override void OnPickup()
    {
        Player.Instance.GainKey();

        A_EventManager.InvokePlaySFX("Key");

        if (Player.Instance.SoftStats.Keys + Player.Instance.SoftStats.GoldKeys >= 10)
            A_EventManager.InvokeUnlock("Keymaster");
    }

    public override void OnChestSpawn(O_Chest chest)
    {
        chest.CanUseKey = true;
    }

    public override void OnPricedItemSpawn(ItemPriced priced)
    {
        priced.CanUseKey = true;
    }
}
