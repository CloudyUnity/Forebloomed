using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_Weevil : Entity
{
    [SerializeField] float _cooldown;
    [SerializeField] Vector2 _randomOffset;
    float _realCooldown;

    [SerializeField] BulletInfo _bulletInfo;

    float _timer;
    bool _shooting;

    public override void Start()
    {
        base.Start();
        _realCooldown = _cooldown + _randomOffset.AsRange();
        transform.rotation = Quaternion.Euler(0, 180, 0);
    }

    public override void Update()
    {
        base.Update();

        Anim.SetBool("Shooting", _shooting);

        if (Player.Instance.Dead)
            return;

        _timer += Time.deltaTime;
        if (_timer > _realCooldown && CanSeePlayer())
        {
            StartCoroutine(C_Shoot());
            A_EventManager.InvokePlaySFX("Enemy Shoot");
            _timer = 0;
            _realCooldown = _cooldown + _randomOffset.AsRange();
            Squash(0.1f);
        }
    }

    IEnumerator C_Shoot()
    {
        _shooting = true;
        for (int i = 1; i < 7; i++)
        {
            _bulletInfo.speed = (i + 1) * Vector2.one * 0.5f;
            BulletInfo info = _bulletInfo;
            info.spread += Confused * 10;
            A_Factory.Instance.MakeEnemyBullet(transform.position, info);
            yield return new WaitForSeconds(0.1f);
        }
        _timer = 0;
        _shooting = false;
    }
}
