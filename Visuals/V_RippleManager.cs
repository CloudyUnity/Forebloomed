using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class V_RippleManager : MonoBehaviour
{
    [SerializeField] GameObject _ripplePS, _splashPS;
    bool _assigned;

    private void Update()
    {
        if (_assigned || Player.Instance == null)
            return;

        Instantiate(_ripplePS, Player.Instance.transform);
        Instantiate(_splashPS, Player.Instance.transform);
        _assigned = true;   
    }

    private void OnEnable()
    {
        A_EventManager.OnEntitySpawn += AssignRipple;
    }

    private void OnDisable()
    {
        A_EventManager.OnEntitySpawn -= AssignRipple;   
    }


    void AssignRipple(Entity entity)
    {
        if (entity.Flying)
            return;

        Instantiate(_ripplePS, entity.transform);
        Instantiate(_splashPS, entity.transform);
    }
}
