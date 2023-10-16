using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class O_DebugEnemySpawner : MonoBehaviour
{
    [SerializeField] GameObject _prefab;
    [SerializeField] int _amount;
    [SerializeField] bool _perFrame;

    private void Start()
    {
        if (_perFrame)
            return;

        for (int i = 0; i < _amount; i++)
        {
            Instantiate(_prefab, new Vector3(-5, 0), Quaternion.identity);
        }
    }

    private void Update()
    {
        if (!_perFrame)
            return;

        for (int i = 0; i < _amount; i++)
        {
            Instantiate(_prefab, new Vector3(-5, 0), Quaternion.identity);
        }
    }
}
