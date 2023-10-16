using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boomerang : MonoBehaviour
{
    [SerializeField] LayerMask _targetLayer;
    [SerializeField] LayerMask _reCollideLayer;
    [SerializeField] bool _antiInflict;
    [SerializeField] string _sfx;
    [SerializeField] TrailRenderer _trail;
    [SerializeField] float _trailMultiplier;
    [SerializeField] Collider2D _col;
    [SerializeField] SpriteRenderer _rend;

    float _range;
    public float _dmg;
    public Vector2 Dir;
    float _duration;
    Vector2 _targetPos;
    Vector2 _startPos;
    Transform _endRend;
    int _piercing;

    public PlayerBoomerang PlayerBoomer;

    public Boomerang NextInList = null;

    float _timer;
    bool _recollided;

    public bool DisableReturnEvent;

    public void CustomInit(Vector2 start, Vector2 end, float time)
    {
        _startPos = start;
        _targetPos = end;
        _timer = time * _duration;
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        float percent = _timer / _duration;

        float rotation = percent * 720;
        transform.rotation = Quaternion.Euler(0, 0, rotation);

        if (percent.IsBetween(0, 0.5f))
        {
            float curved = A_Extensions.CosCurve(percent / 0.5f);
            transform.position = Vector2.Lerp(_startPos, _targetPos, curved);
            return;
        }

        if (percent > 0.5f && !_recollided && !DisableReturnEvent)
        {
            ReCollide();
            J_Boomerang.Boomers = 1;
            A_EventManager.InvokePlayerBoomerangReturn(this);
            _recollided = true;
        }

        if (percent.IsBetween(0.6f, 1))
        {
            float curved = A_Extensions.CosCurve((percent - 0.6f) / 0.4f);
            Vector2 endPos = _endRend.position;
            transform.position = Vector2.Lerp(_targetPos, endPos, curved);
            return;
        }

        if (percent > 1)
        {
            PlayerBoomer.BoomerangsOut--;
            PlayerBoomerang.Push(this);
            gameObject.SetActive(false);
        }            
    }

    public void AddForce(Vector2 dir, PlayerStats stats, Transform rend, PlayerBoomerang boomerang)
    {
        PlayerBoomer = boomerang;

        A_EventManager.InvokePlayerBoomerSpawn(this);

        if (ItemManager.Instance.AllColors.Count > 0)
        {
            _rend.color = ItemManager.Instance.AllColors.RandomItem();
            _rend.SetAlpha(1);
        }        

        _duration = (stats.FireRate / 200) / (stats.BulletSpeed / 10);

        transform.localScale = stats.BulletSize * Vector2.one;
        if (_trail != null)
            _trail.startWidth = stats.BulletSize * _trailMultiplier;

        _range = stats.Range;
        _piercing = (int)stats.Piercing;

        if (stats.Damage > 0)
            _dmg = Mathf.Clamp(stats.Damage, 0.05f, float.MaxValue);

        Dir = dir;

        _targetPos = transform.position + (Vector3)Dir * _range;
        HomeIn(stats.Homing);

        _endRend = rend;
        _timer = 0;
        _recollided = false;
        A_EventManager.InvokePlaySFX(_sfx);
        _startPos = transform.position;
        DisableReturnEvent = false;
        ReCollide();
    }

    GameObject FindTarget(float homing)
    {
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, Mathf.Abs(homing) * 5, _targetLayer);

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

    void HomeIn(float homing)
    {
        if (homing == 0)
            return;
        homing = Mathf.Clamp(homing, -10, 10) * 2;

        GameObject go = FindTarget(homing);
        if (go == null)
            return;

        Vector2 dir = go.transform.position - (Vector3)_targetPos;        

        if (homing < 0)
        {
            Vector2 away = _targetPos - dir.normalized;
            _targetPos = Vector2.Lerp(_targetPos, away, -homing / 10);
            return;
        }

        Vector2 close = _targetPos + dir.normalized;
        _targetPos = Vector2.Lerp(_targetPos, close, homing / 10);
    }

    void ReCollide()
    {
        Collider2D[] results = new Collider2D[5];
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(_reCollideLayer);

        if (_col.OverlapCollider(filter, results) > 0)
        {
            foreach (var hit in results)
            {
                if (hit == null)
                    continue;

                Collide(hit.gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag.Is("Enemy", "Tile", "TileWall", "ITakeDamage"))
            Collide(collision.gameObject);
    }

    void Collide(GameObject go)
    {
        A_EventManager.InvokePlayerBoomerangCollide(this, go);
        A_EventManager.InvokePlaySFX("ShotHit");
        if (_piercing <= 0 && _timer / _duration < 0.5f)
        {
            _timer = _duration - _timer;
        }
        _piercing--;

        if (go.tag == "Tile")
        {
            Tile tile = go.GetComponent<Tile>();
            if (tile is TileWall wall)
            {
                wall.TakeDamage(_dmg);
                if (!_antiInflict)
                    wall.InflictStatus();
            }
        }

        if (go.tag == "Enemy")
        {
            Entity entity = go.GetComponent<Entity>();
            entity.TakeDamage(_dmg);
            if (!_antiInflict)
                entity.InflictStatus();
            entity.KnockBack(Dir);

            MakeInfo? info = ItemManager.Instance.RandomDrop();
            if (info != null && !(entity is E_Dummy))
            {
                A_Factory.Instance.MakeBasic(transform.position, (MakeInfo)info);
            }
        }

        if (go.tag == "ITakeDamage")
        {
            ITakeDamage takeD = go.GetComponent<ITakeDamage>();
            takeD.TakeDamage(_dmg);

            MakeInfo? info = ItemManager.Instance.RandomDrop();
            if (info != null)
            {
                A_Factory.Instance.MakeBasic(transform.position, (MakeInfo)info);
            }
        }
    }

    public void Squash(float dur) => StartCoroutine(C_SqaushStretch(dur));
    IEnumerator C_SqaushStretch(float dur)
    {
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
    }
}
