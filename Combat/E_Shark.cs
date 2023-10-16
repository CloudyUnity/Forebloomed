using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_Shark : Entity
{
    [SerializeField] float _speed;
    [SerializeField] float _minSpeed;
    [SerializeField] float _zigFreq;
    [SerializeField] float _zigSpread;

    [SerializeField] float _biteSFXFreq;
    //float _biteSFXCounter;

    [SerializeField] float _seperateRadius;
    [SerializeField] float _seperateSpeed;
    [SerializeField] LayerMask _sharkLayer;
    [SerializeField] SpriteRenderer _rend;

    float offset;
    float _timeSinceFlipY = 0;

    bool _superShark;

    public static readonly bool SHARK_COLOR_MODE = true;

    private void Awake()
    {
        transform.localScale = Vector3.one * Random.Range(1.75f, 3f);

        _superShark = A_Extensions.RandomChance(0.15f * A_LevelManager.Instance.DifficultyModifier, transform.localScale.x + transform.position.y);

        if (!SHARK_COLOR_MODE || !_superShark)
            return;

        float[] channels = new float[3];

        int s = Random.Range(0, 3);
        channels[s] = 0.2588f;
        channels[(int)Mathf.Repeat(s + 1, 3)] = 1;
        channels[(int)Mathf.Repeat(s+2, 3)] = Random.value;        

        _rend.color = new Color(channels[0], channels[1], channels[2]);

        _speed *= 1.1f;
    }

    public override void Start()
    {
        base.Start();
        //_biteSFXCounter = Time.time;
        HitPoints *= A_TimeManager.Instance.TimePercent * A_LevelManager.Instance.DifficultyModifier;
        offset = Random.Range(0, 5);        
    }

    public override void Update()
    {
        base.Update();

        Vector3 dir = GetDir();

        transform.position += dir * (_speed / (Freeze + 1)) * Time.deltaTime;
        transform.right = Vector2.Lerp(transform.right, dir, Time.deltaTime * 2);

        if (_timeSinceFlipY > 0.3f)
        {
            Rend.flipY = dir.x < 0;
            _timeSinceFlipY = 0;
        }
        _timeSinceFlipY += Time.deltaTime;

        _minSpeed = Mathf.Clamp(A_TimeManager.Instance.TimePercent * 2, 1, 4) + 1;

        ContactDamage = A_LevelManager.Instance.DifficultyModifier > 1 ? 7 : 3;

        //if (_biteSFXCounter < Time.time + offset && !Player.Instance.Dead)
        //{
        //    _biteSFXCounter += _biteSFXFreq;
        //    A_EventManager.InvokePlaySFX("Shark");
        //}
    }

    public override void InflictStatus()
    {
        if (A_LevelManager.Instance.DifficultyModifier > 1)
            return;

        base.InflictStatus();
    }

    public override void Die(GameObject toDie)
    {
        base.Die(toDie);
        A_EventManager.InvokeUnlock("Harpooned");
        A_TimeManager.Instance.TotalSharks--;
    }

    Vector3 GetDir()
    {
        Vector3 preDir = (transform.position - Player.Instance.HitBox).normalized;
        Vector3 target = Player.Instance.Dead ? Player.Instance.HitBox + preDir * 12 : Player.Instance.HitBox;

        Vector3 dir =  target - transform.position;

        if (dir.magnitude < _minSpeed)
            dir = dir.normalized * _minSpeed;

        float angle = Mathf.Sin((offset + Time.time) * _zigFreq) * _zigSpread;
        dir = Quaternion.AngleAxis(angle, Vector3.back) * dir;

        Vector3 sum = Vector2.zero;
        float count = 0;
        var hits = Physics2D.OverlapCircleAll(transform.position, _seperateRadius, _sharkLayer);

        if (hits.Length <= 1)
            return dir;

        foreach (var hit in hits)
        {
            if (hit.transform == transform)
                continue;

            Vector3 difference = transform.position - hit.transform.position;
            difference = difference.normalized / Mathf.Abs(difference.magnitude);
            sum += difference;
            count++;
        }

        sum /= count;
        sum *= _seperateSpeed;

        return dir + sum;
    }
}
