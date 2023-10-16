using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GenData", menuName = "Gen/GenData", order = 1)]
public class GenData : ScriptableObject
{
    public int MapSize = 100;
    public float DistanceMult = 0.001f;
    public float OWChance = 1;

    public GameObject[] Minibosses;

    public TileWave[] HeightWaves;
    public TileWave[] DiffWaves;
    public TileWave[] AltWaves;

    public TilePreset[] Tiles;

    public List<MineResult> DropTable = new List<MineResult>();

    public int ChunkSize => A_OptionsManager.Instance.Current.CamLock ? 11 : 10;
}
