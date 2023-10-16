using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_ParaSnail : Entity
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
        BulletInfo info = _bulletInfo;
        info.spread += Confused * 10;
        A_Factory.Instance.MakeEnemyBullet(transform.position, info);
        A_EventManager.InvokePlaySFX("Enemy Shoot");

        for (int i = 0; i < 360; i += 45)
        {
            float A = Mathf.Deg2Rad * i;
            Vector2 dir = new Vector2(Mathf.Cos(A), Mathf.Sin(A)); // Unit Circle Coords
            A_Factory.Instance.MakeEnemyBullet(transform.position, _bulletInfo, dir);
            Squash(0.3f);
        }
    }
}
