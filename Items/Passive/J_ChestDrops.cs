using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ChestDrops", menuName = "Item/ChestDrops", order = 1)]
public class J_ChestDrops : ItemData
{
    [SerializeField] List<MakeInfo> _infos = new List<MakeInfo>();
    public override void OnChestOpen(O_Chest chest)
    {
        A_Factory.Instance.MakeBasic(chest.transform.position, _infos.RandomItem());
    }
}
