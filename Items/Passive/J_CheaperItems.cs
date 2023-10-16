using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Cheaper", menuName = "Item/Cheaper", order = 1)]
public class J_CheaperItems : ItemData
{
    [SerializeField] float _costReduction;
    [SerializeField] float _chance;

    public override void OnPricedItemSpawn(ItemPriced priced)
    {
        if (priced.ShopData.Cost <= 10 || FailChance(priced.ShopData.Cost))
            return;

        int reduction = (int)(priced.ShopData.Cost * _costReduction);
        reduction = Mathf.Clamp(reduction, 10, 999999);
        priced.UpdateCost(reduction, true);
    }

    public override void OnChestSpawn(O_Chest chest)
    {
        if (chest.Cost <= 10 || FailChance(chest.Cost))
            return;

        int reduction = (int)(chest.Cost * _costReduction);
        reduction = Mathf.Clamp(reduction, 10, 999999);
        chest.UpdateCost(reduction, true);
    }

    public override void OnPricedHeartSpawn(O_PricedHeart pheart)
    {
        if (pheart.Cost <= 5 || FailChance(pheart.Cost))
            return;

        int reduction = (int)(pheart.Cost * _costReduction);
        reduction = Mathf.Clamp(reduction, 5, 999999);
        pheart.UpdateCost(reduction, true);
    }

    public bool FailChance(float cost) => A_Extensions.RandomBetween(0, 100f, A_LevelManager.QuickSeed * cost) > _chance + Player.Instance.CurStats.Luck;
}
