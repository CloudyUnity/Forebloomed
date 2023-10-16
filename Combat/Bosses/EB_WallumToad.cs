using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EB_WallumToad : EBoss
{
    int _subsBroken = 1;
    bool Vunerable { get { return _subsBroken >= 5; } }
    [SerializeField] GameObject _wetSub;
    [SerializeField] BulletInfo _burstInfo;
    [SerializeField] BulletInfo _vomitInfo;
    [SerializeField] BulletInfo _bounceInfo;
    [SerializeField] Color _redColor;
    [SerializeField] V_FocalPoint _focalPoint;
    bool _shotGun;
    float _timer;
    int _last;
    bool _attacking;

    private void OnEnable()
    {
        A_EventManager.OnSubBreak += SubBreak;
    }

    private void OnDisable()
    {
        A_EventManager.OnSubBreak -= SubBreak;
    }

    void SubBreak() => _subsBroken++;

    public override void Start()
    {
        base.Start();

        Instantiate(_wetSub, new Vector2(6.5f, 3), Quaternion.identity);
        Instantiate(_wetSub, new Vector2(6.5f, -3), Quaternion.identity);
        Instantiate(_wetSub, new Vector2(-6.5f, 3), Quaternion.identity);
        Instantiate(_wetSub, new Vector2(-6.5f, -3), Quaternion.identity);
    }

    public override void TakeDamage(float dmg)
    {
        if (!Vunerable)
            return;

        base.TakeDamage(dmg);
    }

    public override void Die(GameObject toDie)
    {
        if (!_shotGun)
            A_EventManager.InvokeUnlock("Drone");

        base.Die(toDie);
    }

    public override void Update()
    {
        if (HasDied)
            return;

        if (Input.GetKey(A_InputManager.Instance.Key("Shoot")))
            _shotGun = true;

        base.Update();

        Rend.color = Color.Lerp(Color.white, _redColor, (_subsBroken - 1) / 4);
        Rend.SetAlpha(_subsBroken < 5 ? 0.4f : 1);
        gameObject.layer = _subsBroken < 4 ? 0 : LayerMask.NameToLayer("Enemy");

        if (_subsBroken >= 4 && !_focalPoint.enabled)
            _focalPoint.enabled = true;

        _timer += Time.deltaTime * _subsBroken;

        if (!_attacking && _timer > 4)
        {
            int rand = Random.Range(0, 3);

            if (rand == _last)
                rand = (int)Mathf.Repeat(rand + 1, 2);

            _attacking = true;

            if (rand == 0)
                StartCoroutine(C_BurstAttack());
            if (rand == 1)
                StartCoroutine(C_VomitAttack());
            if (rand == 2)
                StartCoroutine(C_BouncyAttack());

            _last = rand;
        }
    }

    void BurstAttack()
    {
        A_Factory.Instance.MakeEnemyBullet(transform.position, _burstInfo, new Vector2(1, 0));
        A_Factory.Instance.MakeEnemyBullet(transform.position, _burstInfo, new Vector2(-1, 0));
        Anim.SetTrigger("Shoot");
        A_EventManager.InvokePlaySFX("Enemy Shoot");
    }

    void BounceAttack()
    {
        for (int i = -1; i < 2; i++)
        {
            Vector2 dir = Quaternion.Euler(0, 0, 30 * i) * (Player.Instance.transform.position - transform.position).normalized;
            BulletInfo info = _bounceInfo;
            info.spread += Confused * 10;
            A_Factory.Instance.MakeEnemyBullet(transform.position, info, dir);
        }
        Anim.SetTrigger("Shoot");
        A_EventManager.InvokePlaySFX("Enemy Shoot");
    }

    IEnumerator C_BurstAttack()
    {
        for (int i = 0; i < 3; i++)
        {
            if (HasDied)
                yield break;

            BurstAttack();
            Squash(0.2f / _subsBroken);

            yield return new WaitForSeconds(3 / _subsBroken);
        }
        _timer = 0;
        _attacking = false;
    }

    IEnumerator C_BouncyAttack()
    {
        for (int i = 0; i < 3; i++)
        {
            if (HasDied)
                yield break;

            BounceAttack();
            Squash(0.2f / _subsBroken);

            yield return new WaitForSeconds(3 / _subsBroken);
        }
        _timer = 0;
        _attacking = false;
    }

    IEnumerator C_VomitAttack()
    {
        float elapsed = 0;
        float dur = 2;
        float counter = 0;

        A_EventManager.InvokePlaySFX("Vomit");
        Squash(0.4f);


        while (elapsed < dur)
        {
            if (HasDied)
                yield break;

            if (elapsed > counter)
            {
                A_Factory.Instance.MakeEnemyBullet(transform.position, _vomitInfo);
                counter += 0.1f;
            }

            Anim.SetTrigger("Shoot");
            elapsed += Time.deltaTime;
            yield return null;
        }
        _timer = 0;
        _attacking = false;
    }
}
