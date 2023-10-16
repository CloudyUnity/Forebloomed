using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_Bee : Entity
{
    [SerializeField] float _floatSpeed;
    [SerializeField] float _chargeSpeed;
    [SerializeField] float _floatFreq;
    [SerializeField] float _floatAmp;
    [SerializeField] float _floatMidline;
    [SerializeField] GameObject _sprite;
    [SerializeField] float _switchSpeed;
    float _timeSinceCharge;
    [SerializeField] BulletInfo _info;
    float _timer;
    float _offset;
    [SerializeField] float _cooldown;

    public override void Start()
    {
        base.Start();
        _offset = Random.Range(0f, 10f);
    }

    public override void Update()
    {
        base.Update();

        if (Player.Instance.Dead)
            return;

        Vector3 dir = (Player.Instance.HitBox - transform.position).normalized;

        if (Mathf.Repeat((Time.time + _offset) * _switchSpeed, 1) > 0.5f)
        {
            Floating(dir);
            _timeSinceCharge += Time.deltaTime;
            return;
        }

        Charging(dir);
        if (_timeSinceCharge > 0)
        {
            _timeSinceCharge = 0;
            A_EventManager.InvokePlaySFX("Bee");
            Squash(0.1f);
        }
    }

    void Floating(Vector3 dir)
    {
        transform.position += dir * Time.deltaTime * _floatSpeed / (Freeze + 1);

        Vector2 newPos = _sprite.transform.localPosition;
        newPos.y = Mathf.Sin(_timeSinceCharge * Mathf.PI * _floatFreq) * _floatAmp;
        newPos.y += _floatMidline;
        _sprite.transform.localPosition = newPos;
    }

    void Charging(Vector3 dir)
    {
        transform.position += dir * Time.deltaTime * _chargeSpeed / (Freeze + 1);
        _sprite.transform.localPosition = Vector2.Lerp(_sprite.transform.localPosition, new Vector2(0, _floatMidline), Time.deltaTime * 10f);

        _timer += Time.deltaTime;
        if (_timer >= _cooldown)
        {
            BulletInfo info = _info;
            info.spread += Confused * 10;
            A_Factory.Instance.MakeEnemyBullet(transform.position, info);
            _timer = 0;
        }
    }
}
