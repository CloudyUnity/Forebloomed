using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class B_Knife : MonoBehaviour
{
    public SpriteRenderer _rend;
    [SerializeField] Rigidbody2D _rb;
    [SerializeField] LayerMask _targetLayer;
    [SerializeField] LayerMask _entityLayer;
    [SerializeField] float _extraDissTime;
    public bool AntiInflict;
    public string _sfx;
    [SerializeField] bool _useExplosions;
    [SerializeField] GameObject _explosion;
    [SerializeField] TrailRenderer _trail;
    [SerializeField] float _trailMultiplier;
    [SerializeField] bool _pointInDirTravelling;
    [SerializeField] bool _dontFuckWithTheTrail;
    [SerializeField] Sprite[] _sprites;

    KeyCode _ability;

    public B_Knife NextInList = null;

    public float Range;
    public float _dmg;
    public Vector2 Dir;
    public Vector3 _lastPos;
    float _homing;
    GameObject _target;
    public float Speed;

    bool _lodged;
    bool _dropped;

    float _timer;

    public bool NotDissolveUntilRange;
    public bool DontCallEvents;
    public bool Missed = false;

    private void Start()
    {
        _ability = A_InputManager.Instance.Key("Ability");
    }

    private void Update()
    {
        if (_gathering)
            return;

        if (Player.Instance.AbilityPercent >= 1 && Input.GetKeyDown(_ability))
            Gather();

        if (_lodged || _dropped)
            return;

        _timer += Time.deltaTime;
        if (_timer > Range)
        {
            Drop();
            A_EventManager.InvokePlayerBurMiss(this);
            Missed = true;
            return;
        }

        if (_pointInDirTravelling)
        {
            Vector3 dir = (transform.position - _lastPos).normalized;
            _lastPos = transform.position;

            if (dir != Vector3.zero)
                transform.right = dir;
        }
        else
            transform.right = Dir;
    }

    private void FixedUpdate()
    {
        if (_lodged || _dropped || _gathering)
            return;

        if (_homing != 0)
        {
            HomeOnTarget();
        }
    }

    public void Init(Vector2 dir, PlayerStats stats)
    {
        _rend.sprite = _sprites.RandomItem();
        transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360f));

        A_EventManager.InvokePlaySFX(_sfx);
        A_EventManager.InvokePlayerBurSpawn(this);
        _lastPos = transform.position;

        if (ItemManager.Instance.AllColors.Count > 0)
        {
            _rend.color = ItemManager.Instance.AllColors.RandomItem();
            _rend.SetAlpha(1);
        }

        if (!_dontFuckWithTheTrail && _trail != null)
        {
            _trail.startWidth = stats.BulletSize * _trailMultiplier;
        }

        Speed = Mathf.Clamp(stats.BulletSpeed, 0.5f, float.MaxValue);
        _rb.velocity = dir * Speed;

        transform.localScale = stats.BulletSize * Vector2.one;

        Range = stats.Range;

        if (stats.Damage > 0)
            _dmg = Mathf.Clamp(stats.Damage, 0.05f, float.MaxValue);

        Dir = dir;

        _homing = stats.Homing;

        _timer = 0;
        _lodged = false;
        _dropped = false;
        _gathering = false;
        _target = null;
        Missed = false;
        NotDissolveUntilRange = false;
        DontCallEvents = false;
        Missed = false;
    }

    public void ChangeDirection(Vector2 dir) => _rb.velocity = dir.normalized * Speed;

    public void SetTarget(GameObject target) => _target = target;

    GameObject FindTarget()
    {
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, Mathf.Abs(_homing * 5), _entityLayer);

        float shortestDis = float.MaxValue;
        GameObject closestGO = null;
        foreach (Collider2D col in cols)
        {
            float distance = Vector2.Distance(col.transform.position, transform.position);
            if (distance < shortestDis)
            {
                shortestDis = distance;
                closestGO = col.gameObject;
            }
        }
        return closestGO;
    }

    void HomeOnTarget()
    {
        if (_target == null)
        {
            _target = FindTarget();
            return;
        }

        Vector2 oldDir = _rb.velocity.normalized;
        Vector2 dir = (_target.transform.position - transform.position).normalized;
        if (_homing < 0)
            dir = Vector2.Lerp(oldDir, -dir, -_homing / 50).normalized;
        else
            dir = Vector2.Lerp(oldDir, dir, _homing / 50).normalized;

        _rb.velocity = dir * Speed;
        Dir = dir;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (_gathering)
            return;

        if (!DontCallEvents)
        {
            A_EventManager.InvokePlayerBurLodge(this, collision.gameObject);
        }

        if (_dropped && collision.gameObject.tag == "Magnet")
        {
            Gather();
            return;
        }

        if (_lodged || _dropped)
            return;

        if (collision.gameObject.tag == "Tile")
        {
            Tile tile = collision.gameObject.GetComponent<Tile>();
            if (tile is TileWall wall)
            {
                wall.TakeDamage(_dmg);
                A_EventManager.InvokePlaySFX("ShotHit");
                if (!AntiInflict)
                    wall.InflictStatus();
                Lodge(wall);
                return;
            }
        }

        if (collision.gameObject.tag == "Enemy")
        {
            Entity entity = collision.gameObject.GetComponent<Entity>();

            entity.TakeDamage(_dmg);
            A_EventManager.InvokePlaySFX("ShotHit");
            if (!AntiInflict)
                entity.InflictStatus();
            entity.KnockBack(Dir);
            Lodge(entity);

            MakeInfo? info = ItemManager.Instance.RandomDrop();
            if (info != null && !(entity is E_Dummy))
            {
                A_Factory.Instance.MakeBasic(transform.position, (MakeInfo)info);
            }

            return;
        }

        if (collision.gameObject.tag == "Border")
        {
            Drop();
            A_EventManager.InvokePlaySFX("ShotHit");
            return;
        }

        if (collision.gameObject.tag == "ITakeDamage")
        {
            ITakeDamage takeD = collision.GetComponent<ITakeDamage>();
            takeD.TakeDamage(_dmg);
            Drop();
            A_EventManager.InvokePlaySFX("ShotHit");

            MakeInfo? info = ItemManager.Instance.RandomDrop();
            if (info != null)
            {
                A_Factory.Instance.MakeBasic(transform.position, (MakeInfo)info);
            }
            return;
        }
    }

    public void Drop()
    {
        Speed = 0;
        _rb.velocity = Vector2.zero;
        _dropped = true;
    }

    public void Lodge(Entity e)
    {
        Speed = 0;
        _rb.velocity = Vector2.zero;
        _lodged = true;
        StartCoroutine(C_LodgeDamage(e));
    }

    public void Lodge(TileWall e)
    {
        Speed = 0;
        _rb.velocity = Vector2.zero;
        _lodged = true;
        StartCoroutine(C_LodgeDamage(e));
    }

    IEnumerator C_LodgeDamage(Entity e)
    {
        float elapsed = 0;
        float timer = 0;
        float dur = Player.Instance.CurStats.Piercing * 5;
        float freq = Player.Instance.CurStats.FireRate * 0.01f;

        Vector3 offset = transform.position - e.transform.position;
        while (elapsed < dur && e != null && !_gathering)
        {
            transform.position = e.transform.position + offset;
            if (timer > freq)
            {                
                e.TakeDamage(_dmg * 0.25f);
                timer = 0;
            }
            timer += Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }

        Drop();
    }

    IEnumerator C_LodgeDamage(TileWall e)
    {
        float timer = 0;
        float elapsed = 0;
        float dur = Player.Instance.CurStats.Piercing * 5;
        float freq = Player.Instance.CurStats.FireRate * 0.01f;
        Vector3 offset = transform.position - e.transform.position;
        while (elapsed < dur && e != null && !_gathering)
        {
            transform.position = e.transform.position + offset;
            if (timer > freq)
            {
                e.TakeDamage(_dmg * 0.25f);
                timer = 0;
            }
            timer += Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }

        Drop();
    }

    public void Gather()
    {
        if (_gathering)
            return;

        StartCoroutine(C_BezierMove2());
    }

    public void Deactivate()
    {
        _trail.Clear();
        PlayerKnives.Push(this);
        gameObject.SetActive(false);
    }

    [SerializeField] Vector2 _arcSize;
    [SerializeField] Vector2 _defScale;
    [SerializeField] Vector2 _collectTime;
    bool _gathering;
    IEnumerator C_BezierMove2()
    {
        _gathering = true;

        float elapsed = 0;
        Vector2 p0 = transform.position;
        float randArcSize = _arcSize.AsRange();
        float startSize = _defScale != null ? _defScale.x : transform.localScale.x;
        int flip = Random.value > 0.5f ? -1 : 1;
        float dur = _collectTime.AsRange();

        while (elapsed < dur)
        {
            Vector2 p3 = Player.Instance.transform.position;

            Vector2 dir03 = (p3 - p0).normalized;
            Vector2 dir01 = Quaternion.Euler(0, 0, 90 * flip) * dir03;
            Vector2 p1 = p0 + dir01 * randArcSize;
            Vector2 p2 = p3 + dir01 * randArcSize;

            float curved = A_Extensions.CosCurve(elapsed / dur);

            transform.position = A_Extensions.BezierCube(p0, p1, p2, p3, curved);
            curved = A_Extensions.HumpCurve(curved, startSize * 2, startSize);
            transform.localScale = curved * Vector2.one;

            elapsed += Time.deltaTime;
            yield return null;
        }

        PlayerKnives.Instance.AmmoLeft++;
        A_EventManager.InvokePlaySFX("BurGather");

        Deactivate();
    }

    private void OnDestroy()
    {
        if (PlayerKnives.Instance != null)
        {
            PlayerKnives.Instance.AmmoLeft++;
        }

        if (transform.parent == null)
            return;

        Debug.LogWarning("Knife was lost " + _lodged + " " + _dropped + " " + _gathering + " " + transform.parent.name);
    }
}
