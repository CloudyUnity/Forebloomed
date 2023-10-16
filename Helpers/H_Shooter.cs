using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class H_Shooter : MonoBehaviour
{
    [SerializeField] LayerMask _enemyLayer;
    [SerializeField] float _range;
    [SerializeField] PlayerStats _stats;
    [SerializeField] SpriteRenderer _bullet;
    [SerializeField] bool _canAttackHedges;
    float _timer;
    GameObject _target;

    private void Update()
    {
        if (Player.Instance.Stopped)
            return;

        if (_target == null || Vector2.Distance(_target.transform.position, transform.position) > _range)
        {
            _target = GetTarget();
            return;
        }

        _timer += Time.deltaTime;
        if (_timer > _stats.FireRate / 100 && Input.GetKey(A_InputManager.Instance.Key("Shoot")))
        {
            StartCoroutine(C_ShootBullets());
            _timer = 0;
        }
    }

    IEnumerator C_ShootBullets()
    {
        int amount = (int)Mathf.Floor(_stats.Amount);
        int c = 0;
        for (int i = 0; i < amount; i++)
        {
            if (_target == null)
                yield break;

            float angle = _stats.AmountRange / _stats.Amount * i - _stats.AmountRange / 2;
            angle += _stats.Spread.AsRange();
            Shoot(angle);

            c++;
            if (c > PlayerGun.MAX_SHOTS_PER_FRAME)
            {
                c = 0;
                yield return null;
            }

            if (_stats.Delay > 0 && i != amount - 1)
                yield return new WaitForSeconds(_stats.Delay);
        }
    }

    void Shoot(float angle)
    {
        if (_target == null)
            return;

        BulletPlayer bullet;
        if (PlayerGun.LLEnd == null)
        {
            SpriteRenderer go = Instantiate(_bullet, transform.position, Quaternion.identity);
            bullet = go.GetComponent<BulletPlayer>();
        }
        else
        {
            bullet = PlayerGun.Pop();
            bullet.gameObject.SetActive(true);
            bullet.transform.position = transform.position;
        }

        Vector2 dir = (_target.transform.position - transform.position).normalized;
        dir = Quaternion.AngleAxis(angle, Vector3.back) * dir;

        bullet.Init(dir, _stats, _bullet);

        Squash(0.1f);
    }

    GameObject GetTarget()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, _range, _enemyLayer);

        if (hits.Length == 0)
            return null;

        if (!hits[0].TryGetComponent(out Entity _) && !_canAttackHedges)
            return null;

        return hits[0].gameObject;
    }

    bool _squashing;
    public void Squash(float dur) => StartCoroutine(C_SqaushStretch(dur));
    IEnumerator C_SqaushStretch(float dur)
    {
        if (_squashing)
            yield break;
        _squashing = true;

        float elapsed = 0;
        Vector2 start = transform.localScale;

        while (elapsed < dur)
        {
            float humped = 1 - A_Extensions.HumpCurve(elapsed / dur, 0, 1);
            transform.localScale = start + new Vector2(0.1f * -humped, 0.1f * humped);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = start;
        _squashing = false;
    }
}
