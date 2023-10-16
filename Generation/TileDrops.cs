using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class TileDrops : MonoBehaviour
{
    public static TileDrops Instance;

    Dictionary<Vector2, GridData> _dict = new Dictionary<Vector2, GridData>();
    List<MineResult> _freqData = new List<MineResult>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _freqData = GenDataManager.Instance.GetGenData().DropTable;
    }

    MakeInfo FindType(Vector2 pos)
    {
        double seed = (Mathf.Abs(pos.x) * 2355267 + 7352736) * (Mathf.Abs(pos.y) * 51546283 + 375476264) * A_LevelManager.QuickSeed;
        return SelectRandomWeightedItem(_freqData, (float)seed).Info;
    }

    void MakeGridType(Vector2 pos)
    {
        GridData grid = new GridData { Pos = pos, Info = FindType(pos) };
        _dict.Add(pos, grid);
    }

    public MakeInfo GetGridType(Vector2 pos)
    {
        if (!_dict.ContainsKey(pos))
            MakeGridType(pos);

        return _dict[pos].Info;
    }

    public void SetGridType(Vector2 pos, MakeInfo info)
    {
        _dict[pos] = new GridData() { Pos = pos, Info = info };
    }

    MineResult SelectRandomWeightedItem(List<MineResult> data, float seed)
    {
        float totalFrequency = 0;
        foreach (var item in data)
        {
            totalFrequency += item.Freq;
        }

        float val = A_Extensions.RandomBetween(1, totalFrequency + 1, seed);
        foreach (var item in data)
        {
            if (val <= item.Freq)
            {
                return item;
            }
            val -= item.Freq;
        }

        return _freqData.RandomItem(seed);
    }
}

[System.Serializable]
public struct MineResult
{
    public MakeInfo Info;
    public float Freq;
}

[System.Serializable]
public struct GridData
{
    public Vector2 Pos;
    public MakeInfo Info;
}
