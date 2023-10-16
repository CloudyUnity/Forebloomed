using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_Centipede : Entity
{
    bool _attacking;

    float _aTimer;
    float _aDur;

    Vector3 _targetDir;

    [SerializeField] Vector2 _aFreq;
    [SerializeField] float _distance;
    [SerializeField] float _speed;
    [SerializeField] float _zigFreq;
    [SerializeField] float _zigSpread;
    [SerializeField] Vector2 _length;
    [SerializeField] float _distanceBetweenPoints;
    [SerializeField] E_CentiBody _body;
    [SerializeField] O_Follower _tail;

    float _timeSinceBirth;
    [SerializeField] float _digSFXFreq;
    float _digSFXCounter;

    List<E_CentiBody> _bodies = new List<E_CentiBody>();

    public override void Start()
    {
        base.Start();

        if (!enabled)
        {
            Destroy(transform.parent.gameObject);
            return;
        }            

        MakeSegments();
    }

    void MakeSegments()
    {
        Transform last = transform;
        float realLength = _length.AsRange();

        for (int i = 0; i < realLength; i++)
        {
            E_CentiBody body = Instantiate(_body, transform.parent);
            body.transform.position = transform.position;
            //E_CentiBody body = go.GetComponent<E_CentiBody>();
            body.Init(this);
            _bodies.Add(body);

            O_Follower script = body.GetComponent<O_Follower>();
            script.Target = last;
            script.Distance = _distanceBetweenPoints;
            body.gameObject.SetActive(true);

            last = body.transform;
        }
        _tail.Distance = _distanceBetweenPoints;
        _tail.Target = last;
        _bodies.Add(_tail.GetComponent<E_CentiBody>());
    }

    public override void Update()
    {
        base.Update();

        _timeSinceBirth += Time.deltaTime;

        if (_digSFXCounter < _timeSinceBirth)
        {
            _digSFXCounter += _digSFXFreq;
            A_EventManager.InvokePlaySFX("Dig");
        }

        ChangeModes();
        Move();
    }

    void ChangeModes()
    {
        _aTimer += Time.deltaTime;

        if (_aTimer > _aDur)
        {
            _attacking = !_attacking;
            _aTimer = 0;
            _targetDir = (GetTarget() - transform.position).normalized;
            _aDur = _aFreq.AsRange();
        }
    }

    Vector3 GetTarget() => Player.Instance.HitBox + (_attacking ? Vector3.zero : new Vector3(_distance.AsRange(), _distance.AsRange()));

    void Move()
    {
        float angle = _attacking ? _zigSpread / 2 : _zigSpread;
        float speed = _timeSinceBirth > 0.4f ? _speed : 0;

        Vector3 dir = Quaternion.AngleAxis(Mathf.Sin(Time.time * _zigFreq) * angle, Vector3.back) * _targetDir;

        transform.position += dir * speed * Time.deltaTime;
        transform.right = dir;
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

    public override void Die(GameObject toDie)
    {
        foreach (var body in _bodies)
            body.MakeCorpse(body.transform.position, body.transform.lossyScale, false);
        NormalSize = transform.lossyScale;
        base.Die(transform.parent.gameObject);
    }

    public override void Despawn(GameObject toDespawn) => Destroy(transform.parent.gameObject);
}
