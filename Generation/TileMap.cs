using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMap : MonoBehaviour
{
    public static TileMap Instance;

    public GenData Gen;

    int halfMapSize;
    float[,] heightMap;
    float[,] difficultyMap;
    float[,] _altMap;

    Vector2 lastCurChunk = new Vector2(-1, -1);

    GameObject[,] MegaGridMatrix;

    Tile[,] GridMatrix;

    float _timeSinceStart = 0;

    Vector2 _minibossPos;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Gen = GenDataManager.Instance.GetGenData();

        halfMapSize = Gen.MapSize / 2;
        MegaGridMatrix = new GameObject[Gen.MapSize, Gen.MapSize];
        GridMatrix = new Tile[Gen.MapSize * Gen.ChunkSize, Gen.MapSize * Gen.ChunkSize];

        _minibossPos = new Vector2(A_Extensions.RandomBetween(-1, 1f, A_LevelManager.QuickSeed), A_Extensions.RandomBetween(-1, 1f, A_LevelManager.QuickSeed + 45)).normalized * 40;
        //Debug.Log(_minibossPos);
        _minibossPos = new Vector2(Mathf.RoundToInt(_minibossPos.x), Mathf.RoundToInt(_minibossPos.y));
        A_LevelManager.Instance.MiniBossPos = _minibossPos;
        //Debug.Log(_minibossPos); // (-6, 3), (-1.8, 7.2)

        StartCoroutine(GenerateMap(new Vector2(0, 0), frameByFrame: false));
    }

    private void Update()
    {
        _timeSinceStart += Time.deltaTime;

        if (Player.Instance == null)
            return;

        Vector2 playerPos = Player.Instance.transform.position;
        Vector2 curChunk = playerPos.RoundBy(Gen.ChunkSize);

        if (lastCurChunk != curChunk)
        {
            List<Vector2> surrounding = A_Extensions.PositionsAroundX(curChunk, 1);

            for (int x = (int)curChunk.x - 5; x < curChunk.x + 5; x++)
                for (int y = (int)curChunk.y - 5; y < curChunk.y + 5; y++)
                {
                    GameObject chunk = MegaGridMatrix[x + halfMapSize, y + halfMapSize];

                    if (surrounding.Contains(new Vector2(x, y)))
                    {
                        if (chunk != null)
                        {
                            chunk.SetActive(true);
                            continue;
                        }
                        bool frame = _timeSinceStart > 5;
                        StartCoroutine(GenerateMap(new Vector2(x, y) * Gen.ChunkSize, frameByFrame: frame));
                        continue;
                    }

                    if (chunk != null)
                    {
                        chunk.SetActive(false);
                    }
                }
        }

        lastCurChunk = curChunk;
    }

    IEnumerator GenerateMap(Vector2 offset, bool frameByFrame)
    {
        A_EventManager.InvokeMakeGrid(offset, Gen.ChunkSize);

        heightMap = TileNoise.Generate(Gen.ChunkSize, 1, Gen.HeightWaves, offset);
        difficultyMap = TileNoise.Generate(Gen.ChunkSize, 1, Gen.DiffWaves, offset);
        _altMap = TileNoise.Generate(Gen.ChunkSize, 1, Gen.AltWaves, offset);

        GameObject parent = new GameObject("MG: " + offset.x + "," + offset.y);
        Vector2 megaGridPos = offset.RoundBy(Gen.ChunkSize);
        MegaGridMatrix[(int)megaGridPos.x + halfMapSize, (int)megaGridPos.y + halfMapSize] = parent;

        int counter = 0;
        int frameAmount = 3;
        for (int x = 0; x < Gen.ChunkSize; x++)
        {
            for (int y = 0; y < Gen.ChunkSize; y++)
            {
                Vector2 point = offset - new Vector2(Gen.ChunkSize / 2, Gen.ChunkSize / 2) + new Vector2(x, y);

                TilePreset preset;
                float distance = Mathf.Abs(offset.x) + Mathf.Abs(offset.y);
                // Spawn Area:
                if (Mathf.Abs(point.x) + Mathf.Abs(point.y) <= 4 || Vector2.Distance(point, _minibossPos) < 8)
                {                  
                    preset = GetTile(0.2f, difficultyMap[x, y] + distance * Gen.DistanceMult, _altMap[x, y]);
                }
                else if (!InsideEllipse(point))
                {
                    preset = GetTile(1, difficultyMap[x, y] + distance * Gen.DistanceMult, _altMap[x, y]);
                }
                else
                {
                    preset = GetTile(heightMap[x, y], difficultyMap[x, y] + distance * Gen.DistanceMult, _altMap[x, y]);
                }
                

                Tile tile = Instantiate(preset.TilePrefab, point, Quaternion.identity);
                tile.transform.parent = parent.transform;

                if (point == Vector2.zero)
                    tile.SetSprites((preset.DefaultTop, preset.DefaultBottom));
                else
                    tile.SetSprites(preset.GetSprites());
                tile.SetMyPos(point);

                SetTile(point, tile);
                A_EventManager.InvokeTileSpawn(tile);

                if (tile is TileWall wallScript)
                {
                    wallScript.SetType(preset.HitPoints);
                }

                if (point == Vector2.zero)
                {
                    tile.MarkExplored();
                    counter++;

                    if (frameByFrame && counter >= frameAmount)
                    {
                        counter = 0;
                        yield return null;
                    }
                    continue;
                }                    

                TileDecor decor = preset.GetDecor();
                if (decor != null)
                    A_Factory.Instance.MakeDecor(tile, decor);

                float seed = (Mathf.Abs(point.x) + 656475687) * (Mathf.Abs(point.y) + 3246347635) * A_LevelManager.QuickSeed * (x + 1) * (y + 1);
                if (A_Extensions.RandomBetween(0, 100f, seed) < Gen.OWChance)
                {
                    preset.MakeWorldObjects(point);
                }

                preset.MakeProps(point, tile.gameObject);

                if (point == _minibossPos && Gen.Minibosses.Length > 0)
                    Instantiate(Gen.Minibosses.RandomItem(A_LevelManager.QuickSeed), point, Quaternion.identity);

                counter++;

                if (frameByFrame && counter >= frameAmount)
                {
                    counter = 0;
                    yield return null;
                }
            }
        }
    }

    TilePreset GetTile(float height, float difficulty, float alt)
    {
        var matchingTiles = new List<TilePresetData>();
        foreach (TilePreset tile in Gen.Tiles)
        {
            if (tile.MatchCondition(height, difficulty, alt))
            {
                matchingTiles.Add(new TilePresetData(tile));
            }
        }

        float lowestDiff = 99f;
        TilePreset tileToReturn = null;
        foreach (TilePresetData tile in matchingTiles)
        {
            if (tile.GetDiffValue(height, difficulty, alt) < lowestDiff)
            {
                tileToReturn = tile.tilePreset;
                lowestDiff = tile.GetDiffValue(height, difficulty, alt);
            }
        }
        if (tileToReturn == null)
            tileToReturn = Gen.Tiles[0];

        return tileToReturn;
    }

    public Tile GetTileAtPos(Vector2 pos)
    {
        pos.x += halfMapSize * Gen.ChunkSize;
        pos.y += halfMapSize * Gen.ChunkSize;

        if (pos.x < 0 || pos.y < 0)
            return null;

        return GridMatrix[(int)pos.x, (int)pos.y];
    }

    public void SetTile(Vector2 pos, Tile tile)
    {
        int gridX = (int)pos.x + (halfMapSize * Gen.ChunkSize);
        int gridY = (int)pos.y + (halfMapSize * Gen.ChunkSize);

        if (!InsideEllipse(pos))
        {
            tile.SetHealthToMaximum();
        }

        if (gridX < 0 || gridY < 0)
            return;

        GridMatrix[gridX, gridY] = tile;
    }

    bool InsideEllipse(Vector2 pos) => (pos.x.Pow(2) / 110889) + (pos.y.Pow(2) / 62500) <= 1;
}
