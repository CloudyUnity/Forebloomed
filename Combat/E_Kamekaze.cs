using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_Kamekaze : Entity
{
    [SerializeField] GameObject _explosion;
    [SerializeField] float _speed;
    [SerializeField] LayerMask _layers;
    Vector3 _dir;

    public override void Update()
    {
        base.Update();

        if (_dir == Vector3.zero)
        {
            _dir = (Player.Instance.HitBox - transform.position).normalized;
            Rend.flipX = _dir.x < 0;
        }

        transform.position += _dir * _speed * Time.deltaTime / (Freeze + 1);

        var hits = Physics2D.OverlapCircleAll(transform.position, 0.2f, _layers);

        if (hits.Length == 0)
            return;

        if (hits.Length == 1 && hits[0].gameObject == gameObject)
            return;

        Die(gameObject);
    }

    public override void TakeDamage(float dmg)
    {
        base.TakeDamage(dmg);

        Vector3 dir = (Player.Instance.HitBox - transform.position).normalized;
        transform.position += -dir * 0.1f;
    }

    public override void Die(GameObject toDie)
    {
        GameObject go = Instantiate(_explosion, transform.position, Quaternion.identity);
        go.transform.localScale = Vector2.one * 0.5f;

        base.Die(toDie);
    }
}
