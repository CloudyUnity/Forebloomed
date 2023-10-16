using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilePresetData
{
    public TilePreset tilePreset;

    public TilePresetData(TilePreset preset)
    {
        tilePreset = preset;
    }

    public float GetDiffValue(float height, float difficulty, float danger)
    {
        return (height - tilePreset.MinHeight) + (difficulty - tilePreset.MinDifficulty) + (danger - tilePreset.MinAlt);
    }
}
