using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLauncher : MonoBehaviour
{
    float _bulletTimer;
    [SerializeField] SpriteRenderer _bulletPrefab;
    [SerializeField] SpriteRenderer _rocketPrefab;
    [SerializeField] SpriteRenderer _rend;
    [SerializeField] ParticleSystem _ps;
    [SerializeField] ParticleSystemRenderer _psRend;
    [SerializeField] ParticleSystem _clipPS;
    bool _shooting;

    bool _knocked;

    P_Jack _jack;
    int _knockbacksThisFrame = 0;

    Vector2 NormalSize;
    float _timeSinceInput = 999;

    KeyCode _key;

    bool ShootInput() => Input.GetKey(_key) || A_InputManager.Instance.GamepadShoot();
    bool ShootInputDown() => Input.GetKeyDown(_key) || A_InputManager.Instance.GamepadShootDown();

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
        if (Player.Instance is P_Jack jack)
            _jack = jack;

        _key = A_InputManager.Instance.Key("Shoot");

        NormalSize = transform.localScale;
    }

    private void Update()
    {
        if (!_knocked && !Player.Instance.Stopped)
            Aim();

        transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(0.025f, -0.13f), Time.deltaTime * 15);

        if (A_LevelManager.Instance.BossLevel && A_LevelManager.Instance.SceneTime < PlayerGun.SCENE_START_DELAY_BOSS)
            return;

        if (Player.Instance.Stopped || _shooting || A_LevelManager.Instance.SceneTime < PlayerGun.SCENE_START_DELAY)
            return;

        _bulletTimer += Time.deltaTime;
        PlayerStats stats = Player.Instance.CurStats;
        float fireRate = Mathf.Clamp(stats.FireRate, 1, 500) / 100;

        bool abilityInput = Input.GetKeyDown(A_InputManager.Instance.Key("Ability")) || A_InputManager.Instance.GamepadAbility();
        if (_jack != null && abilityInput && _jack.HasAmmo)
        {
            ShootRocket();
            _jack.UseAllAmmo();
            return;
        }

        _timeSinceInput = ShootInputDown() ? 0 : _timeSinceInput + Time.deltaTime;

        if ((_timeSinceInput < 0.25f && _bulletTimer > fireRate) || (ShootInput() && _bulletTimer > fireRate))
        {
            StopCoroutine(C_KnockGun());
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
                yield break;
            }

            float angle = stats.AmountRange / stats.Amount * i - stats.AmountRange / 2;
            angle += stats.Spread.AsRange();
            Shoot(angle);
            float delay = Mathf.Clamp(stats.Delay, 0, stats.FireRate / 100 / stats.Amount);

            if (!ShootInput() && stats.Delay > 0.01f)
            {
                _shooting = false;
                _bulletTimer = 0;
                _timeSinceInput = 999;
                break;
            }

            c++;
            if (c > PlayerGun.MAX_SHOTS_PER_FRAME)
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

        _rend.sortingOrder = dir.y <= 0 ? 1002 : 997;
    }

    void FlipGun()
    {
        if (transform.right.x >= 0) return;

        Vector3 rot = transform.rotation.eulerAngles;
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
        if (PlayerGun.LLEnd == null)
        {
            SpriteRenderer go = Instantiate(_bulletPrefab, _ps.transform.position, Quaternion.identity);
            bullet = go.GetComponent<BulletPlayer>();
        }
        else
        {
            bullet = PlayerGun.Pop();
            bullet.gameObject.SetActive(true);
            bullet.transform.position = _ps.transform.position;
        }

        Vector2 dir = (worldPosition - (Vector2)_ps.transform.position).normalized;
        if (Vector2.Distance(worldPosition, Player.Instance.transform.position) < 0.7f)
            dir = GetDir();

        dir = Quaternion.AngleAxis(angle, Vector3.back) * dir;

        PlayerStats stats = Player.Instance.CurStats;
        bullet.Init(dir, stats, _bulletPrefab, true);

        _ps.Play();
        _clipPS.Emit(1);
        Squash(0.1f);

        if (_knockbacksThisFrame < 3)
        {
            Player.Instance.KnockBack(-dir, 0.015f);
            _knockbacksThisFrame++;
        }

        if (Vector3.Distance(transform.localPosition, new Vector3(0.025f, -0.13f)) < 0.1f)
            transform.localPosition -= (Vector3)dir * 0.1f;
    }

    void ShootRocket()
    {
        SpriteRenderer go = Instantiate(_rocketPrefab, _ps.transform.position, Quaternion.identity);
        BulletPlayer bullet = go.GetComponent<BulletPlayer>();

        PlayerStats stats = Player.Instance.CurStats;
        int ammo = _jack.Ammo + _jack.AbilityUpgrades;

        A_EventManager.InvokePlaySFX("Rocket");

        stats.BulletSpeed *= ammo;
        stats.Damage *= ammo + 1;
        stats.Piercing = 0;
        stats.BulletSize *= ammo;
        bullet.gameObject.transform.localScale *= ammo;
        stats.Range = 2;
        bullet.NotInPool = true;

        Vector3 dir = GetDir().normalized;
        bullet.Init(dir, stats, _rocketPrefab, true);

        Player.Instance.KnockBack(-dir, 0.02f);

        Squash(0.3f);

        _ps.Play();
        _clipPS.Emit(1);

        if (Vector3.Distance(transform.localPosition, new Vector3(0.025f, -0.13f)) < 0.3f)
            transform.localPosition -= dir * 0.3f;
    }

    IEnumerator C_KnockGun()
    {
        float elapsed = 0;
        float firerate = Player.Instance.CurStats.FireRate;
        float dur = firerate / 100 / 2;

        if (dur <= 0.05f) yield break;
        if (dur > 0.4f) dur = 0.4f;

        _knocked = true;

        Vector2 startRot = transform.right;
        float angle = -firerate * Mathf.Sign(startRot.x);
        Vector2 endRot = Quaternion.AngleAxis(angle / 2, Vector3.back) * startRot;

        while (elapsed < dur && !ShootInput() && _bulletTimer <= firerate)
        {
            float curved = A_Extensions.SlowingCurve(elapsed / dur);
            transform.right = Vector2.Lerp(startRot, endRot, curved);
            FlipGun();
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0;
        Vector2 dir = GetDir();
        while (elapsed < 0.1f)
        {
            transform.right = Vector2.Lerp(endRot, dir, elapsed / dur);
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
}
