using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DeadEye", menuName = "Item/DeadEye", order = 1)]
public class J_DeadEye : ItemData
{
    [SerializeField] float _maxIncrease = 5;
    [SerializeField] float _decreaseSpeed = 0.2f;
    float _kills;

    public override void OnPickup()
    {
        ResetCounter();
    }

    public override void OnLoadItem()
    {
        ResetCounter();
    }

    public override void AfterTime()
    {
        _kills -= Time.deltaTime * _decreaseSpeed;
        AddPlayerStats.FireRate = -Mathf.Clamp(_kills, 0, _maxIncrease);
    }

    private void ResetCounter()
    {
        _kills = 0;
        AddPlayerStats.FireRate = 0;
    }

    public override void OnEntityDie(Entity e)
    {
        if (_kills < 0)
            _kills = 0;
        _kills++;
    }
}
