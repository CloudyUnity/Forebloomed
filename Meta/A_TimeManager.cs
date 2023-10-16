using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class A_TimeManager : MonoBehaviour
{
    public static A_TimeManager Instance;

    [SerializeField] float _totalTime;
    [SerializeField] ParticleSystem _fireflies, _windPS;
    [SerializeField] MakeInfo _sharkInfo;

    TimeData _data = new TimeData();
    float _goldPeak = float.MinValue;
    float _goldValley = float.MaxValue;

    float _timer;
    float _bonusTime;
    int _sharkCounter;
    bool _firefliesMultiplied;
    bool _dayEnded;

    public float TimeMult = 1;

    public int TotalSharks;
    public float TimePercent { get { return _timer / _totalTime; } }
    public float TimeRemaining { get { return _totalTime - _timer; } }

    private void OnEnable()
    {
        Instance = this;

        _data.ItemPos = new List<float>();
        _data.GoldPos = new List<Vector3>();
        _data.ItemSprites = new List<Sprite>();

        A_EventManager.OnGoldChange += MarkGold;
        A_EventManager.OnCollectItem += MarkItem;
    }

    private void OnDisable()
    {
        A_EventManager.OnGoldChange -= MarkGold;
        A_EventManager.OnCollectItem -= MarkItem;
    }

    private void Start()
    {
        MarkGold();
    }

    static bool _nightTrig, _nocTrig;
    private void Update()
    {
        if (Player.Instance == null || Player.Instance.Dead || O_Exit.Entered)
            return;

        _timer += Time.deltaTime * TimeMult;

        if (TimePercent > 0.6f && !_firefliesMultiplied)
        {
            if (_fireflies != null)
            {
                var fireEm = _fireflies.emission;
                fireEm.rateOverTimeMultiplier = 4;
            }

            if (_windPS != null)
            {
                var windEm = _windPS.emission;
                windEm.rateOverTimeMultiplier = 0;
            }

            _firefliesMultiplied = true;
        }

        if (TimePercent > 1 && !_dayEnded)
        {
            A_EventManager.InvokePlaySFX("Day End");
            _dayEnded = true;
        }

        if (TimeRemaining <= -30 && !_nightTrig)
        {
            A_EventManager.InvokeUnlock("Night Crawler");
            _nightTrig = true;
        }            

        if (TimeRemaining <= -200 && !_nocTrig)
        {
            A_EventManager.InvokeUnlock("Nocturnal");
            _nocTrig = true;
        }            

        if (TotalSharks >= 10)
            return;

        float sharkTime = 8;
        float sharkDelay = A_LevelManager.Instance.HardMode ? 2 : 5; 
        if (_timer > _totalTime + sharkDelay + _bonusTime + _sharkCounter * sharkTime)
        {
            A_Factory.Instance.MakeBasicOOB(_sharkInfo);
            _sharkCounter++;
            TotalSharks++;
        }
    }

    public void SetTime(float time) => _timer = time;

    public void IncreaseTime(float amount) => _bonusTime += amount;

    void MarkGold()
    {
        float amount;

        if (Player.Instance == null)
            amount = 0;
        else
         amount = Player.Instance.SoftStats.GoldAmount;

        _data.GoldPos.Add(new Vector3(_timer, amount));

        if (amount > _goldPeak)
            _goldPeak = amount;

        if (amount < _goldValley)
            _goldValley = amount;
    }

    void MarkItem(Item item)
    {
        _data.ItemPos.Add(_timer);
        _data.ItemSprites.Add(item.Data.Icon);
    }

    public TimeData GetData()
    {
        _data.TimeSpent = _timer;
        _data.DayTime = _totalTime;
        _data.GoldPeak = _goldPeak;
        _data.GoldValley = _goldValley;
        return _data;
    }
}

