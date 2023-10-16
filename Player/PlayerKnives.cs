using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerKnives : MonoBehaviour
{
    public static PlayerKnives Instance;

    float _bulletTimer;
    [SerializeField] GameObject _bulletPrefab;
    public SpriteRenderer[] _rends;

    public int AmmoLeft = 5;

    bool _shooting;

    float _timeSinceInput = 999;

    public static B_Knife LLEnd = null;
    public static B_Knife LLStart = null;

    float _lastAmount;

    KeyCode _key;

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

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _key = A_InputManager.Instance.Key("Shoot");

        AmmoLeft = (int)Player.Instance.CurStats.Amount;
        _lastAmount = AmmoLeft;
    }

    private void Update()
    {
        if (Player.Instance.CurStats.Amount != _lastAmount)
            AmmoLeft += (int)(Player.Instance.CurStats.Amount - _lastAmount);
        _lastAmount = Player.Instance.CurStats.Amount;

        if (Player.Instance.Stopped)
            return;

        Aim();
        for (int i = 0; i < 5; i++)
        {
            _rends[i].enabled = i < AmmoLeft;
        }

        if (A_LevelManager.Instance.BossLevel && A_LevelManager.Instance.SceneTime < PlayerGun.SCENE_START_DELAY_BOSS)
            return;

        if (_shooting || A_LevelManager.Instance.SceneTime < PlayerGun.SCENE_START_DELAY)
            return;

        _bulletTimer += Time.deltaTime;
        PlayerStats stats = Player.Instance.CurStats;
        float fireRate = Mathf.Clamp(stats.FireRate, 1, 500) / 100;

        _timeSinceInput = ShootInputDown() ? 0 : _timeSinceInput + Time.deltaTime;

        if (_timeSinceInput < 0.25f && _bulletTimer > fireRate * 0.85f && AmmoLeft > 0)
        {
            float angle = stats.AmountRange / stats.Amount - stats.AmountRange / 2;
            angle += stats.Spread.AsRange();
            Shoot(angle);
            _bulletTimer = 0;
            _timeSinceInput = 99;
            return;
        }
    }

    void Aim()
    {
        float rot = GetDir().x < 0 ? 180 : 0;
        transform.rotation = Quaternion.Euler(rot, 0, rot);
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
        B_Knife bullet;
        if (LLEnd == null)
        {
            GameObject go = Instantiate(_bulletPrefab, transform.position, Quaternion.identity);
            bullet = go.GetComponent<B_Knife>();
        }
        else
        {
            bullet = Pop();
            bullet.gameObject.SetActive(true);
            bullet.transform.position = transform.position;
        }

        Vector2 dir = Quaternion.AngleAxis(angle, Vector3.back) * GetDir();

        PlayerStats stats = Player.Instance.CurStats;
        bullet.Init(dir, stats);

        AmmoLeft--;

        Player.Instance.KnockBack(dir, 0.005f);
    }

    public static void Push(B_Knife bullet)
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

    public static B_Knife Pop()
    {
        B_Knife start = LLStart;
        LLStart = LLStart.NextInList;
        if (LLEnd == start)
            LLEnd = null;
        return start;
    }
}
