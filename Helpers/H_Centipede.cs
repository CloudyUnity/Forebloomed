using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class H_Centipede : MonoBehaviour
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
    [SerializeField] O_Follower _body;
    [SerializeField] O_Follower _tail;
    [SerializeField] LayerMask _targetLayers;

    float _timeSinceBirth;

    public void Start()
    {
        if (!enabled)
        {
            Destroy(transform.parent.gameObject);
            return;
        }
        _targetDir = (GetTarget() - transform.position).normalized;

        MakeSegments();
    }

    void MakeSegments()
    {
        Transform last = transform;
        float realLength = _length.AsRange();

        for (int i = 0; i < realLength; i++)
        {
            O_Follower body = Instantiate(_body, transform.parent);
            body.transform.position = transform.position;

            body.Target = last;
            body.Distance = _distanceBetweenPoints;
            body.gameObject.SetActive(true);

            last = body.transform;
        }
        _tail.Distance = _distanceBetweenPoints;
        _tail.Target = last;
    }

    public void Update()
    {
        if (Player.Instance.Stopped)
            return;

        float dis = Vector2.Distance(transform.position, Player.Instance.transform.position);
        if (dis > 15)
        {
            transform.position = Player.Instance.transform.position;
        }

        _timeSinceBirth += Time.deltaTime;

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

    Vector3 GetTarget()
    {
        if (A_LevelManager.Instance.CurrentLevel.IsEven() && !E_Dummy.FamiliarsAllowedToHit)
            return Player.Instance.transform.position;

        Vector2 playerPos = Player.Instance.transform.position;
        var hits = Physics2D.OverlapCircleAll(playerPos, 6, _targetLayers);
        GameObject target = Player.Instance.gameObject;
        float dis = 99999999;
        foreach (var hit in hits)
        {
            float d = Vector2.Distance(hit.gameObject.transform.position, playerPos);
            if (target == null || d < dis)
            {
                target = hit.gameObject;
                dis = d;
            }
        }
        return target.transform.position;
    }

    void Move()
    {
        float angle = _attacking ? _zigSpread / 2 : _zigSpread;
        float speed = _timeSinceBirth > 0.4f ? _speed : 0;

        Vector3 dir = Quaternion.AngleAxis(Mathf.Sin(Time.time * _zigFreq) * angle, Vector3.back) * _targetDir;

        transform.position += dir * speed * Time.deltaTime;
        transform.right = dir;
    }
}
