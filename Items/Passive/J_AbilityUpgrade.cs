using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AbilityUpgrade", menuName = "Item/AbilityUpgrade", order = 1)]
public class J_AbilityUpgrade : ItemData
{
    public override void OnLoadItem()
    {
        Player.Instance.AbilityUpgrades++;
    }

    public override void OnPickup()
    {
        Player.Instance.AbilityUpgrades++;
    }
}
