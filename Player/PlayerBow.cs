using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBow : MonoBehaviour
{
    [SerializeField] SpriteRenderer _bulletPrefab;
    [SerializeField] SpriteRenderer _rend;
    [SerializeField] GameObject _charge;
    [SerializeField] GameObject _chargeInner;
    [SerializeField] SpriteRenderer _chargeColour;
    [SerializeField] Color _green;
    [SerializeField] Color _yellow;
    [SerializeField] Animator _anim;
    [SerializeField] GameObject _arrow;

    float _timeHeld;
    bool _shooting;

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
        _key = A_InputManager.Instance.Key("Shoot");
    }


    private void Update()
    {
        if (!Player.Instance.Stopped)
            Aim();

        transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.zero, Time.deltaTime * 15);

        if (A_LevelManager.Instance.BossLevel && A_LevelManager.Instance.SceneTime < PlayerGun.SCENE_START_DELAY_BOSS)
            return;

        if (_shooting || Player.Instance.Stopped || A_LevelManager.Instance.SceneTime < PlayerGun.SCENE_START_DELAY)
            return;

        PlayerStats stats = Player.Instance.CurStats;

        float percent = _timeHeld / (Mathf.Clamp(stats.FireRate, 1, 500) / 50);
        _anim.SetBool("Charged", percent > 0.1f);
        _arrow.SetActive(percent > 0.1f);

        _chargeInner.transform.localScale = Mathf.Lerp(0, 1, percent) * Vector2.one;
        Vector2 target = new Vector2(0.3f * Mathf.Sign(GetDir().x), 0.3f);
        _charge.transform.localPosition = Vector2.Lerp(_charge.transform.localPosition, target, Time.deltaTime * 7);
        _chargeColour.color = percent >= 1 ? _yellow : _green;

        Player.Instance.DefaultStats.RecoilMult = percent >= 0.95f ? 0.6f : 0.8f;

        if (!ShootInput())
            _timeHeld = 0;

        if (stats.FireRate < 20 && ShootInput())
        {
            AutoFire(stats);
            return;
        }

        if (ShootInput() && !_shooting)
        {
            _timeHeld += Time.deltaTime;
            return;
        }

        if (percent >= 0.9f)
        {
            StartCoroutine(C_ShootBullets(stats, true));
        }
        else if (percent >= 0.2f)
        {
            stats.BulletSpeed *= percent;
            stats.Damage *= percent;
            stats.Range = 1;
            stats.Piercing = 0;
            stats.Homing = 0;
            stats.Range *= percent * 0.75f;
            StartCoroutine(C_ShootBullets(stats, false));
        }

        _timeHeld = 0;
    }

    float _timeSinceInput = 999;
    void AutoFire(PlayerStats stats)
    {
        _timeHeld += Time.deltaTime;

        float percent = _timeHeld / (Mathf.Clamp(stats.FireRate, 1, 500) / 50);

        _timeSinceInput = ShootInputDown() ? 0 : _timeSinceInput + Time.deltaTime;

        if ((_timeSinceInput < 0.25f && percent >= 0.85f) || (ShootInput() && percent >= 1))
        {
            StartCoroutine(C_ShootBullets(stats, true));
            return;
        }
    }

    IEnumerator C_ShootBullets(PlayerStats stats, bool poisoned)
    {
        _shooting = true;
        int amount = (int)Mathf.Floor(stats.Amount);
        int c = 0;
        for (int i = 0; i < amount; i++)
        {
            if (Player.Instance.Stopped)
            {
                _shooting = false;
                _timeHeld = 0;
                yield break;
            }

            float angle = stats.AmountRange / stats.Amount * i - stats.AmountRange / 2;
            angle += stats.Spread.AsRange();
            Shoot(angle, poisoned, stats);
            float delay = Mathf.Clamp(stats.Delay, 0, stats.FireRate / 100 / stats.Amount);

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
        _timeHeld = 0;
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

    void Shoot(float angle, bool poisoned, PlayerStats stats)
    {
        Vector3 dir = Quaternion.AngleAxis(angle, Vector3.back) * GetDir();

        BulletPlayer bullet;
        if (PlayerGun.LLEnd == null)
        {
            SpriteRenderer go = Instantiate(_bulletPrefab, _arrow.transform.position + dir.normalized * 0.1f, Quaternion.identity);
            bullet = go.GetComponent<BulletPlayer>();
        }
        else
        {
            bullet = PlayerGun.Pop();
            bullet.gameObject.SetActive(true);
            bullet.transform.position = _arrow.transform.position + dir.normalized * 0.1f;
        }

        bullet.Init(dir, stats, _bulletPrefab, true);
        bullet.AntiInflict = !poisoned;

        Player.Instance.KnockBack(-dir, 0.012f);

        if (!poisoned)
            return;

        if (Vector3.Distance(transform.localPosition, Vector3.zero) < 0.1f)
            transform.localPosition -= dir * 0.1f;
    }
}
