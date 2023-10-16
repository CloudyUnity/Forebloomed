using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class H_Katy : MonoBehaviour
{
    float _timeSinceShoot;
    [SerializeField] float _cooldown;
    [SerializeField] GameObject _gun;
    [SerializeField] SpriteRenderer _gunRend;
    [SerializeField] ParticleSystem _ps;
    [SerializeField] float _speed;
    [SerializeField] Animator _anim;
    [SerializeField] SpriteRenderer _rend;
    [SerializeField] float _sightRange;
    [SerializeField] LayerMask _targetLayers;
    [SerializeField] int _pathSize;
    bool _isMoving = false;

    [SerializeField] PlayerStats _stats;
    [SerializeField] SpriteRenderer _prefab;
    GameObject _target;
    int _distance;

    float _timeSinceLastRouting = 0;

    Vector2 _defaultScale;

    private void Start()
    {
        _defaultScale = transform.localScale;
        _distance = Random.Range(2, 4);
    }

    public void Update()
    {
        if (Player.Instance.Stopped)
            return;

        float dis = Vector2.Distance(transform.position, Player.Instance.transform.position);
        if (dis > 15)
        {
            transform.position = Player.Instance.transform.position;
        }

        _target = GetTarget();

        _timeSinceShoot += Time.deltaTime;

        _anim.SetBool("Walking", _isMoving);
        _rend.flipX = _target.transform.position.x < transform.position.x;

        _timeSinceLastRouting += Time.deltaTime;
        if (_timeSinceLastRouting > 0.1f)
        {
            StartCoroutine(C_MoveOneNode(_target.transform.position, _speed));            
        }

        ManageGun();

        if (_target != Player.Instance.gameObject && _timeSinceShoot > _cooldown && Vector2.Distance(_target.transform.position, transform.position) < 3)
        {
            _timeSinceShoot = 0;

            StartCoroutine(C_ShootBullets());
        }
    }

    void ManageGun()
    {
        Vector2 dir = GetDir();
        _gun.transform.right = Vector2.Lerp(_gun.transform.right, dir, Time.deltaTime * 8);
        _gunRend.sortingOrder = dir.y <= 0 ? _rend.sortingOrder + 1 : _rend.sortingOrder - 1;

        if (dir == Vector2.left)
        {
            float z = _gun.transform.rotation.z;
            float newZ = Mathf.Lerp(z, 0, Time.deltaTime);
            _gun.transform.rotation = Quaternion.Euler(0, 180, newZ);
            return;
        }

        if (dir.x >= 0) return;

        Vector3 rot = _gun.transform.rotation.eulerAngles;
        rot.x = 180;
        rot.z *= -1;
        _gun.transform.rotation = Quaternion.Euler(rot);
    }

    Vector2 GetDir()
    {
        if (_target == Player.Instance.gameObject)
        {
            return _target.transform.position.x <= transform.position.x ? Vector2.left : Vector2.right;
        }

        return (_target.transform.position - transform.position).normalized;
    }

    IEnumerator C_ShootBullets()
    {
        int amount = (int)Mathf.Floor(_stats.Amount);
        int c = 0;
        for (int i = 0; i < amount; i++)
        {
            float angle = _stats.AmountRange / _stats.Amount * i - _stats.AmountRange / 2;
            angle += _stats.Spread.AsRange();
            Shoot(angle);

            c++;
            if (c > PlayerGun.MAX_SHOTS_PER_FRAME)
            {
                c = 0;
                yield return null;
            }

            if (_stats.Delay > 0 && i != amount - 1)
                yield return new WaitForSeconds(_stats.Delay);
        }
        A_EventManager.InvokePlaySFX("ShootQ");
    }

    void Shoot(float angle)
    {
        BulletPlayer bullet;
        if (PlayerGun.LLEnd == null)
        {
            SpriteRenderer r = Instantiate(_prefab, transform.position, Quaternion.identity);
            bullet = r.GetComponent<BulletPlayer>();
        }
        else
        {
            bullet = PlayerGun.Pop();
            bullet.gameObject.SetActive(true);
            bullet.transform.position = transform.position;
        }

        Vector2 dir = (_target.transform.position - transform.position).normalized;
        dir = Quaternion.AngleAxis(angle, Vector3.back) * dir;        
        bullet.Init(dir, _stats, _prefab);

        Squash(0.1f);
        _ps.Play();
    }

    GameObject GetTarget()
    {
        if (A_LevelManager.Instance.CurrentLevel.IsEven() && !E_Dummy.FamiliarsAllowedToHit)
            return Player.Instance.gameObject;

        Vector2 playerPos = Player.Instance.transform.position;
        var hits = Physics2D.OverlapCircleAll(playerPos, _sightRange, _targetLayers);
        GameObject target = Player.Instance.gameObject;
        float dis = 99999999;
        foreach (var hit in hits)
        {
            float d = Vector2.Distance(hit.gameObject.transform.position, playerPos);
            if (target == null || d < dis)
            {
                target = hit.gameObject;
                dis = d;
            }
        }
        return target;
    }

    public IEnumerator C_MoveOneNode(Vector2 to, float dur)
    {
        if (_isMoving)
            yield break;

        to += new Vector2(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));
        List<Node> path = NodeManager.Instance.FindPath(transform.position, to, _pathSize);

        if (path == null || path.Count < _distance)
        {
            //Debug.Log("Path failed");
            yield break;
        }

        _timeSinceLastRouting = 0;
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

    bool _squashing;
    public void Squash(float dur) => StartCoroutine(C_SqaushStretch(dur));
    IEnumerator C_SqaushStretch(float dur)
    {
        if (_squashing || _defaultScale == Vector2.zero)
            yield break;

        _squashing = true;

        float elapsed = 0;

        while (elapsed < dur)
        {
            float humped = 1 - A_Extensions.HumpCurve(elapsed / dur, 0, 1);
            transform.localScale = _defaultScale + new Vector2(0.1f * -humped, 0.1f * humped);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = _defaultScale;
        _squashing = false;
    }
}
