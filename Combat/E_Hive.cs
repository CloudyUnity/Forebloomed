using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_Hive : Entity
{
    float _timer;
    [SerializeField] float _cooldown;
    [SerializeField] MakeInfo _info;
    [SerializeField] int _max;
    int _counter;

    public override void Update()
    {
        base.Update();

        if (Player.Instance.Dead || Vector2.Distance(transform.position, Player.Instance.transform.position) > SightRange)
            return;

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
