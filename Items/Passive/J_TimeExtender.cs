using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Time", menuName = "Item/Time", order = 1)]
public class J_TimeExtender : ItemData
{
    [SerializeField] float _increaseAmount;
    public override void OnPickup()
    {
        if (A_TimeManager.Instance == null)
            return;

        A_TimeManager.Instance.IncreaseTime(_increaseAmount);
    }

    public override void OnLoadItem()
    {
        if (A_TimeManager.Instance == null)
            return;

        A_TimeManager.Instance.IncreaseTime(_increaseAmount);
    }
}
