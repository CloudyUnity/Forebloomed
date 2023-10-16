using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenDataManager : MonoBehaviour
{
    public static GenDataManager Instance;

    public GenMode Mode = null;

    private void Awake()
    {
        Instance = this;
    }

    public GenData GetGenData()
    {
        if (Mode == null)
            throw new System.Exception("Mode not found, cmon guys...");

        int level = A_LevelManager.Instance.CurrentLevel;
        int loop = A_LevelManager.Instance.DifficultyModifier;

        int loopIndex = loop - 1;
        if (loopIndex >= Mode.AllGenLoops.Length)
        {
            loopIndex = Mode.AllGenLoops.Length - 1;
        }

        GenLoop genLoop = Mode.AllGenLoops[loopIndex];

        int levelIndex = ((level + 1) / 2) - 1;
        return genLoop.AllGenData[levelIndex];
    }
}
