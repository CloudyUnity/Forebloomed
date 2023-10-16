using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_Sucker : Entity
{
    [SerializeField] float _cooldown;
    [SerializeField] Vector2 _randomOffset;
    float _realCooldown;

    [SerializeField] BulletInfo _bulletInfo;
    [SerializeField] bool _isFrog;
    List<BulletEnemy> _bullets = new List<BulletEnemy>();

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

        if (_isFrog)
        {
            bool shooting = _timer > _cooldown * 0.8f || _timer < _cooldown * 0.3f;
            Anim.SetBool("Shooting", shooting);
        }

        _timer += Time.deltaTime;
        if (_timer > _realCooldown && CanSeePlayer())
        {
            BulletInfo info = _bulletInfo;
            info.spread += Confused * 10;
            var bullets = A_Factory.Instance.MakeEnemyBullet(transform.position, info);
            foreach (BulletEnemy b in bullets)
            {
                _bullets.Add(b);
                b.OverrideHomingTarget = gameObject;
                if (!_isFrog)
                    b.Homing += Random.Range(-5, 5f);
            }
            A_EventManager.InvokePlaySFX("Enemy Shoot");
            _timer = 0;
            _realCooldown = _cooldown + _randomOffset.AsRange();
            Squash(0.1f);
        }

        for (int i = _bullets.Count - 1; i >= 0; i--)
        {
            if (!_bullets[i].enabled)
                _bullets.RemoveAt(i);
        }
    }

    public override void Die(GameObject toDie)
    {
        foreach (var b in _bullets)
        {
            if (b != null)
                b.Homing = 0;
        }
        base.Die(toDie);
    }
}
