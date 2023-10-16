using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_SandWasp : Entity
{
    [SerializeField] float _idealDistance;
    [SerializeField] float _speed;
    [SerializeField] SinWave _wave;
    [SerializeField] BulletInfo _splurt;
    [SerializeField] BulletInfo _brap;
    [SerializeField] float _cooldown;
    [SerializeField] Vector2 _randomOffset;
    float _realCooldown;
    float _timer;
    float _timeSinceStart;

    float _sfxTimer;

    public override void Start()
    {
        base.Start();
        _realCooldown = _cooldown + _randomOffset.AsRange();
    }

    public override void Update()
    {
        base.Update();

        _timeSinceStart += Time.deltaTime;

        if (Player.Instance.Dead || Vector2.Distance(transform.position, Player.Instance.transform.position) > SightRange)
            return;

        Vector3 vector = Player.Instance.HitBox - transform.position;
        float diff = vector.magnitude - _idealDistance;

        Vector2 newPos = transform.position + vector.normalized * diff * _speed * Time.deltaTime / (Freeze + 1);
        newPos.y += A_Extensions.GetSinWave(_wave, _timeSinceStart) * Time.deltaTime;
        transform.position = newPos;

        _timer += Time.deltaTime;
        if (_timer > _realCooldown)
        {
            if (A_Extensions.Rand()) Splurt();
            else StartCoroutine(C_Brap());
            _timer = 0;
            _realCooldown = _cooldown + _randomOffset.AsRange();
        }

        _sfxTimer += Time.deltaTime;
        if (_sfxTimer > 0.45f)
        {
            A_EventManager.InvokePlaySFX("Wasp");
            _sfxTimer = 0;
        }
    }

    void Splurt()
    {
        BulletInfo info = _splurt;
        info.spread += Confused * 10;
        A_Factory.Instance.MakeEnemyBullet(transform.position, info);
        A_EventManager.InvokePlaySFX("Enemy Shoot");
        Squash(0.2f);
    }


    IEnumerator C_Brap()
    {
        for (int i = 0; i < 10; i++)
        {
            BulletInfo info = _brap;
            info.spread += Confused * 10;
            A_Factory.Instance.MakeEnemyBullet(transform.position, info);
            A_EventManager.InvokePlaySFX("Enemy Shoot");
            Squash(0.1f);

            yield return new WaitForSeconds(0.2f);
        }
    }
}
