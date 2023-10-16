using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AngryGold", menuName = "Item/Snail", order = 1)]
public class J_Snail : ItemData
{
    [SerializeField] MakeInfo _info;
    [SerializeField] float _offset;
    [SerializeField] Vector2 _timeRange;
    [SerializeField] Vector2 _spawnPos;
    [SerializeField] float _spawnDelay;

    float _spawnTimer;
    float _snailAmount = 0;

    public override void OnLoadItem()
    {
        if (A_LevelManager.Instance == null || A_LevelManager.Instance.CurrentLevel.IsEven() || A_LevelManager.Instance.BossLevel)
            return;

        _snailAmount++;
        _spawnTimer = 0;
        _timer = 0;
        _dur = _timeRange.AsRange();
    }

    public override void OnPickup()
    {
        if (A_LevelManager.Instance == null || A_LevelManager.Instance.CurrentLevel.IsEven() || A_LevelManager.Instance.BossLevel)
            return;

        _snailAmount++;
        _spawnTimer = 0;
        _timer = 0;
        _dur = _timeRange.AsRange();
    }

    float _timer = 0;
    float _dur = 0;
    public override void AfterTime()
    {
        if (A_LevelManager.Instance == null || A_LevelManager.Instance.CurrentLevel.IsEven() || A_LevelManager.Instance.BossLevel)
            return;

        _spawnTimer += Time.deltaTime;
        if (_spawnTimer > _spawnDelay)
        {
            for (int i = 0; i < _snailAmount; i++)
                A_Factory.Instance.MakeBasic(_spawnPos, _info);
            _spawnTimer = float.MinValue;
            _snailAmount = 0;
        }            

        _timer += Time.deltaTime;
        if (_timer > _dur)
        {
            Player.Instance.SoftStats.GoldAmount += A_LevelManager.Instance.DifficultyModifier;
            _timer = 0;
            _dur = _timeRange.AsRange();
        }
    }
}
