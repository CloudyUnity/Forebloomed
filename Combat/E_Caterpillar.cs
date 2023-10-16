using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_Caterpillar : Entity
{
    bool _infinite;
    [SerializeField] bool _useGhosts = true;

    public override void Update()
    {
        base.Update();

        if (Player.Instance.Dead)
            return;

        if (!_infinite)
            _movingC = StartCoroutine(C_InfinitePath(_useGhosts));
        _infinite = true;
    }
}
