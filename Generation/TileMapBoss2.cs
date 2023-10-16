using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMapBoss2 : MonoBehaviour
{
    public static TileMapBoss2 Instance;

    public TilePreset _floorPreset;
    public TilePreset _wallPreset;

    [Header("Dimensions")]
    public int chunkSize;
    public int fullMapSize;
    public int mapWidth;
    int halfMapSize;
    public float scale = 1.0f;

    GameObject[,] MegaGridMatrix;

    Tile[,] GridMatrix;

    [SerializeField] GameObject _cam;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        halfMapSize = fullMapSize / 2;
        MegaGridMatrix = new GameObject[fullMapSize, mapWidth];
        GridMatrix = new Tile[fullMapSize * chunkSize, mapWidth * chunkSize];

        GenerateMap(new Vector2(0, 0));
    }

    private void Update()
    {
        Vector2 pChunk = ((Vector2)_cam.transform.position).RoundBy(chunkSize);
        Generate(pChunk);
    }

    void Generate(Vector2 pos)
    {
        List<Vector2> surrounding = A_Extensions.PositionsAroundX(pos, 1);

        for (int x = -halfMapSize; x < halfMapSize; x++)
            for (int y = -(mapWidth/2); y < (mapWidth/2); y++)
            {
                GameObject chunk = MegaGridMatrix[x + halfMapSize, y + (mapWidth/2)];

                if (surrounding.Contains(new Vector2(x, y)))
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
        MegaGridMatrix[(int)megaGridPos.x + halfMapSize, (int)megaGridPos.y + (mapWidth/2)] = parent;

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                Vector2 point = offset - new Vector2(chunkSize / 2, chunkSize / 2) + new Vector2(x, y);

                TilePreset preset = GetTile(point);

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
        if (Mathf.Abs(pos.y) < 3)
        {
            return _floorPreset;
        }

        return _wallPreset;
    }

    public Tile GetTileAtPos(Vector2 pos)
    {
        pos.x += halfMapSize * chunkSize;
        pos.y += (mapWidth/2) * chunkSize;
        return GridMatrix[(int)pos.x, (int)pos.y];
    }

    public void SetTile(Vector2 pos, Tile tile)
    {
        int gridX = (int)pos.x + (halfMapSize * chunkSize);
        int gridY = (int)pos.y + ((mapWidth/2) * chunkSize);

        if (gridX < 0 || gridY < 0)
            return;

        GridMatrix[gridX, gridY] = tile;
    }
}
