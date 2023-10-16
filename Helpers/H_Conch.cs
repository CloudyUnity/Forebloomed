using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class H_Conch : MonoBehaviour
{
    [SerializeField] float _speed;
    [SerializeField] SpriteRenderer _rend;
    [SerializeField] float _sightRange;
    [SerializeField] LayerMask _targetLayers;
    [SerializeField] int _pathSize;
    bool _isMoving = false;

    GameObject _target;

    float _timeSinceLastRouting = 0;

    Vector2 _defaultScale;

    private void Start()
    {
        _defaultScale = transform.localScale;
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

        _rend.flipX = _target.transform.position.x < transform.position.x;

        _timeSinceLastRouting += Time.deltaTime;
        if (_timeSinceLastRouting > 0.1f)
        {
            StartCoroutine(C_MoveOneNode(_target.transform.position, _speed));            
        }
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

        if (_target != Player.Instance.gameObject)
            to += new Vector2(Random.Range(-0.4f, 0.4f), Random.Range(-0.4f, 0.4f));
        else
            to += new Vector2(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));

        List<Node> path = NodeManager.Instance.FindPath(transform.position, to, _pathSize);

        int dis = 2;
        if (path == null || path.Count < dis)
        {
            //Debug.Log("Path failed");
            yield break;
        }

        _timeSinceLastRouting = 0;
        _isMoving = true;

        Vector2 startPos = transform.position;
        Vector2 endPos = path[1].pos + new Vector2(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f));
        float elapsed = 0;
        Squash(0.1f);
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
