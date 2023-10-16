using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGun : MonoBehaviour
{
    float _bulletTimer;
    [SerializeField] SpriteRenderer _bulletPrefab;
    [SerializeField] SpriteRenderer _rend;
    [SerializeField] ParticleSystem _ps;
    [SerializeField] ParticleSystemRenderer _psRend;
    [SerializeField] ParticleSystem _clipPS;
    [SerializeField] PlayerStats _overrideStats;
    [SerializeField] bool _overrideStatsEnabled;
    [SerializeField] bool _kickBackOff;
    [SerializeField] bool _knockBackOff;
    [SerializeField] bool _laserGun;

    public static BulletPlayer LLEnd = null;
    public static BulletPlayer LLStart = null;
    public static BulletPlayer LaserLLEnd = null;
    public static BulletPlayer LaserLLStart = null;

    public static readonly int MAX_SHOTS_PER_FRAME = 20;

    public static readonly float SCENE_START_DELAY = 0.25f;
    public static readonly float SCENE_START_DELAY_BOSS = 1.5f;

    bool _shooting;
    bool _knocked;
    float _timeSinceInput = 999;
    int _knockbacksThisFrame = 0;

    Vector2 NormalSize;
    KeyCode _key;

    private void OnEnable()
    {
        A_EventManager.OnDestroyWeapons += DestroySelf;
    }

    private void OnDisable()
    {
        A_EventManager.OnDestroyWeapons -= DestroySelf;
    }
    void DestroySelf() => Destroy(gameObject);

    private void Start()
    {
        LLEnd = null;
        LLStart = null;

        NormalSize = transform.localScale;
        _key = A_InputManager.Instance.Key("Shoot");
    }

    bool ShootInput() => Input.GetKey(_key) || A_InputManager.Instance.GamepadShoot();
    bool ShootInputDown() => Input.GetKeyDown(_key) || A_InputManager.Instance.GamepadShootDown();

    private void Update()
    {
        if (!_knocked && !Player.Instance.Stopped)
            Aim();

        if (!_kickBackOff)
            transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(0.025f, -0.13f), Time.deltaTime * 15);

        if (A_LevelManager.Instance.BossLevel && A_LevelManager.Instance.SceneTime < SCENE_START_DELAY_BOSS)
            return;

        if (Player.Instance.Stopped || _shooting || A_LevelManager.Instance.SceneTime < SCENE_START_DELAY)
            return;        

        _bulletTimer += Time.deltaTime;
        PlayerStats stats = Player.Instance.CurStats;
        float fireRate = Mathf.Clamp(stats.FireRate, 1, 500) / 100;

        _timeSinceInput = ShootInputDown() ? 0 : _timeSinceInput + Time.deltaTime;

        bool readyToShoot = (_timeSinceInput < 0.25f && _bulletTimer > fireRate * 0.85f) || (ShootInput() && _bulletTimer > fireRate);
        if (readyToShoot)
        {
            _knocked = false;

            StartCoroutine(C_ShootBullets(stats));            
            return;
        }
    }

    IEnumerator C_ShootBullets(PlayerStats stats)
    {
        _shooting = true;
        _knockbacksThisFrame = 0;
        int amount = (int)Mathf.Floor(stats.Amount);
        int c = 0;

        for (int i = 0; i < amount; i++)
        {
            if (Player.Instance.Stopped)
            {
                _shooting = false;
                _bulletTimer = 0;
                _timeSinceInput = 999;
                break;
            }

            float angle = stats.AmountRange / stats.Amount * i - stats.AmountRange / 2;
            angle += stats.Spread.AsRange();
            Shoot(angle);
            float delay = Mathf.Clamp(stats.Delay, 0, (stats.FireRate / 100) / stats.Amount);

            if (!ShootInput() && stats.Delay > 0.01f)
            {
                _shooting = false;
                _bulletTimer = 0;
                _timeSinceInput = 999;
                break;
            }

            c++;
            if (c > MAX_SHOTS_PER_FRAME)
            {
                c = 0;
                yield return null;
            }

            if (delay > 0 && i != amount - 1)
                yield return new WaitForSeconds(delay);
        }
        _shooting = false;
        _bulletTimer = 0;
        _timeSinceInput = 999;
        if (stats.Delay == 0)
            StartCoroutine(C_KnockGun());
    }

    void Aim()
    {
        Vector2 dir = GetDir();
        transform.right = dir;

        FlipGun();

        _rend.sortingOrder = dir.y <= 0 ? 1002 : 998;
    }

    void FlipGun()
    {
        if (transform.right.x > 0)
            return;

        Vector3 rot = transform.rotation.eulerAngles;
        if (rot.y != 180)
            rot.x = 180;
        rot.z *= -1;
        transform.rotation = Quaternion.Euler(rot);
    }

    Vector2 GetDir()
    {
        if (A_InputManager.GamepadMode)
            return (V_GamepadCursor.CursorPos - (Vector2)Player.Instance.transform.position).normalized;

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.nearClipPlane;
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(mousePos);
        return (worldPosition - (Vector2)Player.Instance.transform.position).normalized;
    }

    void Shoot(float angle)
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.nearClipPlane;
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(mousePos);

        if (A_InputManager.GamepadMode)
            worldPosition = V_GamepadCursor.CursorPos;

        BulletPlayer bullet;

        if (_laserGun)
        {
            if (LaserLLEnd == null)
            {
                SpriteRenderer go = Instantiate(_bulletPrefab, _ps.transform.position, Quaternion.identity);
                bullet = go.GetComponent<BulletPlayer>();
            }
            else
            {
                bullet = PopLaser();
                bullet.transform.position = _ps.transform.position;
                bullet.gameObject.SetActive(true);
            }
        }

        else if (LLEnd == null)
        {
            SpriteRenderer go = Instantiate(_bulletPrefab, _ps.transform.position, Quaternion.identity);
            bullet = go.GetComponent<BulletPlayer>();
        }
        else
        {
            bullet = Pop();
            bullet.transform.position = _ps.transform.position;
            bullet.gameObject.SetActive(true);
        }        

        Vector2 dir = (worldPosition - (Vector2)_ps.transform.position).normalized;
        if (Vector2.Distance(worldPosition, Player.Instance.transform.position) < 0.7f)
            dir = GetDir();

        dir = Quaternion.AngleAxis(angle, Vector3.back) * dir;

        PlayerStats stats = _overrideStatsEnabled ? _overrideStats : Player.Instance.CurStats;
        bullet.Init(dir, stats, _bulletPrefab, true);

        _ps.Play();
        _clipPS.Emit(1);
        Squash(0.15f);

        if (!_knockBackOff && _knockbacksThisFrame < 3)
        {
            Player.Instance.KnockBack(-dir, 0.008f);
            _knockbacksThisFrame++;
        }        

        if (!_kickBackOff && Vector3.Distance(transform.localPosition, new Vector3(0.025f, -0.13f)) < 0.1f)
            transform.localPosition -= (Vector3)dir * 0.1f;
    }

    IEnumerator C_KnockGun()
    {
        float elapsed = 0;
        float firerate = Player.Instance.CurStats.FireRate;
        float dur = firerate / 200;

        if (dur <= 0.05f) 
            yield break;

        if (dur > 0.4f) 
            dur = 0.4f;

        _knocked = true;

        Vector2 startRot = transform.right;
        float angle = -firerate * Mathf.Sign(startRot.x);
        Vector2 endRot = Quaternion.AngleAxis(angle / 2, Vector3.back) * startRot;

        while (elapsed < dur && !Input.GetKey(_key) && _bulletTimer <= firerate)
        {
            float curved = A_Extensions.SlowingCurve(elapsed / dur);
            transform.right = Vector2.Lerp(startRot, endRot, curved);
            FlipGun();
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0;
        while (elapsed < 0.1f)
        {
            transform.right = Vector2.Lerp(endRot, GetDir(), elapsed / dur);
            FlipGun();
            elapsed += Time.deltaTime;
            yield return null;
        }

        _knocked = false;
    }

    bool _squashing;
    public void Squash(float dur) => StartCoroutine(C_SqaushStretch(dur));
    IEnumerator C_SqaushStretch(float dur)
    {
        if (_squashing || NormalSize == Vector2.zero)
            yield break;

        _squashing = true;

        float elapsed = 0;

        while (elapsed < dur)
        {
            float humped = 1 - A_Extensions.HumpCurve(elapsed / dur, 0, 1);
            transform.localScale = NormalSize + new Vector2(0.3f * -humped, 0.1f * humped);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = NormalSize;
        _squashing = false;
    }

    public static void Push(BulletPlayer bullet)
    {
        if (LLEnd == null)
        {
            LLEnd = bullet;
            LLStart = bullet;
            return;
        }

        LLEnd.NextInList = bullet;
        LLEnd = bullet;
    }

    public static BulletPlayer Pop()
    {
        BulletPlayer start = LLStart;
        LLStart = LLStart.NextInList;
        if (LLEnd == start)
            LLEnd = null;
        return start;
    }

    public static void PushLaser(BulletPlayer bullet)
    {
        if (LaserLLEnd == null)
        {
            LaserLLEnd = bullet;
            LaserLLStart = bullet;
            return;
        }

        LaserLLEnd.NextInList = bullet;
        LaserLLEnd = bullet;
    }

    public static BulletPlayer PopLaser()
    {
        BulletPlayer start = LaserLLStart;
        LaserLLStart = LaserLLStart.NextInList;
        if (LaserLLEnd == start)
            LaserLLEnd = null;
        return start;
    }
}
