using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPlayer : MonoBehaviour
{
    public SpriteRenderer _rend;
    [SerializeField] Rigidbody2D _rb;
    [SerializeField] ParticleSystem _burstPS;
    [SerializeField] LayerMask _targetLayer;
    [SerializeField] float _extraDissTime;
    public bool AntiInflict;
    public string _sfx;
    [SerializeField] bool _useExplosions;
    [SerializeField] GameObject _explosion;
    [SerializeField] TrailRenderer _trail;
    [SerializeField] float _trailMultiplier;
    [SerializeField] bool _pointInDirTravelling;
    [SerializeField] bool _dealShockDamage;
    [SerializeField] bool _inflictShock;
    [SerializeField] bool _dontFuckWithTheTrail;

    public BulletPlayer NextInList = null;

    public float Range;
    public float _dmg;
    public Vector2 Dir;
    public Vector3 _lastPos;
    float _piercing;
    float _homing;
    GameObject _target;
    public float Speed;

    bool _dissolving;

    float _timer;

    public bool NotDissolveUntilRange;
    public bool DontCallEvents;
    public bool NotInPool = false;
    public bool AiderbotPool = false;
    public bool LaserPool = false;
    public bool Missed = false;

    public float RemainingLifeTime => Range - _timer;

    float _professionalAch = -1;

    public bool OptimizedMode => Player.Instance.CurStats.Amount > 30;

    private void Update()
    {
        if (_dissolving)
            return;

        _timer += Time.deltaTime;
        if (_timer > Range)
        {
            Speed = 1;
            A_EventManager.InvokePlayerBulletMiss(this);
            Missed = true;
            Dissipate();
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
        if (_dissolving)
            return;

        if (_homing != 0)
        {
            HomeOnTarget();
        }
    }

    public void Init(Vector2 dir, PlayerStats stats, SpriteRenderer rend, bool useItemColors = false) => Init(dir, stats, rend.sprite, rend.color, rend.sharedMaterial, useItemColors);

    public void Init(Vector2 dir, PlayerStats stats, Sprite spr, Color color, Material mat, bool useItemColors)
    {
        A_EventManager.InvokePlaySFX(_sfx);
        A_EventManager.InvokePlayerBulletSpawn(this);
        _lastPos = transform.position;

        if (useItemColors && ItemManager.Instance.AllColors.Count > 0)
        {
            _rend.color = ItemManager.Instance.AllColors.RandomItem();
            _rend.SetAlpha(1);
        }
        else
            _rend.color = color;
        _rend.sprite = spr;
        _rend.material = mat;

        if (OptimizedMode && !_dontFuckWithTheTrail)
            _trail.enabled = false;
        else if (!_dontFuckWithTheTrail && _trail != null)
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
        _piercing = stats.Piercing;
        if (_piercing > 2)
            _professionalAch = _piercing;

        _homing = stats.Homing;

        _timer = 0;
        _dissolving = false;
        _exploded = false;
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
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, Mathf.Abs(_homing * 5), _targetLayer);

        if (OptimizedMode)
            return cols.RandomItem().gameObject;

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

    public void IncreaseSpeed(float spd)
    {
        Speed += spd;
        _rb.velocity = Dir * Mathf.Clamp(Speed, 0.5f, float.MaxValue);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_dissolving) 
            return;

        if (!DontCallEvents)
            A_EventManager.InvokePlayerBulletCollide(this, collision.gameObject);

        if (collision.gameObject.tag == "Tile")
        {
            CollideTile(collision.gameObject);
            return;
        }

        if (collision.gameObject.tag == "Enemy")
        {
            CollideEntity(collision.gameObject);
            return;
        }

        if (collision.gameObject.tag == "ITakeDamage")
        {
            CollideITake(collision.gameObject);
            return;
        }

        if (collision.gameObject.tag != "Border")
            return;

        Dissipate();
        A_EventManager.InvokePlaySFX("ShotHit");
    }

    void CollideTile(GameObject go)
    {
        Tile tile = go.GetComponent<Tile>();
        if (tile is TileWall wall)
        {
            wall.TakeDamage(_dmg);
            Dissipate();
            A_EventManager.InvokePlaySFX("ShotHit");
            if (!AntiInflict && !_inflictShock)
                wall.InflictStatus();
        }
    }

    static bool _profTriggered;
    void CollideEntity(GameObject go)
    {
        Entity entity = go.GetComponent<Entity>();

        if (entity.HitPoints > _dmg)
            _professionalAch = -1;

        entity.TakeDamage(_dmg);
        A_EventManager.InvokePlaySFX("ShotHit");
        if (_inflictShock)
            entity.InflictStatus(shockOnly: true);
        else if (!AntiInflict)
            entity.InflictStatus();
        entity.KnockBack(Dir);

        MakeInfo? info = ItemManager.Instance.RandomDrop();
        if (info != null && !(entity is E_Dummy))
        {
            A_Factory.Instance.MakeBasic(transform.position, (MakeInfo)info);
        }

        if (_piercing <= 0)
            Dissipate();
        _piercing--;

        if (_professionalAch - _piercing >= 3 && _professionalAch != -1 && !_profTriggered)
        {
            A_EventManager.InvokeUnlock("Professional");
            _profTriggered = true;
        }            

        if (_dealShockDamage)
            entity.DealShockDamage();
    }

    void CollideITake(GameObject go)
    {
        ITakeDamage takeD = go.GetComponent<ITakeDamage>();
        takeD.TakeDamage(_dmg);
        A_EventManager.InvokePlaySFX("ShotHit");

        MakeInfo? info = ItemManager.Instance.RandomDrop();
        if (info != null)
        {
            A_Factory.Instance.MakeBasic(transform.position, (MakeInfo)info);
        }

        if (_piercing <= 0)
            Dissipate();
        _piercing--;
    }

    public void Dissipate()
    {
        if (_dissolving || (NotDissolveUntilRange && _timer < Range) || A_LevelManager.Instance == null)
            return;

        if (_useExplosions && !_exploded)
        {
            Explode();
            _exploded = true;
            Deactivate();
            return;
        }

        if (OptimizedMode)
        {
            Deactivate();
            return;
        }
        
        StartCoroutine(C_Dissapate());
    }

    bool _exploded = false;
    public IEnumerator C_Dissapate()
    {
        if (!_dontFuckWithTheTrail && _trail != null)
            _trail.enabled = false;

        _dissolving = true;

        if (_burstPS != null)
        {
            var ps = Instantiate(_burstPS, transform.position, Quaternion.identity);
            ps.transform.localScale = transform.localScale;
        }        

        float elapsed = 0;
        float dur = 0.1f + _extraDissTime;

        Vector2 startSize = transform.localScale;

        _rb.velocity = Vector2.zero;
        Speed = 0;

        while (elapsed < dur)
        {
            float curved = A_Extensions.CosCurve(elapsed / dur);
            transform.localScale = Vector2.Lerp(startSize, Vector2.zero, curved);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Deactivate();
    }

    public void Deactivate()
    {
        if (NotInPool)
        {
            Destroy(gameObject);
            return;
        }

        if (AiderbotPool)
        {
            H_Aiderbot.Push(this);
            gameObject.name = "AIDERBOT";
            gameObject.SetActive(false);
            return;
        }

        if (LaserPool)
        {
            PlayerGun.PushLaser(this);
            gameObject.name = "LASER";
            gameObject.SetActive(false);
            return;
        }

        gameObject.name = "BULLET";
        _trail.Clear();
        PlayerGun.Push(this);
        gameObject.SetActive(false);
    }

    void Explode()
    {
        GameObject go = Instantiate(_explosion, transform.position, Quaternion.identity);
        go.transform.localScale = transform.localScale * 1.5f;
    }
}
