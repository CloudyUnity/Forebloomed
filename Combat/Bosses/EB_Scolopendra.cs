using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EB_Scolopendra : EBoss
{
    [SerializeField] float _distance;
    [SerializeField] float _speed;
    [SerializeField] float _zigFreq;
    [SerializeField] float _zigSpread;
    [SerializeField] Vector2 _length;
    [SerializeField] float _distanceBetweenPoints;
    [SerializeField] E_CentiBody _body;
    [SerializeField] O_Follower _tail;
    [SerializeField] float _surroundRadius;
    [SerializeField] float _surroundSpeed;
    [SerializeField] BulletInfo _bulletInfoRoam;
    [SerializeField] BulletInfo _bulletInfoSurround;
    [SerializeField] BulletInfo _bulletInfoHoming;
    [SerializeField] float _cooldownRoam;
    [SerializeField] float _cooldownSurround;
    [SerializeField] float _cooldownAttack;
    float _fireTimer;
    Vector3 _target;
    float _timeSinceBirth;
    int _surroundCount;

    List<E_CentiBody> _bodies = new List<E_CentiBody>();
    float _sfxCounter;
    [SerializeField] float _sfxFreq;

    [SerializeField] GameObject _pointer;
    float _pointerSize;

    public override void Start()
    {
        base.Start();
        MakeSegments();
        if (A_BossManager.HideUIDebug)
        {
            _pointer.SetActive(false);
            return;
        }

        _pointer.transform.parent = null;
        _pointerSize = _pointer.transform.localScale.x;
        _pointer.transform.localScale = Vector2.zero;
    }

    public override void Update()
    {
        if (HasDied)
            return;

        base.Update();
        _timeSinceBirth += Time.deltaTime;

        if (_pointer != null)
        {
            _pointer.transform.localScale = Vector2.Lerp(_pointer.transform.localScale, _pointerSize * Vector2.one, Time.deltaTime * 3);

            Vector3 dir = (Player.Instance.transform.position - transform.position).normalized;
            _pointer.transform.position = Player.Instance.transform.position - dir;
            _pointer.transform.right = -dir;
        }

        if (_sfxCounter < _timeSinceBirth)
        {
            _sfxCounter += _sfxFreq;
            bool playSFX = false;
            foreach (E_CentiBody body in _bodies)
            {
                if (Vector2.Distance(Player.Instance.transform.position, body.transform.position) <= 3)
                {
                    playSFX = true;
                    break;
                }                    
            }
            if (playSFX || Vector2.Distance(Player.Instance.transform.position, transform.position) <= 5)
                A_EventManager.InvokePlaySFX("Scolop");
        }

        foreach (E_CentiBody body in _bodies)
        {
            if (Vector3.Distance(Player.Instance.transform.position, body.transform.position) < 6)
            {
                A_EventManager.InvokeCameraShake(0.02f, 0.5f);
                break;
            }
        }
        

        _fireTimer += Time.deltaTime;

        if (HealthPercent.IsBetween(0.6f, 0.8f) || HealthPercent.IsBetween(0.2f, 0.4f))
        {
            SurroundPhase();
            return;
        }

        if (HealthPercent.IsBetween(0, 0.2f))
        {
            AttackPhase();
            return;
        }

        RoamPhase();
    }

    void MakeSegments()
    {
        Transform last = transform;
        float realLength = _length.AsRange();

        for (int i = 0; i < realLength; i++)
        {
            E_CentiBody body = Instantiate(_body, transform.parent);
            body.transform.position = transform.position;
            body.Init(this);
            _bodies.Add(body);

            O_Follower script = body.GetComponent<O_Follower>();
            script.Target = last;
            script.Distance = _distanceBetweenPoints;
            body.gameObject.SetActive(true);

            body.GetComponent<SpriteRenderer>().sortingOrder = 1290 + i;

            last = body.transform;
        }
        _tail.Distance = _distanceBetweenPoints;
        _tail.Target = last;
        _bodies.Add(_tail.GetComponent<E_CentiBody>());
        _tail.GetComponent<SpriteRenderer>().sortingOrder = (int)(1290 + realLength);
    }

    void AttackPhase()
    {
        Vector3 dir = (_target - transform.position).normalized;
        Vector2 playerDir = (Player.Instance.transform.position - transform.position).normalized;
        float dotProduct = Vector3.Dot(dir, new Vector3(playerDir.x, playerDir.y, 0));
        float angle = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;

        if (_target == null || Vector2.Distance(transform.position, _target) < 0.5f || angle < 15)
        {
            _target = Player.Instance.transform.position + new Vector3(_distance.AsRange() * 0.5f, _distance.AsRange() * 0.5f);
        }

        if (_fireTimer >= _cooldownAttack)
        {
            BulletInfo info = _bulletInfoHoming;
            info.spread += Confused * 10;
            A_Factory.Instance.MakeEnemyBullet(transform.position, info);
            A_EventManager.InvokePlaySFX("Enemy Shoot");
            Squash(0.1f);

            _fireTimer = 0;
        }

        Move(dir);
    }

    void RoamPhase()
    {
        Vector3 dir = (_target - transform.position).normalized;
        Vector2 playerDir = (Player.Instance.transform.position - transform.position).normalized;
        float dotProduct = Vector3.Dot(dir, new Vector3(playerDir.x, playerDir.y, 0));
        float angle = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;

        if (_target == null || Vector2.Distance(transform.position, _target) < 1 || angle < 15)
        {
            _target = Player.Instance.transform.position + new Vector3(_distance.AsRange(), _distance.AsRange());
        }

        Move(dir);

        if (_fireTimer >= _cooldownRoam)
        {
            foreach (var body in _bodies)
            {
                if (Random.Range(0, 100) < 50)
                    continue;

                BulletInfo info = _bulletInfoRoam;
                info.spread += Confused * 10;
                A_Factory.Instance.MakeEnemyBullet(body.transform.position, info);
                body.Squash(0.2f);
            }
            _fireTimer = 0;
            A_EventManager.InvokePlaySFX("Enemy Shoot");
        }
    }

    void SurroundPhase()
    {
        Vector3 dir = Player.Instance.transform.position - transform.position;
        float dis = dir.magnitude;

        if (dis > _surroundRadius)
        {         
            Debug.DrawRay(transform.position, dir.normalized * 20, Color.yellow);
            Move(dir.normalized);
            return;
        }

        Vector3 oldPos = transform.position;
        transform.RotateAround(Player.Instance.transform.position, Vector3.back, _surroundSpeed * Time.deltaTime);
        transform.right = (transform.position - oldPos).normalized;

        if (_fireTimer >= _cooldownSurround)
        {
            _surroundCount = (int)Mathf.Repeat(_surroundCount + 1, _bodies.Count - 1);
            BulletInfo info = _bulletInfoSurround;
            info.spread += Confused * 10;
            A_Factory.Instance.MakeEnemyBullet(_bodies[_surroundCount].transform.position, info);
            A_EventManager.InvokePlaySFX("Enemy Shoot");
            _bodies[_surroundCount].Squash(0.2f);
            _fireTimer = 0;
        }
    }

    void Move(Vector2 dir)
    {
        Vector3 newDir = Quaternion.AngleAxis(Mathf.Sin(Time.time * _zigFreq) * _zigSpread, Vector3.back) * dir;
        transform.position += newDir * (_speed) * Time.deltaTime;
        transform.right = newDir;
    }

    public override void AnimDamage()
    {

    }

    public void FakeTakeDamage(float dmg)
    {
        base.TakeDamage(dmg);
    }

    public override void TakeDamage(float dmg)
    {
        base.TakeDamage(dmg);
        base.AnimDamage();
    }

    public override void ActuallyDie()
    {
        foreach (var body in _bodies)
        {
            body.MakeCorpse(body.transform.position, body.transform.lossyScale, false);
        }
        NormalSize = transform.lossyScale;

        Destroy(_pointer);

        base.ActuallyDie(transform.parent.gameObject);
    }
}
