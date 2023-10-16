using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_Dasher : Entity
{
    [SerializeField] float _waitTime;
    [SerializeField] float _cooldown;
    [SerializeField] float _distance;
    [SerializeField] Vector2 _dashDur;
    float _timer;
    bool _dashing;

    public override void Start()
    {
        base.Start();
        transform.rotation = Quaternion.Euler(0, 180, 0);
    }

    public override void Update()
    {
        base.Update();

        if (Player.Instance.Dead)
            return;

        _timer += Time.deltaTime;

        Anim.SetBool("Dashing", _dashing);

        if (_dashing || !CanSeePlayer())
            return;

        if (_timer > _cooldown && Vector2.Distance(transform.position, Player.Instance.transform.position) < _distance)
        {
            StartCoroutine(C_Dash());
            _timer = 0;
            return;
        }
    }

    IEnumerator C_Dash()
    {
        _dashing = true;
        float elapsed = 0;
        float dur = _dashDur.AsRange() * (Freeze + 1);
        Squash(dur);

        Vector3 dir = (Player.Instance.HitBox - transform.position).normalized;
        Vector2 endPos = transform.position + dir * _distance;
        Vector2 start = transform.position;

        A_EventManager.InvokePlaySFX("Flea");

        while (elapsed < dur)
        {
            float curved = A_Extensions.CosCurve(elapsed / dur);
            transform.position = Vector2.Lerp(start, endPos, curved);
            elapsed += Time.deltaTime;
            yield return null;
        }
        _dashing = false;
    }
}
