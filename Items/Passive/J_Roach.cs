using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Roach", menuName = "Item/Roach", order = 1)]
public class J_Roach : ItemData
{
    public override void OnPickup()
    {
        Player.Instance.SoftStats.BonusHealth = 0;
        Player.Instance.SoftStats.MaxHealth = 1;
        Player.Instance.SoftStats.CurHealth = 1;

        A_EventManager.InvokeUnlock("Funky Fresh");
    }
}
