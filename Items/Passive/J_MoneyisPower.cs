using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MoneyPower", menuName = "Item/MoneyPower", order = 1)]
public class J_MoneyisPower : ItemData
{
    float maxAmount;

    public override void OnPickup()
    {
        maxAmount = 100 * A_LevelManager.Instance.DifficultyModifier;
    }

    public override void OnLoadItem()
    {
        maxAmount = 100 * A_LevelManager.Instance.DifficultyModifier;
    }

    public override void AfterTime()
    {
        float percentage = Mathf.Clamp(Player.Instance.SoftStats.GoldAmount, 0, maxAmount) / maxAmount;
        AddPlayerStats.Damage = (percentage - 0.5f) * 0.15f;
    }
}
