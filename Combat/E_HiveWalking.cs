using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_HiveWalking : Entity
{
    float _timer;
    [SerializeField] float _cooldown;
    [SerializeField] MakeInfo _info;
    [SerializeField] int _max;
    [SerializeField] float _waitTime;
    int _counter;
    bool infinite;

    public override void Update()
    {
        base.Update();

        if (Player.Instance.Dead)
            return;

        if (!infinite && _timer >= _waitTime)
        {
            _movingC = StartCoroutine(C_InfinitePath(true));
            infinite = true;
        }

        _timer += Time.deltaTime;

        if (_counter >= _max)
            return;

        if (_timer >= _cooldown)
        {
            _counter++;
            A_Factory.Instance.MakeBasic(transform.position, _info);
            A_EventManager.InvokePlaySFX("Hive");
            Squash(0.3f);
            _timer = 0;
        }
    }
}
