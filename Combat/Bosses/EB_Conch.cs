using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EB_Conch : EBoss
{
    [SerializeField] float _speed;
    [SerializeField] float _rotationMax;
    [SerializeField] float _rotationSpeed;
    [SerializeField] bool _isAlt;
    [SerializeField] BulletInfo _wallInfo;
    [SerializeField] BulletInfo _aimInfo;
    [SerializeField] BulletInfo _bigBoi;
    [SerializeField] float _cooldown;
    [SerializeField] SpriteRenderer _whiteRend;
    [SerializeField] SpriteRenderer _rend;
    [SerializeField] Color _redTele, _orangeTele, _yellowTele;

    float _timer;
    bool _attacking;
    float _multiplier;

    int _last;

    public override void Update()
    {
        base.Update();

        if (Player.Instance.Dead || HasDied)
            return;

        _multiplier = HealthPercent > 0.5f ? 1 : 1.5f;
        float speedMult = Mathf.Lerp(2, 1, HealthPercent);

        transform.position += Vector3.right * Time.deltaTime * _speed * speedMult / (Freeze + 1);

        transform.rotation = Quaternion.Euler(0, 0, (Mathf.PingPong(Time.time * _rotationSpeed, 2) - 1) * _rotationMax);

        _timer += Time.deltaTime * _multiplier;
        if (_timer > _cooldown && !_attacking)
        {
            _attacking = true;

            int rand = Random.Range(0, 3);

            if (rand == _last)
                rand = (int)Mathf.Repeat(rand + 1, 2);

            if (rand == 0)
                StartCoroutine(C_Wall());
            if (rand == 1)
                StartCoroutine(C_Aim());
            if (rand == 2)
                StartCoroutine(C_Big());

            _last = rand;
        }
    }

    void WallAttack()
    {
        int remove = Mathf.RoundToInt(new Vector2(-2f, 0).AsRange());
        int remove2 = Mathf.RoundToInt(new Vector2(0, 2f).AsRange());
        for (float i = -2f; i <= 3f; i += 0.25f)
        {
            BulletInfo info = _wallInfo;

            if (i.Is(remove, remove + 0.25f, remove + 0.5f, remove - 0.25f, remove - 0.5f, remove2, remove2 + 0.25f, remove2 + 0.5f, remove2 - 0.25f, remove2 - 0.5f))
            {
                if (_isAlt)
                    continue;
                info.speed -= 1.5f * Vector2.one;                
            }

            A_Factory.Instance.MakeEnemyBullet(new Vector2(transform.position.x, i - 0.7f), info, Vector2.right);
        }
    }

    void AimAttack()
    {
        for (int i = -2; i < 2.5f; i++)
        {
            BulletInfo info = _aimInfo;
            if (_isAlt)
            {
                A_Factory.Instance.MakeEnemyBullet(new Vector2(transform.position.x + 15, i), info);
                continue;
            }
            A_Factory.Instance.MakeEnemyBullet(new Vector2(transform.position.x, i), info);
        }
    }

    void BigAttack()
    {
        BulletInfo info = _bigBoi;
        info.spread += Confused * 10;
        A_Factory.Instance.MakeEnemyBullet(transform.position, info, Vector2.right);
    }

    void SidesAttack()
    {
        A_Factory.Instance.MakeEnemyBullet(new Vector2(transform.position.x, 2.3f), _wallInfo, Vector2.right);
        A_Factory.Instance.MakeEnemyBullet(new Vector2(transform.position.x, -2), _wallInfo, Vector2.right);
    }

    IEnumerator C_Wall()
    {
        yield return new WaitForSeconds(0.2f);

        for (int i = 0; i < 3; i++)
        {
            if (HasDied)
                break;

            WallAttack();
            A_EventManager.InvokePlaySFX("Conch1");
            Squash(1 / _multiplier);

            yield return new WaitForSeconds(1.5f / _multiplier);
        }
        _timer = 0;
        _attacking = false;
    }

    IEnumerator C_Aim()
    {
        StartCoroutine(C_TelegraphAttack(_redTele));
        yield return new WaitForSeconds(0.4f);
        for (int i = 0; i < 5; i++)
        {
            if (HasDied)
                break;

            AimAttack();
            A_EventManager.InvokePlaySFX("Conch2");
            Squash(0.1f / _multiplier);

            yield return new WaitForSeconds(0.2f / _multiplier);
        }
        _timer = 0;
        _attacking = false;
    }

    IEnumerator C_Big()
    {
        yield return new WaitForSeconds(0.15f);

        for (int i = 0; i < 3; i++)
        {
            if (HasDied)
                break;

            BigAttack();
            A_EventManager.InvokePlaySFX("Conch3");
            for (int j = 0; j < 3 && !_isAlt; j++)
            {
                SidesAttack();
                yield return new WaitForSeconds(0.1f);
            }
            Squash(1 / _multiplier);

            yield return new WaitForSeconds(1.5f / _multiplier);
        }
        _timer = 0;
        _attacking = false;
    }

    public override void TakeDamage(float dmg)
    {
        base.TakeDamage(dmg);
        StartCoroutine(C_Flash());
    }


    IEnumerator C_Flash()
    {
        float elapsed = 0;
        float dur = 0.1f;

        while (elapsed < dur)
        {
            _whiteRend.SetAlpha(Mathf.Lerp(1, 0, elapsed / dur));
            elapsed += Time.deltaTime;
            yield return null;
        }
        _whiteRend.SetAlpha(0);
    }

    IEnumerator C_TelegraphAttack(Color color)
    {
        float elapsed = 0;
        float dur = 0.3f;

        while (elapsed < dur)
        {
            float curved = A_Extensions.HumpCurve(elapsed / dur, 1, 0);
            _rend.color = Color.Lerp(Color.white, color, curved);
            elapsed += Time.deltaTime;
            yield return null;
        }
        _rend.color = Color.white;
    }
}
