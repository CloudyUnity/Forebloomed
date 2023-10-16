using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DestroyWeapons", menuName = "Item/Chall/DestroyWeapons", order = 1)]
public class J_DestroyWeapons : ItemData
{
    public override void OnPickup()
    {
        A_EventManager.InvokeDestroyWeapons();
    }

    public override void OnLoadItem()
    {
        A_EventManager.InvokeDestroyWeapons();
    }
}
