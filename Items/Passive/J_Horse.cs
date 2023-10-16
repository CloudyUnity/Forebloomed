using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Horse", menuName = "Item/Horse", order = 1)]
public class J_Horse : ItemData
{
    float _timeSinceShoot;

    public override void AfterTime()
    {
        _timeSinceShoot += Time.deltaTime;

        if (Input.GetKey(A_InputManager.Instance.Key("Shoot")))
            _timeSinceShoot = 0;

        MultiplyPlayerStats.Speed = _timeSinceShoot > 3 ? 1.5f : 0;
    }
}
