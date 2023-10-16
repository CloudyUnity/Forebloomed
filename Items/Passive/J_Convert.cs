using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Covert", menuName = "Item/Convert", order = 1)]
public class J_Convert : ItemData
{
    public override void OnPickup()
    {
        int hp = Player.Instance.SoftStats.MaxHealth;
        int thp = Player.Instance.SoftStats.BonusHealth;
        Player.Instance.SoftStats.BonusHealth = (int)(hp * 1.5f) + thp;
        Player.Instance.SoftStats.MaxHealth = 0;
    }
}
