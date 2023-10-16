using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Creep", menuName = "Item/Creep", order = 1)]
public class J_Creep : ItemData
{
    [SerializeField] float _cooldown;
    [SerializeField] GameObject _creep;
    float _timer;
    public override void AfterTime()
    {
        _timer += Time.deltaTime;
        if (_timer > _cooldown)
        {
            Instantiate(_creep, Player.Instance.transform.position, Quaternion.identity);
            _timer = 0;
        }
    }
}
