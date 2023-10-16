using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class H_Aiderbot : MonoBehaviour
{
    [SerializeField] LayerMask _targetLayers;
    [SerializeField] SpriteRenderer _bullet;
    [SerializeField] Animator _anim, _eyeAnim;
    [SerializeField] GameObject _pointer;
    [SerializeField] SpriteRenderer _pointRend;
    [SerializeField] Sprite _pointOn, _pointOff;
    [SerializeField] ParticleSystem _ps, _clipPS, _lightningBurstPS;
    [SerializeField] float _chargeCooldown;
    float _chargeTimer = 999;
    GameObject _targetMove;
    GameObject _targetShoot;
    bool _shooting;

    public static BulletPlayer LLEnd = null;
    public static BulletPlayer LLStart = null;
    int _distance;

    float _timer;

    bool _disableWallAttacks = false;

    public static readonly bool CURSOR_TARGET_MODE = true;

    private void OnEnable()
    {
        A_EventManager.OnBossSpawn += SetBossMode;
    }

    private void OnDisable()
    {
        A_EventManager.OnBossSpawn -= SetBossMode;
    }

    private void Start()
    {
        A_LevelManager.Instance.AiderbotCount++;
        if (A_LevelManager.Instance.AiderbotCount >= 7)
            A_EventManager.InvokeUnlock("Playing Army");

        _distance = 3;
    }

    bool chargedLastFrame;
    private void Update()
    {
        bool charged = _chargeTimer <= _chargeCooldown;
        _eyeAnim.SetBool("HasCharge", charged);
        _anim.SetBool("Walking", _isMoving);

        float dis = Vector2.Distance(transform.position, Player.Instance.transform.position);
        if (dis > 15)
        {
            transform.position = Player.Instance.transform.position;
        }        

        Pointer();

        _chargeTimer += Time.deltaTime;

        if (!charged && chargedLastFrame)
            A_EventManager.InvokePlaySFX("Bot Die");
        chargedLastFrame = charged;

        if (P_Springleaf._overclocked)
            _chargeTimer = 0;

        if (Player.Instance.Stopped || A_LevelManager.Instance.SceneTime < PlayerGun.SCENE_START_DELAY)
            return;

        Transform walkTarget = _targetMove != null && charged ? _targetMove.transform : Player.Instance.transform;
        Vector3 shoot = _targetShoot != null ? _targetShoot.transform.position : walkTarget.position;
        Vector3 mid = Vector2.Lerp(walkTarget.position, shoot, 0.5f);
        if (!_isMoving)
            StartCoroutine(C_MoveOneNode(1 / Player.Instance.CurStats.Speed + 0.05f.AsRange(), mid));

        TryGetTarget();

        if (!charged)
            return;

        _timer += Time.deltaTime;

        float fr = Player.Instance.CurStats.FireRate / 100;
        fr *= Mathf.Clamp(_chargeTimer / _chargeCooldown, 0.5f, 1);

        if (_targetShoot == null || _timer < fr || _shooting)
            return;

        StartCoroutine(C_ShootBullets(_targetShoot.transform.position));
        _eyeAnim.gameObject.SetActive(_targetShoot.transform.position.y < transform.position.y);
    }

    void Shoot(Vector2 dir)
    {
        BulletPlayer bullet;
        if (LLEnd == null)
        {
            SpriteRenderer go = Instantiate(_bullet, transform.position, Quaternion.identity);
            bullet = go.GetComponent<BulletPlayer>();
            bullet.AiderbotPool = true;
        }
        else
        {
            bullet = Pop();
            bullet.gameObject.SetActive(true);
            bullet.transform.position = transform.position;
            bullet.AiderbotPool = true;
        }

        bullet.Init(dir, Player.Instance.CurStats, _bullet);

        _ps.Play();
        _clipPS.Emit(1);
    }

    IEnumerator C_ShootBullets(Vector3 target)
    {
        _shooting = true;
        PlayerStats stats = Player.Instance.CurStats;
        Squash(0.1f);
        int amount = (int)Mathf.Floor(stats.Amount);
        int c = 0;
        for (int i = 0; i < amount; i++)
        {
            float angle = stats.AmountRange / stats.Amount * i - stats.AmountRange / 2;
            angle += stats.Spread.AsRange();
            Vector2 realDir = (target - transform.position).normalized;
            if (A_InputManager.GamepadMode)
                realDir = (V_GamepadCursor.CursorPos - (Vector2)transform.position).normalized;

            Vector2 dir = Quaternion.AngleAxis(angle, Vector3.back) * realDir;            
            Shoot(dir.normalized);

            c++;
            if (c > PlayerGun.MAX_SHOTS_PER_FRAME)
            {
                c = 0;
                yield return null;
            }

            if (stats.Delay > 0 && i != amount - 1)
                yield return new WaitForSeconds(stats.Delay);
        }
        _timer = 0;
        _shooting = false;
    }

    void TryGetTarget()
    {
        if (CURSOR_TARGET_MODE)
        {
            _targetMove = Player.Instance.gameObject;
            _targetShoot = A_LevelManager.Instance.GetCursor();
            return;
        }

        Vector2 playerPos = Player.Instance.transform.position;
        var hits = Physics2D.OverlapCircleAll(transform.position, 12, _targetLayers);
        GameObject target = Player.Instance.gameObject;
        float dis = 99999999;
        bool entityGot = false;
        bool iTakeGot = false;
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out Entity e))
            {
                float d = Vector2.Distance(e.transform.position, playerPos);
                if (target == null || d < dis)
                {
                    target = hit.gameObject;
                    dis = d;
                    entityGot = true;
                }
            }
            if (entityGot)
                continue;

            if (hit.TryGetComponent(out ITakeDamage i))
            {
                float d = Vector2.Distance(hit.gameObject.transform.position, playerPos);
                if (target == null || d < dis)
                {
                    target = hit.gameObject;
                    dis = d;
                    iTakeGot = true;
                }
            }
            if (iTakeGot)
                continue;

            if (!_disableWallAttacks && hit.TryGetComponent(out TileWall w))
            {
                float d = Vector2.Distance(w.transform.position, playerPos);
                if (target == null || d < dis)
                {
                    target = hit.gameObject;
                    dis = d;
                }
            }
        }
        _targetMove = target;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag != "Charged")
            return;

        _chargeTimer = 0;
        _lightningBurstPS.Play();
        A_EventManager.InvokePlaySFX("Recharge");
        Squash(0.2f);
    }

    bool _isMoving;
    public IEnumerator C_MoveOneNode(float dur, Vector2 to)
    {
        if (_isMoving)
            yield break;

        //yield return new WaitForSeconds(Random.Range(0, 0.1f));

        to += new Vector2(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));
        List<Node> path = NodeManager.Instance.FindPath(transform.position, to, 7);

        if (path == null || path.Count < _distance)
            yield break;

        _isMoving = true;

        Vector2 startPos = transform.position;
        Vector2 endPos = path[1].pos + new Vector2(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f));
        float elapsed = 0;
        dur *= Vector2.Distance(startPos, endPos);

        while (elapsed < dur)
        {
            transform.position = Vector2.Lerp(startPos, endPos, elapsed / dur);
            elapsed += Time.deltaTime;
            yield return null;
        }

        _isMoving = false;
    }

    void Pointer()
    {
        Vector3 dir = (Player.Instance.transform.position - transform.position).normalized;
        _pointer.transform.position = Player.Instance.transform.position - dir;
        _pointer.transform.right = -dir;

        float dis = Vector2.Distance(Player.Instance.transform.position, transform.position);

        Vector2 targetSize = dis > 1.5f ? Vector2.one : Vector2.zero;
        _pointer.transform.localScale = Vector2.Lerp(_pointer.transform.localScale, targetSize, Time.deltaTime * 5);

        _pointRend.sprite = _chargeTimer <= _chargeCooldown ? _pointOn : _pointOff;
    }

    void SetBossMode(EBoss _) => _disableWallAttacks = true;

    bool _squashing;
    public void Squash(float dur) => StartCoroutine(C_SqaushStretch(dur));
    IEnumerator C_SqaushStretch(float dur)
    {
        if (_squashing)
            yield break;
        _squashing = true;

        float elapsed = 0;
        Vector2 start = transform.localScale;

        while (elapsed < dur)
        {
            float humped = 1 - A_Extensions.HumpCurve(elapsed / dur, 0, 1);
            transform.localScale = start + new Vector2(0.1f * -humped, 0.1f * humped);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = start;
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
}
