using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class O_Spawner : MonoBehaviour
{
    [SerializeField] float _cooldown;
    [SerializeField] MakeInfo _info;
    float _timer;
    
    private void Update()
    {
        _timer += Time.deltaTime;
        
        if (_timer > _cooldown)
        {
            A_Factory.Instance.MakeBasic(transform.position, _info);
            _timer = 0;
        }
    }
}
