using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EB_Katydid : EBoss
{
    float _timeSinceShoot;
    [SerializeField] float _cooldown;
    [SerializeField] GameObject _gun;
    [SerializeField] SpriteRenderer _gunRend;
    [SerializeField] ParticleSystem _ps;
    [SerializeField] float _radiusSize;
    [SerializeField] float _speed;

    [SerializeField] BulletInfo _blaster;
    [SerializeField] BulletInfo _sniper;

    [SerializeField] GameObject _pointer;
    float _pointerSize;

    bool canSeePlayer = false;

    float _timeSinceLastRouting = 0;

    public override void Start()
    {
        base.Start();

        if (A_BossManager.HideUIDebug)
        {
            _pointer.SetActive(false);
            return;
        }

        if (_pointer != null)
        {
            _pointer.transform.parent = null;
            _pointerSize = 0.9f;
            _pointer.transform.localScale = Vector2.zero;
        }
    }

    public override void Update()
    {
        base.Update();

        if (Player.Instance.Stopped || HasDied)
            return;

        _timeSinceShoot += Time.deltaTime;

        _speed = Vector2.Distance(Player.Instance.HitBox, transform.position) > 17 ? 0.08f : 0.35f;

        Anim.SetBool("Walking", IsMoving);
        Rend.flipX = Player.Instance.transform.position.x < transform.position.x;

        _timeSinceLastRouting += Time.deltaTime;
        if (_timeSinceLastRouting > 0.1f)
        {
            _movingC = StartCoroutine(C_MoveOneNode(Player.Instance.transform.position, _speed));
            _timeSinceLastRouting = 0;
        }

        canSeePlayer = CanSeePlayer();

        ManageGun();
        EnterShoot();

        if (_pointer != null)
        {
            _pointer.transform.localScale = Vector2.Lerp(_pointer.transform.localScale, _pointerSize * Vector2.one, Time.deltaTime * 3);

            Vector3 dir = (Player.Instance.transform.position - transform.position).normalized;
            _pointer.transform.position = Player.Instance.transform.position - dir;
            _pointer.transform.right = -dir;
        }
    }

    void EnterShoot()
    {
        float dis = Vector2.Distance(Player.Instance.transform.position, transform.position);

        float cool = dis < 2 ? _cooldown / 2 : _cooldown;

        if (!canSeePlayer || _timeSinceShoot < cool || TakingKnockback)
            return;

        _timeSinceShoot = 0;

        if (dis >= 5)
            ShootBullet(_sniper);
        else
            ShootBullet(_blaster);
    }

    void ManageGun()
    {
        Vector2 dir = GetDir();
        _gun.transform.right = Vector2.Lerp(_gun.transform.right, dir, Time.deltaTime * 8);
        _gunRend.sortingOrder = dir.y <= 0 ? 1301 : 1299;

        if (dir == Vector2.left)
        {
            float z = _gun.transform.rotation.z;
            float newZ = Mathf.Lerp(z, 0, Time.deltaTime);
            _gun.transform.rotation = Quaternion.Euler(0, 180, newZ);
            return;
        }

        if (dir.x >= 0) return;

        Vector3 rot = _gun.transform.rotation.eulerAngles;
        rot.x = 180;
        rot.z *= -1;
        _gun.transform.rotation = Quaternion.Euler(rot);
    }

    Vector2 GetDir()
    {
        if (!canSeePlayer)
        {
            return Player.Instance.transform.position.x <= transform.position.x ? Vector2.left : Vector2.right;
        }

        return (Player.Instance.HitBox - transform.position).normalized;
    }

    void ShootBullet(BulletInfo info)
    {
        var newInfo = info;
        newInfo.speed *= HealthPercent < 0.25f ? 1.1f : 1;

        float dis = Vector2.Distance(Player.Instance.transform.position, transform.position);
        newInfo.spread = dis < 2 ? 180 : info.spread;

        newInfo.spread += Confused * 10;
        A_Factory.Instance.MakeEnemyBullet(_ps.transform.position, newInfo);
        A_EventManager.InvokePlaySFX("Enemy Shoot");
        Squash(0.1f);

        _ps.Play();
    }

    public override void ActuallyDie()
    {
        base.ActuallyDie();
        Destroy(_pointer);
    }
}
