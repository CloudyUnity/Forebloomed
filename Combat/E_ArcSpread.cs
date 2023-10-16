using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_ArcSpread : Entity
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

        transform.right = (Player.Instance.HitBox - transform.position).normalized;

        _timer += Time.deltaTime;
        if (_timer > _realCooldown && CanSeePlayer())
        {
            Shoot();
            _timer = 0;
            _realCooldown = _cooldown + _randomOffset.AsRange();
        }

        if (!infinite)
        {
            _movingC = StartCoroutine(C_InfinitePath(true));
            infinite = true;
        }
    }

    void Shoot()
    {
        //A_Factory.Instance.MakeEnemyBullet(transform.position, _bulletInfo);
        A_EventManager.InvokePlaySFX("Enemy Shoot");
        Squash(0.1f);

        for (int i = -45; i < 45; i += 20)
        {
            Vector2 dir = Quaternion.Euler(0, 0, i) * (Player.Instance.HitBox - transform.position).normalized;
            A_Factory.Instance.MakeEnemyBullet(transform.position, _bulletInfo, dir);
        }
    }
}
