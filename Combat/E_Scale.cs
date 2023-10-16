using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_Scale : Entity
{
    [SerializeField] float _cooldown;
    [SerializeField] Vector2 _randomOffset;
    float _realCooldown;

    [SerializeField] BulletInfo _bulletInfo;
    [SerializeField] LayerMask _wallLayer;
    [SerializeField] Animator[] _anims;

    float _timer;

    public override void Start()
    {
        base.Start();
        _realCooldown = _cooldown + _randomOffset.AsRange();
        StartCoroutine(C_TurnOnSprites());
    }

    IEnumerator C_TurnOnSprites()
    {
        foreach (var anim in _anims)
        {
            yield return new WaitForSecondsRealtime(Random.Range(0.05f, 0.2f));

            anim.enabled = true;
        }
    }

    public override void Update()
    {
        base.Update();

        if (Player.Instance.Dead)
            return;

        _timer += Time.deltaTime;
        if (_timer > _realCooldown && CanSeePlayer())
        {
            foreach (var anim in _anims)
            {
                A_Factory.Instance.MakeEnemyBullet(anim.transform.position, _bulletInfo);
            }
            A_EventManager.InvokePlaySFX("Enemy Shoot");
            _timer = 0;
            _realCooldown = _cooldown + _randomOffset.AsRange();
        }

        if (_timer > _realCooldown)
        {
            var hits = Physics2D.OverlapCircleAll(transform.position, 1, _wallLayer);
            if (hits.Length == 0)
                return;

            Vector2 dir = (hits[0].transform.position - transform.position).normalized;
            BulletInfo info = _bulletInfo;
            info.spread += Confused * 10;
            A_Factory.Instance.MakeEnemyBullet(transform.position, info, dir);
            A_EventManager.InvokePlaySFX("Enemy Shoot");
            Squash(0.1f);
            _timer = 0;
            _realCooldown = _cooldown + _randomOffset.AsRange();
        }
    }

    public override void TakeDamage(float dmg)
    {
        base.TakeDamage(dmg);

        foreach (var anim in _anims)
        {
            anim.SetTrigger("Damage");
        }
    }

    public override void Die(GameObject toDie)
    {
        base.Die(toDie);

        foreach (var anim in _anims)
        {
            MakeCorpse(anim.transform.position, Vector2.one * 0.1f, false);
        }
    }
}
