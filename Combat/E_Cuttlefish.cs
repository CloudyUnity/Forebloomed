using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_Cuttlefish : Entity
{
    [SerializeField] float _changeSpeed;
    [SerializeField] float _cooldown;
    [SerializeField] BulletInfo _info;
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

        Rend.SetAlpha(Mathf.Lerp(Rend.color.a, 0f, Time.deltaTime * _changeSpeed));

        _timer += Time.deltaTime;
        if (_timer > _realCooldown)
        {
            StartCoroutine(C_Brap());
            _timer = 0;
            _realCooldown = _cooldown + _randomOffset.AsRange();
        }
    }

    public override void TakeDamage(float dmg)
    {
        Rend.SetAlpha(0.6f);
        base.TakeDamage(dmg);
    }


    IEnumerator C_Brap()
    {
        for (int i = 0; i < 3; i++)
        {
            BulletInfo info = _info;
            info.spread += Confused * 10;
            A_Factory.Instance.MakeEnemyBullet(transform.position, info);
            A_EventManager.InvokePlaySFX("Enemy Shoot");
            Squash(0.15f);

            yield return new WaitForSeconds(0.2f);
        }
    }
}
