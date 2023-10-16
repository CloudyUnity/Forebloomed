using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ecballium", menuName = "Item/Ecballium", order = 1)]
public class J_Ecballium : ItemData
{
    public Vector2 RateRange;
    public Vector2 SpreadRange;
    public float TimeToMax;
    float _heldTimer;

    KeyCode _key;

    public override void OnPickup()
    {
        _heldTimer = 0;
        _key = A_InputManager.Instance.Key("Shoot");
    }

    public override void OnHit()
    {
        _heldTimer = 0;
    }

    public override void OnLoadItem()
    {
        _key = A_InputManager.Instance.Key("Shoot");
    }

    public override void AfterTime()
    {
        if (Input.GetKey(_key))
            _heldTimer += Time.deltaTime;
        else
            _heldTimer = 0;

        float percent = Mathf.Clamp01(_heldTimer / TimeToMax);
        AddPlayerStats.FireRate = Mathf.Lerp(RateRange.x, RateRange.y, percent);
        AddPlayerStats.Spread = Mathf.Lerp(SpreadRange.x, SpreadRange.y, percent);
    }
}
