using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_Crab : Entity
{
    bool infinite = false;
    [SerializeField] float _waitTime;
    [SerializeField] float _switchTime;
    [SerializeField] BulletInfo _info;
    float _timer;
    float _timer2;

    bool _shelled;

    private void OnEnable()
    {
        A_EventManager.OnPlayerBulletCollide += BulletCollide;

        _timer2 = Random.value;
    }

    private void OnDisable()
    {
        A_EventManager.OnPlayerBulletCollide -= BulletCollide;
    }

    public override void Update()
    {
        base.Update();

        if (Player.Instance.Dead)
            return;

        _timer += Time.deltaTime;
        _timer2 += Time.deltaTime;

        Anim.SetBool("Moving", IsMoving);
        Anim.SetBool("Shelled", _shelled);

        PauseMovement = _shelled || _timer2 < 0.4f;

        if (!infinite && _timer >= _waitTime)
        {
            _movingC = StartCoroutine(C_InfinitePath(true));
            infinite = true;
        }

        float multiplier = _shelled ? 0.5f : 1f;
        if (_timer2 >= _switchTime * multiplier)
        {
            _shelled = !_shelled;
            _timer2 = 0;
        }
    }

    public override void TakeDamage(float dmg)
    {
        if (_shelled)
        {
            return;
        }

        base.TakeDamage(dmg);
    }

    void BulletCollide(BulletPlayer bullet, GameObject collision)
    {
        if (collision == gameObject && _shelled)
        {
            _info.speed = bullet.Speed * 0.25f * Vector2.one;
            BulletInfo info = _info;
            info.spread += Confused * 10;
            A_Factory.Instance.MakeEnemyBullet(transform.position, info, -bullet.Dir);
            A_EventManager.InvokePlaySFX("Reflect");
            Squash(0.05f);
        }
    }
}
