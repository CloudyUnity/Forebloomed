using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_SuperBeetle : Entity
{
    float _startHealth;
    bool infinite = false;

    public override void Start()
    {
        base.Start();
        _startHealth = HitPoints;
    }
    public override void Update()
    {
        base.Update();

        if (Player.Instance.Dead)
            return;

        float percent = HitPoints / _startHealth;

        MoveDur = Mathf.Clamp(percent / 2, 0.4f, 10);

        if (!infinite)
        {
            _movingC = StartCoroutine(C_InfinitePath(true));
            infinite = true;
        }
    }
}
