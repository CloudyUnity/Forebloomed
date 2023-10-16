using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSword : MonoBehaviour
{
    [SerializeField] GameObject _slash;
    [SerializeField] GameObject _swordUp;
    [SerializeField] GameObject _swordDown;
    [SerializeField] SpriteRenderer _s1, _s2;
    float _timer;
    bool _flippedSword;
    bool _shooting;
    float _timeSinceInput = 999;

    public static B_Slash LLEnd = null;
    public static B_Slash LLStart = null;

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

    private void Start()
    {
        _key = A_InputManager.Instance.Key("Shoot");
    }
    void DestroySelf() => Destroy(gameObject);

    Vector2 GetDir()
    {
        if (A_InputManager.GamepadMode)
            return (V_GamepadCursor.CursorPos - (Vector2)Player.Instance.transform.position).normalized;

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.nearClipPlane;
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(mousePos);
        return (worldPosition - (Vector2)Player.Instance.transform.position).normalized;
    }

    private void Update()
    {
        if (Player.Instance.Stopped)
            return;

        Vector2 dir = GetDir();
        transform.right = dir;
        _swordUp.SetActive(_flippedSword);
        _swordDown.SetActive(!_flippedSword);

        _s1.sortingOrder = dir.x > 0 ? 998 : 1002;
        _s2.sortingOrder = dir.x < 0 ? 998 : 1002;

        if (A_LevelManager.Instance.BossLevel && A_LevelManager.Instance.SceneTime < PlayerGun.SCENE_START_DELAY_BOSS)
            return;

        if (Player.Instance.Stopped || _shooting || A_LevelManager.Instance.SceneTime < PlayerGun.SCENE_START_DELAY)
            return;

        _timer += Time.deltaTime;
        float fireRate = Mathf.Clamp(Player.Instance.CurStats.FireRate, 5, 300) / 300;

        _timeSinceInput = ShootInput() ? 0 : _timeSinceInput + Time.deltaTime;

        if (_timeSinceInput < 0.25f && _timer > fireRate)
        {
            StartCoroutine(C_SliceALL());
        }
    }

    IEnumerator C_SliceALL()
    {
        _shooting = true;
        PlayerStats stats = Player.Instance.CurStats;
        int amount = (int)Mathf.Floor(stats.Amount);
        int c = 0;
        for (int i = 0; i < amount; i++)
        {
            if (Player.Instance.Stopped)
            {
                _shooting = false;
                _timer = 0;
                _timeSinceInput = 999;
                yield break;
            }

            float angle = stats.AmountRange / stats.Amount * i - stats.AmountRange / 2;
            angle += stats.Spread.AsRange();
            SliceAt(angle);
            _flippedSword = !_flippedSword;
            float delay = Mathf.Clamp(stats.Delay, 0, stats.FireRate / 500 / stats.Amount);

            if (!ShootInput() && stats.Delay > 0.01f)
            {
                _shooting = false;
                _timer = 0;
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
        _timer = 0;
        _timeSinceInput = 999;
    }

    void SliceAt(float angle)
    {
        Vector3 dir = Quaternion.AngleAxis(angle, Vector3.back) * GetDir();
        float range = Player.Instance.CurStats.Range / 2.5f;
        range = Mathf.Clamp(range, 1, 2.5f);
        Vector3 pos = Player.Instance.transform.position + dir * range;

        B_Slash bullet;
        if (LLEnd == null)
        {
            GameObject go = Instantiate(_slash, pos, Quaternion.identity);
            bullet = go.GetComponent<B_Slash>();
        }
        else
        {
            bullet = Pop();
            bullet.gameObject.SetActive(true);
            bullet.transform.position = pos;
        }

        bullet.transform.up = dir;
        bullet.transform.localScale = Player.Instance.CurStats.BulletSize * Vector2.one * 6;

        Player.Instance.KnockBack(dir, 0.01f);

        float dmg = Player.Instance.CurStats.Damage;
        bullet.Slice(dmg, dir);
    }

    public static void Push(B_Slash bullet)
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

    public static B_Slash Pop()
    {
        B_Slash start = LLStart;
        LLStart = LLStart.NextInList;
        if (LLEnd == start)
            LLEnd = null;
        return start;
    }
}
