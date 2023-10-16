using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Rage", menuName = "Item/Rage", order = 1)]
public class J_Rage : ItemData
{
    float _bonus;

    public override void OnLoadItem()
    {
        _bonus = 0;
    }

    public override void OnHit()
    {
        if (_bonus < 0)
            _bonus = 0;

        _bonus = Mathf.Clamp(_bonus + 0.5f, 0, 1);
    }

    public override void AfterTime()
    {
        _bonus -= Time.deltaTime * 0.1f;
        AddPlayerStats.Damage = Mathf.Clamp(_bonus, 0, 1);
        AddPlayerStats.BulletSize = Mathf.Clamp(_bonus, 0, 1);
    }
}
