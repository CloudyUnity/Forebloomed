using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Razor", menuName = "Item/Razor", order = 1)]
public class J_RazorWeeds : ItemData
{
    [SerializeField] SpriteRenderer _bullet;
    [SerializeField] PlayerStats _stats;
    public override void OnHedgeBreak(TileWall wall)
    {
        Vector2 dir = (Player.Instance.transform.position - wall.transform.position).normalized;
        BulletPlayer bullet;
        if (PlayerGun.LLEnd == null)
        {
            SpriteRenderer go = Instantiate(_bullet, Player.Instance.transform.position, Quaternion.identity);
            bullet = go.GetComponent<BulletPlayer>();
        }
        else
        {
            bullet = PlayerGun.Pop();
            bullet.gameObject.SetActive(true);
            bullet.transform.position = Player.Instance.transform.position;
        }
        bullet.Init(dir, _stats, _bullet, true);
        A_EventManager.InvokePlaySFX("Shoot");
    }
}
