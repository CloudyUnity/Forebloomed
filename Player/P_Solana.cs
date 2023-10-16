using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P_Solana : Player
{
    [SerializeField] float _goldLossCooldown;
    [SerializeField] MakeInfo _goldInfo;
    [SerializeField] GameObject _fakeGold;
    [SerializeField] float _abilityCooldown;
    float _abilityTimer;
    float _goldTimer;
    bool _bossDead;

    KeyCode _ability;

    public bool AbilityButtonDown() => Input.GetKeyDown(_ability) || A_InputManager.Instance.GamepadAbility();

    public override void OnEnable()
    {
        base.OnEnable();

        A_EventManager.OnPricedHeartSpawn += DestroyHeart;
        A_EventManager.OnBossDie += (_) => _bossDead = true;
    }

    public override void OnDisable()
    {
        base.OnDisable();

        A_EventManager.OnPricedHeartSpawn -= DestroyHeart;
        A_EventManager.OnBossDie -= (_) => _bossDead = true;
    }

    public override void Start()
    {
        if (_hidden)
            Hide(false);

        _ghosts[0].parent.parent = null;
        _ghosts[0].parent.transform.position = Vector2.zero;

        for (int i = 0; i < SoftStats.Keys; i++)
        {
            GameObject go = Instantiate(_keyFollower, transform.position, Quaternion.identity);
            Keys.Add(go);
        }

        for (int i = 0; i < SoftStats.GoldKeys; i++)
        {
            GameObject go = Instantiate(_goldKeyFollower, transform.position, Quaternion.identity);
            GoldKeys.Add(go);
        }

        _goldLossCooldown /= A_LevelManager.Instance.DifficultyModifier;
        _ability = A_InputManager.Instance.Key("Ability");
    }

    public void DestroyHeart(O_PricedHeart h)
    {
        Destroy(h.gameObject);
    }

    public override void Update()
    {
        base.Update();

        if (Dead)
            return;

        TickDownGold();

        _abilityTimer += Time.deltaTime;

        float cooldown = _abilityCooldown / (AbilityUpgrades + 1);
        if (_abilityTimer < cooldown)
            AbilityPercent = _abilityTimer / cooldown;
        else
            AbilityPercent = 1;

        if (AbilityButtonDown() && AbilityPercent >= 1)
        {
            _abilityTimer = 0;
            A_EventManager.InvokeUseAbility();
        }            

        SoftStats.CurHealth = 1;
        SoftStats.MaxHealth = 2;
        SoftStats.BonusHealth = 0;

        if (SoftStats.GoldAmount <= 0 && !Dead)
            DieFromPoor();
    }

    void TickDownGold()
    {
        if (A_LevelManager.Instance.CurrentLevel.IsEven() || _bossDead || Dead)
            return;

        _goldTimer += Time.deltaTime;
        if (_goldTimer >= _goldLossCooldown)
        {
            SoftStats.GoldAmount--;
            _goldTimer = 0;
        }
    }

    public override void AddSoftStats(PlayerSoftStats add)
    {
        if (Dead)
            return;

        int goldGain = (Mathf.Clamp(add.CurHealth, 0, 5) + add.MaxHealth + add.BonusHealth) * 25;

        add.CurHealth = 0;
        add.MaxHealth = 0;
        add.BonusHealth = 0;

        _goldInfo.Amount = Vector2.one * goldGain;
        A_Factory.Instance.MakeBasic(transform.position, _goldInfo);

        base.AddSoftStats(add);
    }

    public override void TakeDamage(int amount, Vector2 from, string name)
    {
        if (amount <= 0 || HasInvincibilty || Stopped || Dead)
            return;

        int goldLost = (int)(SoftStats.GoldAmount * 0.25f);

        SoftStats.GoldAmount -= goldLost;
        StartCoroutine(C_FlingFakeGold(Mathf.Clamp(goldLost, 0, 40)));
        TookDamage = true;
        if (SoftStats.GoldAmount <= 0)
            DieFromPoor();
        else
            AfterDamage(from);
    }

    void DieFromPoor()
    {
        if (HasInvincibilty || Stopped || DEBUG_INVINCIBLE || A_LevelManager.Instance.SceneTime < 5 || Dead)
            return;

        base.TakeDamage(999, transform.position, "Being poor");
    }

    IEnumerator C_FlingFakeGold(int amount)
    {
        float e = 0;
        float d = 2;

        Vector3 start = transform.position;

        List<GameObject> gold = new List<GameObject>();
        for (int i = 0; i < amount; i++)
        {
            float angle = i * (360 / amount);
            gold.Add(Instantiate(_fakeGold, start, Quaternion.AngleAxis(angle, Vector3.back)));
        }
        
        while (e < d)
        {
            for (int i = 0; i < amount; i++)
            {
                gold[i].transform.position = Vector3.Lerp(start, start + gold[i].transform.up * 10, e / d);
                gold[i].transform.localScale = Vector3.one * Mathf.Lerp(0.5f, 0, e / d);
            }

            e += Time.deltaTime;
            yield return null;
        }
        for (int i = amount - 1; i >= 0; i--)
        {
            Destroy(gold[i]);
        }
    }
}
