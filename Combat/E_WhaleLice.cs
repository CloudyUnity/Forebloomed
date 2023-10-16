using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_WhaleLice : Entity
{
    [SerializeField] float _fakePercent;
    [SerializeField] float _speed;
    [SerializeField] Sprite _fakeSprite;
    [SerializeField] Vector2 _fakeDeadTime;
    float _startHealth;
    float _timer;
    bool _pretendDead;
    bool _hasPretended;

    public override void Start()
    {
        base.Start();
        _startHealth = HitPoints;
    }

    public override void Update()
    {
        base.Update();

        if (!_pretendDead)
        {
            Vector3 dir = (Player.Instance.HitBox - transform.position).normalized;
            transform.right = dir;
            transform.position += dir * Time.deltaTime * _speed / (Freeze + 1);
        }

        _timer += Time.deltaTime;
        if (_timer > _fakeDeadTime.AsRange() && _pretendDead)
        {
            Anim.enabled = true;
            _pretendDead = false;
        }
    }

    public override void TakeDamage(float dmg)
    {
        base.TakeDamage(dmg);

        if (HitPoints / _startHealth < _fakePercent && !_hasPretended)
        {
            FakeDie();
        }
    }

    void FakeDie()
    {
        Rend.sprite = _fakeSprite;
        Anim.enabled = false;
        _pretendDead = true;
        _hasPretended = true;
        A_EventManager.InvokePlaySFX("FakeDie");
        Squash(0.2f);
        _timer = 0;
    }
}
