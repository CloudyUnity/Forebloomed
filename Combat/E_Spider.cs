using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_Spider : Entity
{
    [SerializeField] List<GameObject> _targets = new List<GameObject>();
    [SerializeField] List<GameObject> _legs = new List<GameObject>();
    [SerializeField] List<E_SpiderPart> _subParts = new List<E_SpiderPart>();
    [SerializeField] float _moveDur;
    [SerializeField] float _safeZoneRadius;

    bool _infinite;
    bool moving;
    Vector2[] basePos;

    public override void Start()
    {
        base.Start();

        if (!enabled)
        {
            Destroy(transform.parent.gameObject);
            return;
        }

        basePos = new Vector2[_targets.Count];
        for (int i = 0; i < _targets.Count; i++)
            basePos[i] = _targets[i].transform.localPosition;
    }

    public override void Update()
    {
        base.Update();

        if (!_infinite)
        {
            _movingC = StartCoroutine(C_InfinitePath(false));
            _infinite = true;
        }                    

        transform.right = Vector2.Lerp(transform.right, (Player.Instance.HitBox - transform.position).normalized, Time.deltaTime * 2);

        MoveLegs();

        if (moving)
            return;

        float largestDistance = 0;
        GameObject furthestTarget = _targets[0];
        Vector2 storedZone = Vector2.zero;

        for (int i = 0; i < _targets.Count; i++)
        {
            Vector3 safeZone = RotatePoint(basePos[i], transform.rotation.eulerAngles);
            safeZone += transform.localPosition;

            float distance = Vector2.Distance(safeZone, _targets[i].transform.localPosition);
            if (distance >= largestDistance)
            {
                largestDistance = distance;
                furthestTarget = _targets[i];
                storedZone = safeZone;
            }
        }

        if (largestDistance < _safeZoneRadius)
            return;

        Vector2 rand = new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
        StartCoroutine(C_MoveTarget(furthestTarget, storedZone + rand));
    }

    IEnumerator C_MoveTarget(GameObject target, Vector2 endPos)
    {
        moving = true;

        float elapsed = 0;
        Vector2 startPos = target.transform.localPosition;

        while (elapsed < _moveDur)
        {
            target.transform.localPosition = Vector2.Lerp(startPos, endPos, elapsed / _moveDur);
            elapsed += Time.deltaTime;
            yield return null;
        }

        moving = false;
    }

    void MoveLegs()
    {
        for (int i = 0; i < _legs.Count; i++)
        {
            Vector2 dir = (_targets[i].transform.localPosition - transform.localPosition).normalized;

            _legs[i].transform.right = dir;

            _legs[i].transform.localPosition = transform.localPosition;
        }
    }

    public override void Despawn(GameObject toDespawn) => base.Despawn(transform.parent.gameObject);

    public override void Die(GameObject toDie)
    {
        foreach (var part in _subParts) 
            part.MakeCorpse(part.transform.position, part.transform.lossyScale, false);
        NormalSize = transform.lossyScale;
        base.Die(transform.parent.gameObject);
    }

    public override void TakeDamage(float dmg)
    {
        foreach (var part in _subParts)
            part.AnimDamage();        
        base.TakeDamage(dmg);
    }

    private Vector2 RotatePoint(Vector3 point, Vector2 angles) => Quaternion.Euler(angles) * point;
}