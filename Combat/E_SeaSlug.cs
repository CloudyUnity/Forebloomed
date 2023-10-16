using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_SeaSlug : Entity
{
    [SerializeField] BulletInfo _info;
    float _timer;
    float _realTimer;
    bool _up;

    public override void Update()
    {
        base.Update();

        if (Player.Instance.Dead)
            return;

        _timer += Time.deltaTime;
        _realTimer += Time.deltaTime;

        float cooldown = 0.2f + Mathf.Clamp(_realTimer, 0, 6) / 30;
        
        if (_timer > cooldown)
        {
            Vector2 dir = Random.onUnitSphere.normalized;
            Vector2 dir2 = (Player.Instance.HitBox - transform.position).normalized;
            dir = (dir + dir2/2).normalized;
            A_Factory.Instance.MakeEnemyBullet(transform.position, _info, dir);
            A_EventManager.InvokePlaySFX("Enemy Shoot");
            Squash(0.1f);

            _up = !_up;
            Anim.SetBool("Up", _up);
            _timer = 0;
        }
    }
}
