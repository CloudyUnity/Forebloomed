using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_Beetle : Entity
{
    bool infinite = false;
    [SerializeField] float _waitTime;
    float _timer;

    public override void Update()
    {
        base.Update();

        if (Player.Instance.Dead)
            return;

        _timer += Time.deltaTime;

        if (!infinite && _timer >= _waitTime)
        {
            _movingC = StartCoroutine(C_InfinitePath(true));
            infinite = true;
        }
    }
}
