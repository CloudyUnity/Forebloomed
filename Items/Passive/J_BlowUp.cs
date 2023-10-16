using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BlowUp", menuName = "Item/BlowUp", order = 1)]
public class J_BlowUp : ItemData
{
    [SerializeField] MakeInfo info;
    [SerializeField] float _chance;
    [SerializeField] float _cooldown;
    [SerializeField] bool _allowContact;
    float _timer;

    public override void OnContact()
    {
        if (_timer < _cooldown || !_allowContact || A_LevelManager.Instance == null || A_LevelManager.Instance.CurrentLevel.IsEven())
            return;

        if (Random.Range(0, 100) >= _chance || (A_TimeManager.Instance != null && A_TimeManager.Instance.TimePercent > 1))
            return;

        A_Factory.Instance.MakeBasic(Player.Instance.transform.position, info);
        _timer = 0;
    }

    public override void AfterTime()
    {
        _timer += Time.deltaTime;
    }

    //public override void OnHit()
    //{
    //    if (_timer < _cooldown || A_TimeManager.Instance == null)
    //        return;

    //    if (Random.Range(0, 100) >= _chance || A_TimeManager.Instance.TimePercent > 1)
    //        return;

    //    A_Factory.Instance.MakeBasic(Player.Instance.transform.position, info);
    //    _timer = 0;
    //}
}
