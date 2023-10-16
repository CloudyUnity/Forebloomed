using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBoomerang : MonoBehaviour
{
    public static PlayerBoomerang Instance;

    float _bulletTimer;
    [SerializeField] GameObject _bulletPrefab;
    [SerializeField] SpriteRenderer _rend;

    public Transform RendTransform => _rend.transform;

    bool _shooting;

    float _timeSinceInput = 999;

    public float BoomerangsOut = 0;

    public static Boomerang LLEnd = null;
    public static Boomerang LLStart = null;

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

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _key = A_InputManager.Instance.Key("Shoot");
    }

    private void Update()
    {
        _rend.enabled = BoomerangsOut == 0;

        if (!Player.Instance.Stopped)
            Aim();

        if (A_LevelManager.Instance.BossLevel && A_LevelManager.Instance.SceneTime < PlayerGun.SCENE_START_DELAY_BOSS)
            return;

        if (Player.Instance.Stopped || _shooting || A_LevelManager.Instance.SceneTime < PlayerGun.SCENE_START_DELAY)
            return;

        _bulletTimer += Time.deltaTime;
        PlayerStats stats = Player.Instance.CurStats;
        float fireRate = Mathf.Clamp(stats.FireRate, 5, 500) / 100;        

        _rend.transform.localScale = Vector2.one * stats.BulletSize / stats.Size;
        _timeSinceInput = (Input.GetKeyDown(_key) || A_InputManager.Instance.GamepadShootDown()) ? 0 : _timeSinceInput + Time.deltaTime;

        if ((_timeSinceInput < 0.25f && _bulletTimer > fireRate * 0.85f) || ((Input.GetKey(_key) || A_InputManager.Instance.GamepadShoot()) && _bulletTimer > fireRate))
        {
            StartCoroutine(C_ShootBullets(stats));
            return;
        }
    }

    IEnumerator C_ShootBullets(PlayerStats stats)
    {
        _shooting = true;
        int amount = (int)Mathf.Floor(stats.Amount);
        int c = 0;
        for (int i = 0; i < amount; i++)
        {
            if (Player.Instance.Stopped)
            {
                break;
            }

            float angle = stats.AmountRange / stats.Amount * i - stats.AmountRange / 2;
            angle += stats.Spread.AsRange();
            Shoot(angle);
            float delay = Mathf.Clamp(stats.Delay, 0, stats.FireRate / 100 / stats.Amount);

            if (!Input.GetKey(_key) && stats.Delay > 0.01f)
            {
                _shooting = false;
                _bulletTimer = 0;
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

        _timeSinceInput = 999;
        while (BoomerangsOut != 0)
            yield return null;
        _shooting = false;
        _bulletTimer = Mathf.Clamp(stats.FireRate, 5, 500) / 150;
    }

    void Aim()
    {
        Vector2 dir = GetDir();
        transform.right = dir;

        _rend.sortingOrder = dir.y <= 0 ? 1002 : 998;
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

    public void Shoot(float angle)
    {
        Boomerang bullet;
        if (LLEnd == null)
        {
            GameObject go = Instantiate(_bulletPrefab, _rend.transform.position, Quaternion.identity);
            bullet = go.GetComponent<Boomerang>();
        }
        else
        {
            bullet = Pop();
            bullet.gameObject.SetActive(true);
            bullet.transform.position = _rend.transform.position;
        }

        Vector2 dir = Quaternion.AngleAxis(angle, Vector3.back) * GetDir();

        PlayerStats stats = Player.Instance.CurStats;
        bullet.AddForce(dir, stats, _rend.transform, this);

        BoomerangsOut++;

        Player.Instance.KnockBack(dir, 0.005f);
    }

    public static void Push(Boomerang bullet)
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

    public static Boomerang Pop()
    {
        Boomerang start = LLStart;
        LLStart = LLStart.NextInList;
        if (LLEnd == start)
            LLEnd = null;
        return start;
    }
}
