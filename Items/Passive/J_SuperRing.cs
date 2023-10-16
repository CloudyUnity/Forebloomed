using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SuperRing", menuName = "Item/SuperRing", order = 1)]
public class J_SuperRing : ItemData
{
    [SerializeField] SpriteRenderer ThornBullet;
    [SerializeField] float _cooldown;

    float timer;

    public override void AfterTime()
    {
        if (Player.Instance == null || Player.Instance.Stopped || A_LevelManager.Instance.SceneTime < 3)
            return;

        timer += Time.deltaTime;

        if (timer >= _cooldown && !Player.Instance.Dead)
        {
            PlayerStats stats = new PlayerStats();

            for (int i = 0; i < 360; i += 45)
            {
                float A = Mathf.Deg2Rad * i;
                Vector2 dir = new Vector2(Mathf.Cos(A), Mathf.Sin(A)); // Unit Circle Coords
                BulletPlayer bullet;
                if (PlayerGun.LLEnd == null)
                {
                    SpriteRenderer go = Instantiate(ThornBullet, Player.Instance.transform.position, Quaternion.identity);
                    bullet = go.GetComponent<BulletPlayer>();
                }
                else
                {
                    bullet = PlayerGun.Pop();
                    bullet.gameObject.SetActive(true);
                    bullet.transform.position = Player.Instance.transform.position;
                }
                bullet.transform.up = dir;
                stats.Damage = 0.48f;
                stats.Range = 0.8f;
                stats.BulletSpeed = 10;
                stats.BulletSize = 0.2f;
                bullet.Init(dir, stats, ThornBullet, true);
            }
            A_EventManager.InvokePlaySFX("ShootQ");
            timer = 0;
        }
    }
}
