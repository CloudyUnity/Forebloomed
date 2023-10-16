using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_Worm : Entity
{
    [SerializeField] float _chargeUp;
    [SerializeField] Vector2 _randomOffset;
    float _realCooldown;

    [SerializeField] BulletInfo info;
    [SerializeField] LayerMask WallLayer;
    [SerializeField] MakeInfo _setInfo;
    float _timer;

    public override void Start()
    {
        base.Start();
        transform.rotation = Quaternion.Euler(0, 180, 0);
        _realCooldown = _chargeUp + _randomOffset.AsRange();
    }

    public override void Update()
    {
        base.Update();

        if (Player.Instance.Dead)
            return;

        _timer += Time.deltaTime;
        if (_timer > _realCooldown)
        {
            Shoot();
            _timer = 0;
            _realCooldown = _chargeUp + _randomOffset.AsRange();
        }
    }

    void Shoot()
    {
        BulletInfo info2 = info;
        info2.spread += Confused * 10;
        A_Factory.Instance.MakeEnemyBullet(transform.position, info2);
        A_EventManager.InvokePlaySFX("Enemy Shoot");
        Squash(0.2f);
        ScurryToBlock();
    }

    void ScurryToBlock()
    {
        GameObject closestWall = ClosestWall();
        if (closestWall == null)
        {
            Anim.SetBool("Run", false);
            return;
        }

        Anim.SetBool("Run", true);
        
        StartCoroutine(C_MoveTo(closestWall));
    }

    GameObject ClosestWall()
    {
        RaycastHit2D[] hits = Physics2D.BoxCastAll(transform.position, 3 * Vector2.one, 0, Vector3.back, 1, WallLayer);
        if (hits.Length == 0)
            return null;

        float lowestDis = float.MaxValue;
        GameObject go = hits[0].collider.gameObject;
        foreach (RaycastHit2D hit in hits)
        {
            float newDis = Vector2.Distance(hit.point, transform.position);
            if (newDis < lowestDis)
            {
                lowestDis = newDis;
                go = hit.collider.gameObject;
            }
        }
        return go;
    }

    IEnumerator C_MoveTo(GameObject target)
    {
        yield return new WaitForSeconds(1);
        float elapsed = 0;
        float dur = 1;
        Vector2 startPos = transform.position;
        A_EventManager.InvokePlaySFX("Worm");

        while(elapsed < dur)
        {
            _timer = 0;

            if (target == null)
            {
                ScurryToBlock();
                yield break;
            }

            float curved = A_Extensions.CosCurve(elapsed / dur);
            transform.position = Vector2.Lerp(startPos, target.transform.position, curved);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (target == null)
        {
            ScurryToBlock();
            yield break;
        }

        TileDrops.Instance.SetGridType(target.transform.position, _setInfo);
        Destroy(gameObject);
    }
}
