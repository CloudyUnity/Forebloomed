using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class O_Collectable : MonoBehaviour
{
    public static int HeartsGrabbed;

    [SerializeField] Vector2 _collectTime;
    [SerializeField] Vector2 arcSize;
    [SerializeField] PlayerSoftStats softStats;
    [SerializeField] int charges;
    [SerializeField] float time;
    [SerializeField] SpriteRenderer _rend;
    [SerializeField] SpriteRenderer _rend2;
    [SerializeField] Sprite[] _sprites;
    [SerializeField] bool _ignoreAtMaxHealth;
    [SerializeField] bool _includeInCap;
    [SerializeField] CircleCollider2D _col;
    [SerializeField] LayerMask _combineLayer;
    [SerializeField] MakeInfo _gold10;
    [SerializeField] MakeInfo _gold100;
    [SerializeField] bool _jackAmmo;
    [SerializeField] float _despawnRange;
    [SerializeField] ParticleSystem _collectPS;
    public string Name;
    Vector2 _defScale;

    public static int BeziersGathering = 0;

    bool _gathering;

    private void Start()
    {
        if (_includeInCap && A_LevelManager.Instance.HeartCount >= 10)
        {
            Destroy(gameObject);
            return;
        }

        if (_includeInCap)
            A_LevelManager.Instance.HeartCount++;

        if (_rend == null)
            return;

        Sprite spr = _sprites.RandomItem();
        _rend.sprite = spr;
        if (_rend2 != null)
            _rend2.sprite = spr;

        if (!_col.enabled)
            return;

        Combine();

        StartCoroutine(C_Grow());
    }

    [SerializeField] float _jiggleCooldown = 1.5f;
    [SerializeField] float _jiggleDur = 0.3f;
    [SerializeField] float _jiggleMag = 0.1f;
    private void Update()
    {
        _sqaushTimer += Time.deltaTime;
        if (_sqaushTimer > _jiggleCooldown)
        {
            Squash(_jiggleDur);
            _sqaushTimer = 0;
        }
    }

    float _sqaushTimer;
    public void Squash(float dur) => StartCoroutine(C_SqaushStretch(dur));
    IEnumerator C_SqaushStretch(float dur)
    {
        float elapsed = 0;
        Vector2 start = _rend.transform.localScale;

        while (elapsed < dur)
        {
            float humped = 1 - A_Extensions.HumpCurve(elapsed / dur, 0, 1);
            _rend.transform.localScale = start + new Vector2(_jiggleMag * -humped, _jiggleMag * humped);
            elapsed += Time.deltaTime;
            yield return null;
        }
        _rend.transform.localScale = start;
    }

    IEnumerator C_Grow()
    {
        float elapsed = 0;
        float dur = 0.15f;
        _defScale = transform.localScale;

        while (elapsed < dur)
        {
            transform.localScale = Vector2.Lerp(Vector2.zero, _defScale, elapsed / dur);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = _defScale;
    }

    void Combine()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, 2, _combineLayer);
        List<O_Collectable> list = new List<O_Collectable>();
        foreach (var hit in hits)
        {
            if (list.Count == 10)
                break;

            if (hit.gameObject == gameObject || hit == null)
                continue;

            var collect = hit.gameObject.GetComponent<O_Collectable>();
            if (collect != null && collect.Name == Name && Name != "")
            {
                list.Add(collect);
            }
        }

        if (list.Count < 10)
            return;

        foreach (var col in list)
        {
            col.StartCoroutine(col.C_Move(transform.position));
            col.DisableCollider();
        }

        DisableCollider();
        StartCoroutine(C_Combine());
    }

    public void DisableCollider() => _col.enabled = false;

    IEnumerator C_Combine()
    {
        yield return new WaitForSeconds(0.2f);

        if (Name == "Gold")
            A_Factory.Instance.MakeBasic(transform.position, _gold10);
        else if (Name == "Gold10")
            A_Factory.Instance.MakeBasic(transform.position, _gold100);
        else
            Debug.Log(Name + " is not valid");

        if (_includeInCap)
            A_LevelManager.Instance.HeartCount--;

        Destroy(gameObject);
    }


    IEnumerator C_Move(Vector2 pos)
    {
        float elapsed = 0;
        float dur = 0.2f;
        Vector2 startPos = transform.position;

        while (elapsed < dur)
        {
            transform.position = Vector2.Lerp(startPos, pos, elapsed / dur);
            transform.localScale = Vector2.Lerp(Vector2.one, Vector2.zero, elapsed / dur);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (_includeInCap)
            A_LevelManager.Instance.HeartCount--;
        Destroy(gameObject);
    }

    static bool _goldTrig;
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (_gathering || collision.gameObject.tag != "Magnet")
            return;

        int emptyHearts = Player.Instance.SoftStats.MaxHealth - Player.Instance.SoftStats.CurHealth;

        if (_ignoreAtMaxHealth && emptyHearts <= HeartsGrabbed)
            return;

        if (Name == "BonusHeart" && Player.Instance.SoftStats.MaxHealth + Player.Instance.SoftStats.BonusHealth >= 15)
            return;

        if (Name == "Heart" && Player.Instance.SoftStats.CurHealth == Player.Instance.SoftStats.MaxHealth)
            return;

        if (_jackAmmo && Player.Instance is P_Jack jack)
        {
            if (jack.Ammo + jack.AmmoToGrab >= 5)
                return;

            jack.AmmoToGrab++;
        }

        if (_ignoreAtMaxHealth)
            HeartsGrabbed++;

        if (Name == "Gold100" && !_goldTrig)
        {
            A_EventManager.InvokeUnlock("What a catch!");
            _goldTrig = true;
        }            

        if (BeziersGathering > 15)
        {
            Collect();
            if (_includeInCap)
                A_LevelManager.Instance.HeartCount--;
            Destroy(gameObject);
            return;
        }

        _rend.sortingOrder = 2000;
        DisableCollider();
        StartCoroutine(C_BezierMove());
    }

    IEnumerator C_BezierMove()
    {
        if (_gathering)
            yield break;

        _gathering = true;
        BeziersGathering++;

        float elapsed = 0;
        Vector2 p0 = transform.position;
        float randArcSize = arcSize.AsRange();
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

        Collect();

        if (_includeInCap)
            A_LevelManager.Instance.HeartCount--;

        BeziersGathering--;
        Destroy(gameObject);
    }

    void Collect()
    {
        A_EventManager.InvokeGetCollectable(this);

        Player.Instance.AddSoftStats(softStats);

        if (_collectPS != null)
            Instantiate(_collectPS, Player.Instance.transform.position, Quaternion.identity);

        if (_ignoreAtMaxHealth)
        {
            HeartsGrabbed--;
        }

        if (_jackAmmo && Player.Instance is P_Jack jack)
        {
            jack.GetAmmo();
            jack.AmmoToGrab--;
        }
    }
}
