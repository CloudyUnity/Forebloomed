using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_Snail : Entity
{
    [SerializeField] float _cooldown;
    [SerializeField] Vector2 _randomOffset;
    float _realCooldown;

    [SerializeField] BulletInfo _bulletInfo;

    float _timer;

    bool infinite = false;

    public override void Start()
    {
        base.Start();
        _realCooldown = _cooldown + _randomOffset.AsRange();
    }

    public override void Update()
    {
        base.Update();

        if (Player.Instance.Dead)
            return;

        _timer += Time.deltaTime;   
        if (_timer > _realCooldown && CanSeePlayer())
        {
            BulletInfo info = _bulletInfo;
            info.spread += Confused * 10;
            A_Factory.Instance.MakeEnemyBullet(transform.position, info);
            A_EventManager.InvokePlaySFX("Enemy Shoot");
            _timer = 0;
            _realCooldown = _cooldown + _randomOffset.AsRange();
            Squash(0.1f);
        }

        if (!infinite)
        {
            _movingC = StartCoroutine(C_InfinitePath(true));
            infinite = true;
        }
    }
}
