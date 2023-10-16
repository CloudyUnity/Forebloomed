using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMapBoss3 : MonoBehaviour
{
    public static TileMapBoss3 Instance;

    public TilePreset _floorPreset;
    public TilePreset _wallPreset;

    [Header("Dimensions")]
    public int chunkSize;
    public int fullMapSize;
    int halfMapSize;
    public float scale = 1.0f;

    Tile[,] GridMatrix;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        halfMapSize = fullMapSize / 2;
        GridMatrix = new Tile[fullMapSize * chunkSize, fullMapSize * chunkSize];

        Generate(new Vector2(0, 0));
    }

    void Generate(Vector2 pos)
    {
        List<Vector2> surrounding = A_Extensions.PositionsAroundX(pos, 1);

        for (int x = -halfMapSize; x < halfMapSize; x++)
            for (int y = -halfMapSize; y < halfMapSize; y++)
            {
                if (surrounding.Contains(new Vector2(x, y)))
                {
                    GenerateMap(new Vector2(x, y) * chunkSize);
                    continue;
                }
            }
    }

    void GenerateMap(Vector2 offset)
    {
        GameObject parent = new GameObject("MG: " + offset.x + "," + offset.y);

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
        if (InsideEllipse(pos) && Mathf.Abs(pos.x) != 10 && Mathf.Abs(pos.y) != 7)
        {
            return _floorPreset;
        }

        return _wallPreset;
    }

    bool InsideEllipse(Vector2 pos) => (pos.x.Pow(2) / 100) + (pos.y.Pow(2) / 49) <= 1;

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
