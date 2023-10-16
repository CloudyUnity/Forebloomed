using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_Dragonfly : Entity
{
    [SerializeField] float _distance;
    [SerializeField] float _cooldown;
    [SerializeField] Vector2 _cooldownDiff;
    [SerializeField] float _speed;

    [SerializeField] float _shootCooldown;
    [SerializeField] BulletInfo _info;
    float _shootTimer;
    float _timer = 999;
    float _realCooldown;
    Vector3 _targetDir;
    Vector3 _lastDir;

    public override void Start()
    {
        base.Start();
        _realCooldown = _cooldown + _cooldownDiff.AsRange();
    }

    public override void Update()
    {
        base.Update();

        _timer += Time.deltaTime;
        if (_timer > _realCooldown)
        {
            Vector3 targetPos = Player.Instance.HitBox + new Vector3(_distance.AsRange(), _distance.AsRange());
            _targetDir = (targetPos - transform.position).normalized;
            _realCooldown = _cooldown + _cooldownDiff.AsRange();

            _timer = 0;
        }        

        Vector3 dir = (_targetDir + _lastDir).normalized;
        transform.position += dir * _speed * Time.deltaTime / (Freeze + 1);

        transform.up = dir;

        _lastDir = dir;

        _shootTimer += Time.deltaTime;
        if (_shootTimer > _shootCooldown)
        {
            BulletInfo info = _info;
            info.spread += Confused * 10;
            A_Factory.Instance.MakeEnemyBullet(transform.position, info, -dir);
            Squash(0.1f);
            _shootTimer = 0;
        }
    }
}
