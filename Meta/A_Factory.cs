using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class A_Factory : MonoBehaviour
{
    public static A_Factory Instance;

    [SerializeField] GameObject _enemyBullet;
    [SerializeField] GameObject _enemyBulletPurple;
    [SerializeField] GameObject _enemyBulletBreaker;
    [SerializeField] GameObject _enemyBulletPill;
    [SerializeField] GameObject _enemyBulletVisible;
    [SerializeField] GameObject _wormPrefab;
    [SerializeField] GameObject _decor;
    [SerializeField] GameObject _baseItem;
    [SerializeField] Vector2 MinBounds;
    [SerializeField] Vector2 MaxBounds;
    [SerializeField] MakeInfo _gold10;
    [SerializeField] MakeInfo _gold100;

    public Sprite DEFAULT_BULLET_SPRITE;
    public Material DEFAULT_BULLET_MAT;
    public Color DEFAULT_BULLET_COLOR;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, MaxBounds * 2);
        Gizmos.DrawWireCube(transform.position, MinBounds * 2);
    }

    public BulletEnemy[] MakeEnemyBullet(Vector3 pos, BulletInfo info)
    {
        Vector2 dir = (Player.Instance.HitBox - pos).normalized;
        return MakeEnemyBullet(pos, info, dir);
    }

    public BulletEnemy[] MakeEnemyBullet(Vector3 pos, BulletInfo info, Vector2 dir)
    {
        BulletEnemy[] gos = new BulletEnemy[info.amount];
        for (int i = 0; i < info.amount; i++)
        {
            BulletEnemy be;
            if (BulletEnemyCache.LLEnd == null)
            {
                be = Instantiate(_enemyBullet, pos, Quaternion.identity).GetComponent<BulletEnemy>();
            }
            else
            {
                be = BulletEnemyCache.Pop();
                be.gameObject.SetActive(true);
                be.transform.position = pos;
            }

            gos[i] = be;

            info.dir = dir;
            info.dir = Quaternion.AngleAxis(Random.Range(-info.spread, info.spread), Vector3.back) * info.dir;

            be.SetDir(info);
        }
        return gos;
    }

    public GameObject TurnToItem(Vector3 pos, ItemData data, float displacement)
    {
        GameObject go = Instantiate(_baseItem, pos + Displacement(displacement), Quaternion.identity);
        Item item = go.GetComponent<Item>();
        item.Data = data;
        return go;
    }

    public void MakeItem(float seed, Vector2 pos) => TurnToItem(pos, ItemHolder.Instance.GetRandomItem(seed, out _), 0);
    public GameObject MakeItem(float seed, Vector2 pos, ItemPool pool) => TurnToItem(pos, ItemHolder.Instance.GetRandomItem(seed, pool, out _), 0);
    public GameObject MakeItem(float seed, Vector2 pos, ItemPool pool, out int index) => TurnToItem(pos, ItemHolder.Instance.GetRandomItem(seed, pool, out index), 0);

    public void MakeDecor(Tile tile, TileDecor decor)
    {
        float amount = decor.Amount.AsRange();
        for (int i = 0; i < amount; i++)
        {
            GameObject go = Instantiate(_decor, tile.transform);
            go.transform.position += new Vector3(decor.Offset.AsRange(), decor.Offset.AsRange());
            if (tile is TileWall) go.transform.position += new Vector3(0, 0.5f);
            go.transform.localScale = decor.Size * Vector2.one;

            var rend = go.GetComponent<SpriteRenderer>();
            rend.sprite = decor.Sprite;
            rend.flipX = A_Extensions.Rand();
            if (tile is TileWall) rend.sortingOrder = 1550;
        }
    }

    public void MakeBasic(Vector3 pos, MakeInfo info) => MakeBasic(pos, info, null);

    public void MakeBasic(Vector3 pos, MakeInfo info, Transform parent)
    {
        if (info.Name == "Worm")
        {
            info.Prefab = new GameObject[] { _wormPrefab };
        }

        if (info.Prefab == null || info.Prefab.Length == 0)
            return;

        GameObject prefab = info.Prefab.RandomItem();
        if (prefab == null)
        {
            Debug.Log("Error with prefab " + info.Name + info.Amount + info.Displacement);
            return;
        }

        int total = (int)info.Amount.AsRange();

        if (info.Name == "Gold10" && total >= 10)
        {
            int amount = Mathf.FloorToInt(total / 10);
            for (int i = 0; i < amount; i++)
            {
                MakeBasic(pos, _gold100);
            }
            total %= 10;
        }

        if (info.Name == "Gold" && total >= 100)
        {
            int amount = Mathf.FloorToInt(total / 100);
            for (int i = 0; i < amount; i++)
            {
                MakeBasic(pos, _gold100);
            }
            total %= 100;
        }

        if (info.Name == "Gold" && total >= 10)
        {
            int amount = Mathf.FloorToInt(total / 10);
            for (int i = 0; i < amount; i++)
            {
                MakeBasic(pos, _gold10);
            }
            total %= 10;
        }

        for (int i = 0; i < total; i++)
        {
            Instantiate(prefab, pos + Displacement(info.Displacement), Quaternion.identity, parent);
        }

        if (info.Name == "Gold")
            A_EventManager.InvokeGoldSpawn(pos, total);
    }

    public void MakeBasicOOB(MakeInfo info)
    {
        GameObject prefab = info.Prefab.RandomItem();
        for (int i = 0; i < Random.Range(info.Amount.x, info.Amount.y); i++)
            Instantiate(prefab, OutsideCam() + Displacement(info.Displacement), Quaternion.identity);
    }

    Vector3 Displacement(float amount) => new Vector3(Random.Range(-amount, amount), Random.Range(-amount, amount));

    Vector3 OutsideCam()
    {
        int randSign = Random.value > 0.5f ? 1 : -1;
        Vector2 spawnPoint = new Vector2(Random.Range(MinBounds.x, MaxBounds.x) * randSign, Random.Range(MinBounds.y, MaxBounds.y) * randSign);

        if (Player.Instance != null)
            spawnPoint += (Vector2)Player.Instance.transform.position;

        return spawnPoint;
    }
}

[System.Serializable]
public struct MakeInfo
{
    public GameObject[] Prefab;
    public Vector2 Amount;
    public string Name;
    public float Displacement;

    public static MakeInfo operator +(MakeInfo a, MakeInfo b)
    {
        return new MakeInfo
        {
            Name = a.Name,
            Amount = a.Amount + b.Amount,
            Displacement = a.Displacement + b.Displacement,
            Prefab = a.Prefab
        };
    }
}
