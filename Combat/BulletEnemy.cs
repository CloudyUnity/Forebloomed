using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletEnemy : MonoBehaviour
{
    [SerializeField] Rigidbody2D _rb;
    [SerializeField] ParticleSystem _burstPS;
    [SerializeField] GameObject _playerBullet;
    [SerializeField] SpriteRenderer _rend, _rend2;
    [SerializeField] Animator _anim;
    [SerializeField] TrailRenderer _trail;
    public BulletInfo SplitInfo;
    public GameObject OverrideHomingTarget;
    float _lifeTime;
    int _dmg;
    string _enemyName;
    bool _dissolving;
    public float Homing;
    public bool OnionBlocked;
    float _maxSpeed;
    bool _bouncing;
    bool _breaker;
    bool _rider;
    bool _antiCollideEvent;
    bool _customPS;

    public BulletEnemy NextInList = null;

    ParticleSystem _curPS;

    bool _reflected;

    public static readonly bool BULLET_CAP = true;

    public bool OptimizeMode => A_LevelManager.Instance.BulletCount > 100;

    public void SetDir(BulletInfo info)
    {
        A_LevelManager.Instance.BulletCount++;
        if (BULLET_CAP && A_LevelManager.Instance.BulletCount >= 1200)
        {
            Deactivate();
            return;
        }               

        _maxSpeed = info.speed.AsRange();
        if (A_LevelManager.Instance.HardMode)
            _maxSpeed *= A_LevelManager.HARD_MODE_BULLET_MULT;
        _rb.velocity = info.dir * _maxSpeed;
        _lifeTime = info.lifetime.AsRange();
        _enemyName = info.enemy;
        Homing = info.homing;
        _bouncing = info.bouncing;
        _breaker = info.breaker;
        _dmg = info.dmg;
        _rider = info.rider;
        _antiCollideEvent = info.antiCollideEvent;
        _reflected = false;
        _dissolving = false;
        _timer = 0;
        OnionBlocked = info.onionsBlocked;

        if (info.deactivateRend2)
            _rend2.enabled = false;

        float size = info.size.AsRange();
        transform.localScale = Vector2.one * size;        

        if (info.trailMultiplier == 404 || OptimizeMode)
            _trail.enabled = false;
        else
        {
            _trail.startWidth = size * (info.trailMultiplier == 0 ? 1 : info.trailMultiplier);
            if (info.customTrailColor.a != 0)
                _trail.startColor = info.customTrailColor;
            else
                _trail.startColor = A_Factory.Instance.DEFAULT_BULLET_COLOR;
        }

        if (info.customMat != null)
        {
            _rend.material = info.customMat;
            _rend2.material = info.customMat;
        }
        else
        {
            _rend.material = A_Factory.Instance.DEFAULT_BULLET_MAT;
            _rend2.material = A_Factory.Instance.DEFAULT_BULLET_MAT;
        }

        if (info.customSprite.Length > 0)
        {
            _anim.runtimeAnimatorController = null;
            Sprite sprite = info.customSprite.RandomItem();
            _rend.sprite = sprite;
            _rend2.sprite = sprite;
        }
        else
        {
            _rend.sprite = A_Factory.Instance.DEFAULT_BULLET_SPRITE;
            _rend2.sprite = A_Factory.Instance.DEFAULT_BULLET_SPRITE;
        }

        if (info.animController != null)
            _anim.runtimeAnimatorController = info.animController;

        _customPS = info.customPS != null;
        _curPS = _customPS ? info.customPS : _burstPS;

        if (info.enemy == null)
            Debug.Log("Missing Name!");

        A_EventManager.InvokeBulletEnemySpawn(this);
    }

    public void ModifyStats(BulletInfo info)
    {
        if (info.speed != Vector2.zero)
        {
            float speedInc = info.speed.AsRange();
            _rb.velocity *= speedInc;
            _maxSpeed *= speedInc;
        }

        _dmg += info.dmg;
        _lifeTime += info.lifetime.AsRange();

        if (info.homing < 0 && Homing > 0)
            Homing *= 0.75f;

        Homing += info.homing;
    }

    float _timer;
    private void Update()
    {
        if (_dissolving)
            return;

        transform.right = _rb.velocity;

        _timer += Time.deltaTime;
        if (_timer > _lifeTime)
        {
            Dissipate();
        }
    }

    private void FixedUpdate()
    {
        if (_dissolving)
            return;

        if (Homing != 0)
            HomeOnTarget();
    }

    void HomeOnTarget()
    {
        if (Homing < 0 && OnionBlocked)
            return;

        Vector2 dir;

        if (OverrideHomingTarget != null)
        {
            dir = (OverrideHomingTarget.transform.position - transform.position).normalized;
        }
        else
            dir = (Player.Instance.HitBox - transform.position).normalized;

        Vector2 oldDir = _rb.velocity.normalized;
        if (Homing < 0)
            dir = Vector2.Lerp(oldDir, -dir, -Homing / 50).normalized;
        else
            dir = Vector2.Lerp(oldDir, dir, Homing / 50).normalized;
        _rb.velocity = dir * _maxSpeed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_dissolving)
            return;

        if (collision.gameObject.tag == "Player")
        {
            A_EventManager.InvokeDealDamage(_dmg, transform.position, _enemyName);
            _timer = 99;
            Dissipate();
            A_EventManager.InvokePlaySFX("ShotHit");
            return;
        }

        if (collision.gameObject.tag == "Tile" && _rider)
        {
            A_EventManager.InvokePlaySFX("Bounce");
            if (transform.position.y > collision.bounds.max.y || transform.position.y < collision.bounds.min.y)
            {
                int sign = transform.position.y > collision.transform.position.y ? 1 : -1;
                transform.position += new Vector3(0, 0.1f * sign);
                _rb.velocity = new Vector2(A_Extensions.RandomizeSign(_maxSpeed), 0);
                return;
            }

            int sign2 = transform.position.x > collision.transform.position.x ? 1 : -1;
            transform.position += new Vector3(0.1f * sign2, 0);
            _rb.velocity = new Vector2(0, A_Extensions.RandomizeSign(_maxSpeed));
            return;
        }

        if (collision.gameObject.tag == "Tile" && _breaker)
        {
            A_EventManager.InvokePlaySFX("ShotHit");
            collision.GetComponent<TileWall>().TakeDamage(50);
            Dissipate();
            return;
        }

        if (collision.gameObject.tag == "Tile" && _bouncing)
        {
            A_EventManager.InvokePlaySFX("Bounce");
            _rb.velocity = new Vector2(-_rb.velocity.y, _rb.velocity.x);
            return;
        }      

        if (collision.gameObject.tag.Is("Tile", "Border"))
        {
            A_EventManager.InvokePlaySFX("ShotHit");
            Dissipate();
            return;
        }

        if (collision.gameObject.tag == "Orbital")
        {
            A_EventManager.InvokePlaySFX("ShotHit");
            _timer = 99;
            Dissipate();
            return;
        }

        if (collision.gameObject.tag == "Blocker")
        {
            A_EventManager.InvokePlaySFX("ShotHit");
            collision.GetComponent<ITakeDamage>().TakeDamage(_dmg);
            _timer = 99;
            Dissipate();
            return;
        }
    }

    public void Dissipate()
    {
        if (_antiCollideEvent && _timer < _lifeTime)
            return;

        if (_dissolving || !gameObject.activeSelf)
            return;

        if (OptimizeMode)
        {
            Deactivate();
            return;
        }

        StartCoroutine(C_Dissapate());
    }

    public IEnumerator C_Dissapate()
    {
        _dissolving = true;

        float elapsed = 0;
        float dur = 0.1f;

        Vector2 startSize = transform.localScale;

        var ps = Instantiate(_curPS, transform.position, Quaternion.identity);
        ps.transform.localScale = transform.localScale;

        _trail.enabled = false;

        while (elapsed < dur)
        {
            float curved = A_Extensions.CosCurve(elapsed / dur);
            transform.localScale = Vector2.Lerp(startSize, Vector2.zero, curved);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        Deactivate();
    }

    public void ReverseDir()
    {
        if (_reflected)
            return;

        _reflected = true;
        _rb.velocity *= -1;
    }

    public void ChangeToPlayer(float dmg)
    {
        BulletPlayer bullet;
        if (PlayerGun.LLEnd == null)
        {
            GameObject go = Instantiate(_playerBullet, transform.position, Quaternion.identity);
            bullet = go.GetComponent<BulletPlayer>();
        }
        else
        {
            bullet = PlayerGun.Pop();
            bullet.gameObject.SetActive(true);
            bullet.transform.position = transform.position;
        }

        PlayerStats stats = new PlayerStats
        {
            Damage = dmg,
            BulletSize = transform.localScale.x,
            Homing = Homing,
            Range = _lifeTime - _timer,
            BulletSpeed = _maxSpeed / 3,
        };

        bullet.Init(_rb.velocity, stats, _rend);
        Deactivate();
    }

    public void Deactivate()
    {
        A_LevelManager.Instance.BulletCount--;
        if (SplitInfo.amount != 0)
        {
            A_Factory.Instance.MakeEnemyBullet(transform.position, SplitInfo);
            A_EventManager.InvokePlaySFX("Split");
        }

        BulletEnemyCache.Push(this);
        SplitInfo = default(BulletInfo);
        OverrideHomingTarget = null;
        gameObject.SetActive(false);
    }
}

[System.Serializable]
public struct BulletInfo
{
    public string enemy;
    public Vector2 dir;
    public Vector2 speed;
    public int dmg;
    public float spread;
    public Vector2 lifetime;
    public Vector2 size;
    public int amount;
    public float homing;
    public bool bouncing;
    public bool breaker;
    public bool rider;
    public bool antiCollideEvent;
    public bool deactivateRend2;
    public bool onionsBlocked;

    public float trailMultiplier;
    public Color customTrailColor;
    public Sprite[] customSprite;
    public Material customMat;
    public RuntimeAnimatorController animController;
    public ParticleSystem customPS;
}
