using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Item/???", order = 1)]
public class J_Boomerang : ItemData
{
    public override void OnPlayerBulletMiss(BulletPlayer old)
    {
        if (old.Speed < 0.2f)
            return;

        BulletPlayer bullet;
        if (old.LaserPool)
        {
            if (PlayerGun.LaserLLEnd == null)
            {
                bullet = Instantiate(old, old.transform.position, Quaternion.identity);
            }
            else
            {
                bullet = PlayerGun.PopLaser();
                bullet.transform.position = old.transform.position;
                bullet.gameObject.SetActive(true);
            }
        }
        else if (PlayerGun.LLEnd == null)
        {
            bullet = Instantiate(old, old.transform.position, Quaternion.identity);
        }
        else
        {
            bullet = PlayerGun.Pop();
            bullet.transform.position = old.transform.position;
            bullet.gameObject.SetActive(true);
        }
        old.NextInList = bullet;

        bullet.Init(Vector2.zero, Player.Instance.CurStats, old._rend);
        bullet.Range += 99;
        bullet.transform.localScale = old.transform.localScale;        
        bullet.StartCoroutine(C_TravelBack(bullet));
    }

    IEnumerator C_TravelBack(BulletPlayer b)
    {        
        float e = 0;
        float d = Random.Range(0.3f, 1.2f);
        Vector2 st = b.transform.position;

        while (e < d && b.gameObject.activeSelf)
        {
            float c = A_Extensions.CosCurve(e / d);
            b.transform.position = Vector2.Lerp(st, Player.Instance.transform.position, c);
            e += Time.deltaTime;
            yield return null;
        }
        if (b.gameObject.activeSelf)
            b.Deactivate();
    }

    public static int Boomers = 1;
    public override void OnPlayerBoomerangReturn(Boomerang old)
    {
        Boomerang boom;
        if (PlayerBoomerang.LLEnd == null)
        {
            boom = Instantiate(old, old.transform.position, Quaternion.identity);
        }
        else
        {
            boom = PlayerBoomerang.Pop();
            boom.gameObject.SetActive(true);
            boom.transform.position = old.transform.position;
        }

        old.NextInList = boom;

        PlayerStats stats = Player.Instance.CurStats;
        boom.AddForce(Vector2.zero, stats, old.PlayerBoomer.RendTransform, old.PlayerBoomer);
        boom.transform.localScale = old.transform.localScale;

        Vector3 dir = (old.transform.position - old.PlayerBoomer.RendTransform.position).normalized;
        Vector3 end = old.transform.position + dir * 0.3f * Boomers;
        Boomers++;
        boom.CustomInit(old.PlayerBoomer.RendTransform.position,end, 0.47f);

        boom.DisableReturnEvent = true;
        old.PlayerBoomer.BoomerangsOut++;
    }
}
