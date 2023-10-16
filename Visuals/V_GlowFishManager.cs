using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class V_GlowFishManager : MonoBehaviour
{
    [SerializeField] int _numGlowFish;
    [SerializeField] MakeInfo _fishInfo;
    [SerializeField] float _spawnTime;

    bool fishSpawned;

    private void OnEnable()
    {
        A_EventManager.OnMakeFish += MakeFish;
    }

    private void OnDisable()
    {
        A_EventManager.OnMakeFish -= MakeFish;
    }

    void Update()
    {
        if (fishSpawned || (A_TimeManager.Instance != null && A_TimeManager.Instance.TimePercent < _spawnTime))
            return;

        for (int i = 0; i < _numGlowFish; i++)
        {
            MakeFish();
        }
        fishSpawned = true;
    }

    void MakeFish()
    {
        A_Factory.Instance.MakeBasicOOB(_fishInfo);
    }
}
