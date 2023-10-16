using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SuperRing", menuName = "Item/Tammys", order = 1)]
public class J_Tammys : ItemData
{
    [SerializeField] SpriteRenderer ThornBullet;
    [SerializeField] SpriteRenderer _laser;
    [SerializeField] SpriteRenderer _arrow;
    [SerializeField] SpriteRenderer _slash;
    [SerializeField] Boomerang _boomer;

    public override void OnUseAbility()
    {
        if (Player.Instance == null || Player.Instance.Stopped)
            return;

        Player.Instance.StartCoroutine(C_FireShots());
    }

    IEnumerator C_FireShots()
    {
        yield return new WaitForSeconds(Random.Range(0, 0.1f));

        PlayerStats stats = Player.Instance.CurStats;

        for (int i = 0; i < 360; i += 45)
        {
            float A = Mathf.Deg2Rad * i;
            Vector3 dir = new Vector2(Mathf.Cos(A), Mathf.Sin(A)); // Unit Circle Coords

            if (Player.Instance.CharacterIndex == 6)
            {
                Boomerang boomer = null;
                if (PlayerBoomerang.LLEnd == null)
                {
                    boomer = Instantiate(_boomer, PlayerBoomerang.Instance.RendTransform.position, Quaternion.identity);
                }
                else
                {
                    boomer = PlayerBoomerang.Pop();
                    boomer.gameObject.SetActive(true);
                    boomer.transform.position = PlayerBoomerang.Instance.RendTransform.position;
                }

                boomer.AddForce(dir, stats, PlayerBoomerang.Instance.RendTransform, PlayerBoomerang.Instance);

                PlayerBoomerang.Instance.BoomerangsOut++;

                Player.Instance.KnockBack(dir, 0.005f);
                continue;
            }

            if (Player.Instance.CharacterIndex == 3)
            {
                B_Slash slash;
                Vector2 pos = Player.Instance.transform.position + (dir * Player.Instance.CurStats.Range);
                if (PlayerGun.LLEnd == null)
                {
                    var go = Instantiate(_slash, pos, Quaternion.identity);
                    slash = go.GetComponent<B_Slash>();
                }
                else
                {
                    slash = PlayerSword.Pop();
                    slash.gameObject.SetActive(true);
                    slash.transform.position = pos;
                }

                slash.Slice(Player.Instance.CurStats.Damage, dir);

                continue;
            }

            BulletPlayer bullet;

            if (Player.Instance.CharacterIndex == 2)
            {
                if (PlayerGun.LaserLLEnd == null)
                {
                    SpriteRenderer go = Instantiate(_laser, Player.Instance.transform.position, Quaternion.identity);
                    bullet = go.GetComponent<BulletPlayer>();
                }
                else
                {
                    bullet = PlayerGun.PopLaser();
                    bullet.transform.position = Player.Instance.transform.position;
                    bullet.gameObject.SetActive(true);
                }
                bullet.Init(dir, stats, _laser, true);
                continue;
            }

            else if (PlayerGun.LLEnd == null)
            {
                var go = Instantiate(ThornBullet, Player.Instance.transform.position, Quaternion.identity);
                bullet = go.GetComponent<BulletPlayer>();
            }
            else
            {
                bullet = PlayerGun.Pop();
                bullet.gameObject.SetActive(true);
                bullet.transform.position = Player.Instance.transform.position;
            }
            stats.Damage *= 0.5f;

            if (Player.Instance.CharacterIndex == 4)
            {
                bullet.Init(dir, stats, _arrow, true);
                continue;
            }

            bullet.Init(dir, stats, ThornBullet, true);
        }
    }
}
