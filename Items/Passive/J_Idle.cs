using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Idle", menuName = "Item/Idle", order = 1)]
public class J_Idle : ItemData
{
    public override void AfterTime()
    {
        MultiplyPlayerStats.Spread = Player.Instance.PlayerInput == Vector2.zero ? 0.3f : 1.3f;
        MultiplyPlayerStats.AmountRange = Player.Instance.PlayerInput == Vector2.zero ? 0.5f : 1.3f;
    }
}
