using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RC", menuName = "Item/RC", order = 1)]
public class J_RC : ItemData
{
    GameObject _cursor;
    float _timer;
    [SerializeField] SpriteRenderer _bulletPrefab;
    KeyCode _key;

    public override void OnLoadItem()
    {
        _key = A_InputManager.Instance.Key("Shoot");
    }

    public override void AfterTime()
    {
        if (Player.Instance == null || Player.Instance.Stopped || A_LevelManager.Instance.SceneTime < 3)
            return;

        if (_cursor == null)
        {
            _cursor = A_LevelManager.Instance.GetCursor();
        }

        if (_key == default(KeyCode))
            _key = A_InputManager.Instance.Key("Shoot");

        _timer += Time.deltaTime;

        if (_timer >= 5 && Input.GetKey(_key))
        {
            BulletPlayer bullet;
            if (PlayerGun.LLEnd == null)
            {
                SpriteRenderer go = Instantiate(_bulletPrefab, Player.Instance.transform.position, Quaternion.identity);
                bullet = go.GetComponent<BulletPlayer>();
            }
            else
            {
                bullet = PlayerGun.Pop();
                bullet.gameObject.SetActive(true);
                bullet.transform.position = Player.Instance.transform.position;
            }

            Vector2 dir = (_cursor.transform.position - Player.Instance.transform.position).normalized;

            PlayerStats stats = new PlayerStats();
            stats.Homing = 15;
            stats.BulletSpeed = 4f;
            stats.Range = 5;
            stats.Damage = 2.5f;
            stats.BulletSize = 0.6f;

            bullet.Init(dir, stats, _bulletPrefab, true); 
            bullet.SetTarget(_cursor);

            _timer = 0;
        }
    }
}
