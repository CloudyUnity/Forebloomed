using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMapBoss : MonoBehaviour
{
    public static TileMapBoss Instance;

    public TilePreset _floorPreset;
    public TilePreset _wallPreset;

    [Header("Dimensions")]
    public int chunkSize;
    public int fullMapSize;
    int halfMapSize;
    public float scale = 1.0f;

    GameObject[,] MegaGridMatrix;

    Tile[,] GridMatrix;

    EBoss _boss;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable() => A_EventManager.OnBossSpawn += AssignBoss;
    private void OnDisable() => A_EventManager.OnBossSpawn -= AssignBoss;

    void AssignBoss(EBoss boss) => _boss = boss;

    private void Start()
    {
        halfMapSize = fullMapSize / 2;
        MegaGridMatrix = new GameObject[fullMapSize, fullMapSize];
        GridMatrix = new Tile[fullMapSize * chunkSize, fullMapSize * chunkSize];

        GenerateMap(new Vector2(0, 0));
    }

    private void Update()
    {
        Vector2 pPos;
        Vector2 bPos;
        Vector2 pChunk = Vector2.zero;
        Vector2 bChunk = Vector2.zero;

        if (Player.Instance != null)
        {
            pPos = Player.Instance.transform.position;
            pChunk = pPos.RoundBy(chunkSize);
        }

        if (_boss != null)
        {
            bPos = _boss.transform.position;
            bChunk = bPos.RoundBy(chunkSize);
        }

        Generate(pChunk, bChunk);
    }

    void Generate(Vector2 pos, Vector2 posBoss)
    {
        List<Vector2> surrounding = A_Extensions.PositionsAroundX(pos, 1);
        List<Vector2> surroundingBoss = A_Extensions.PositionsAroundX(posBoss, 1);

        for (int x = -halfMapSize; x < halfMapSize; x++)
            for (int y = -halfMapSize; y < halfMapSize; y++)
            {
                GameObject chunk = MegaGridMatrix[x + halfMapSize, y + halfMapSize];

                if (surrounding.Contains(new Vector2(x, y)) || surroundingBoss.Contains(new Vector2(x, y)))
                {
                    if (chunk != null)
                    {
                        chunk.SetActive(true);
                        continue;
                    }
                    GenerateMap(new Vector2(x, y) * chunkSize);
                    continue;
                }

                if (chunk != null)
                {
                    chunk.SetActive(false);
                }
            }
    }

    void GenerateMap(Vector2 offset)
    {
        GameObject parent = new GameObject("MG: " + offset.x + "," + offset.y);
        Vector2 megaGridPos = offset.RoundBy(chunkSize);
        MegaGridMatrix[(int)megaGridPos.x + halfMapSize, (int)megaGridPos.y + halfMapSize] = parent;

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                Vector2 point = offset - new Vector2(chunkSize / 2, chunkSize / 2) + new Vector2(x, y);

                TilePreset preset;
                // Spawn Area:
                if (Mathf.Abs(point.x) + Mathf.Abs(point.y) <= 2)
                {
                    preset = _floorPreset;
                }
                else
                {
                    preset = GetTile(point);
                }


                Tile tile = Instantiate(preset.TilePrefab, point, Quaternion.identity);
                tile.transform.parent = parent.transform;

                tile.SetSprites(preset.GetSprites());
                tile.SetMyPos(point);

                tile.MarkExplored();

                TileDecor decor = preset.GetDecor();
                if (decor != null)
                    A_Factory.Instance.MakeDecor(tile, decor);

                SetTile(point, tile);
                A_EventManager.InvokeTileSpawn(tile);

                if (tile is TileWall wallScript)
                    wallScript.SetType(preset.HitPoints);
            }
        }
    }

    TilePreset GetTile(Vector2 pos)
    {
        float spacing = 2.5f;
        bool isEvenEvenX = (pos.x / spacing).IsEven();
        bool isEvenEvenX1 = ((pos.x - 1) / spacing).IsEven();

        bool isEvenEvenY = (pos.y / spacing).IsEven();
        bool isEvenEvenY1 = ((pos.y - 1) / spacing).IsEven();

        if ((isEvenEvenX || isEvenEvenX1) && (isEvenEvenY || isEvenEvenY1))
        {
            return _wallPreset;
        }

        if (!InsideEllipse(pos))
            return _wallPreset;

        return _floorPreset;
    }

    bool InsideEllipse(Vector2 pos) => (pos.x.Pow(2) / 625) + (pos.y.Pow(2) / 441) <= 1;

    public Tile GetTileAtPos(Vector2 pos)
    {
        pos.x += halfMapSize * chunkSize;
        pos.y += halfMapSize * chunkSize;
        return GridMatrix[(int)pos.x, (int)pos.y];
    }

    public void SetTile(Vector2 pos, Tile tile)
    {
        int gridX = (int)pos.x + (halfMapSize * chunkSize);
        int gridY = (int)pos.y + (halfMapSize * chunkSize);

        if (gridX < 0 || gridY < 0)
            return;

        GridMatrix[gridX, gridY] = tile;
    }
}
