using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_Isopod : Entity
{
    [SerializeField] BulletInfo _info;
    [SerializeField] BulletInfo _info2;

    [SerializeField] float _cooldown;
    [SerializeField] Vector2 _randomOffset;
    float _realCooldown;

    float _timer;

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
            BulletInfo info = _info;
            info.spread += Confused * 10;
            A_Factory.Instance.MakeEnemyBullet(transform.position, info);
            A_EventManager.InvokePlaySFX("Enemy Shoot");
            Squash(0.1f);
            _timer = 0;
            _realCooldown = _cooldown + _randomOffset.AsRange();
        }
    }

    public override void Die(GameObject toDie)
    {
        A_Factory.Instance.MakeEnemyBullet(transform.position, _info2);
        A_EventManager.InvokePlaySFX("IsopodCrack");

        base.Die(toDie);
    }
}
