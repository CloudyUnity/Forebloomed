using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Cluster", menuName = "Item/Cluster", order = 1)]
public class J_ClusterShot : ItemData
{
    [SerializeField] SpriteRenderer _bullet;

    public override void OnEntityDie(Entity e)
    {
        Vector2 dir = Random.insideUnitCircle.normalized;
        PlayerStats stats = Player.Instance.CurStats;

        BulletPlayer bullet;
        if (PlayerGun.LLEnd == null)
        {
            SpriteRenderer go = Instantiate(_bullet, e.transform.position, Quaternion.identity);
            bullet = go.GetComponent<BulletPlayer>();
        }
        else
        {
            bullet = PlayerGun.Pop();
            bullet.transform.position = e.transform.position;
            bullet.gameObject.SetActive(true);
        }

        stats.Damage *= 0.5f;
        stats.Range = 0.2f;        
        bullet.Init(dir, stats, _bullet);
        bullet._sfx = null;
    }
}
