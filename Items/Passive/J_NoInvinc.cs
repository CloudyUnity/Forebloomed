using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NoInv", menuName = "Item/NoInv", order = 1)]
public class J_NoInvinc : ItemData
{
    public override void OnPickup()
    {
        Player.Instance._invincibiltyFramesDur /= 2;

        A_Factory.Instance.MakeItem(0, Player.Instance.transform.position + new Vector3(0, 1), ItemPool.Chest);
        A_Factory.Instance.MakeItem(0, Player.Instance.transform.position + new Vector3(-1, -1), ItemPool.LockBox);
        A_Factory.Instance.MakeItem(0, Player.Instance.transform.position + new Vector3(1, -1), ItemPool.Shop);

        A_EventManager.InvokeUnlock("Probably fine");
    }

    public override void OnLoadItem()
    {
        Player.Instance._invincibiltyFramesDur /= 2;
    }
}
