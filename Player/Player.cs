using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance;

    public static GameObject Selected;

    public Vector2 PlayerInput;
    public int CharacterIndex;

    Vector2 _lastInput;
    [SerializeField] Rigidbody2D _rb;
    [SerializeField] CircleCollider2D _magnet;
    [SerializeField] CapsuleCollider2D _mainCol;
    [SerializeField] PlayerAnim Animation;
    [SerializeField] ParticleSystem _leafPS;
    [SerializeField] ParticleSystem _potPS;
    [SerializeField] GameObject _dermalShield;
    public float _invincibiltyFramesDur;
    [SerializeField] float KnockbackMagnitude;
    public float _invincibiltyFramesTimer = 99;
    float _footStepTimer;

    public PlayerStats DefaultStats;
    public PlayerStats CurStats;
    public PlayerSoftStats SoftStats;
    public Sprite DeathImg;
    public Color[] PlayerColors;
    public List<StatusEffect> StatusEffects;

    public int FreeHits;

    public Sprite AbilityIcon;
    public float AbilityPercent;

    public bool TookDamage;
    public bool Teleporting;
    public bool InDialogue;
    public bool InCutscene;

    public List<O_Follower> FollowItems = new List<O_Follower>();
    public List<GameObject> Keys = new List<GameObject>();
    public List<GameObject> GoldKeys = new List<GameObject>();

    [SerializeField] protected GameObject _keyFollower;
    [SerializeField] protected GameObject _goldKeyFollower;

    public bool HasInvincibilty { get { return _invincibiltyFramesTimer < _invincibiltyFramesDur && !Dead;  } }
    public bool Dead { get { return SoftStats.CurHealth <= 0 && SoftStats.BonusHealth <= 0;  } }
    bool TakingKnockback { get { return _invincibiltyFramesTimer < 0.2f; } }
    public bool Stopped { get { return Teleporting || InDialogue || InCutscene || Dead || _exited || V_HUDManager.Instance.IsPaused; } }
    public bool CanPause { get { return !InDialogue && !InCutscene && !Dead && !_exited && !Teleporting; } }
    public bool UsingAbility;

    [SerializeField] protected Transform[] _ghosts;

    KeyCode upKey, downKey, leftKey, rightKey, shootKey;

    bool _exited;
    bool _squashing;

    public int AbilityUpgrades;

    bool _daredevilWorthy;

    bool _alreadyDead;

    int _knockBacksThisFrame = 0;

    public static readonly bool DEBUG_INVINCIBLE = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public virtual void OnEnable()
    {
        A_EventManager.OnDealDamage += TakeDamage;
        A_EventManager.OnNextScene += TouchedExit;
    }

    public virtual void OnDisable()
    {
        A_EventManager.OnDealDamage -= TakeDamage;
        A_EventManager.OnNextScene -= TouchedExit;
    }

    bool ShootInput() => Input.GetKey(shootKey) || A_InputManager.Instance.GamepadShoot();
    bool ShootInputDown() => Input.GetKeyDown(shootKey) || A_InputManager.Instance.GamepadShootDown();

    public virtual void Start()
    {
        if (_hidden)
            Hide(false);

        if (SoftStats.CurHealth == 0)
            SoftStats.CurHealth = SoftStats.MaxHealth;

        if (SoftStats.CurHealth + SoftStats.BonusHealth == 1)
            _daredevilWorthy = true;

        CurStats = DefaultStats;

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
    }

    void GetInput()
    {
        if (A_InputManager.Instance == null)
            return;

        if (A_InputManager.GamepadMode)
        {
            PlayerInput.x = Input.GetAxis("Horizontal");
            PlayerInput.y = Input.GetAxis("Vertical");

            if (PlayerInput.x != 0) 
                _lastInput.x = PlayerInput.x;
            if (PlayerInput.y != 0)
                _lastInput.y = PlayerInput.y;
            return;
        }

        upKey = A_InputManager.Instance.Key("Up");
        downKey = A_InputManager.Instance.Key("Down");
        leftKey = A_InputManager.Instance.Key("Left");
        rightKey = A_InputManager.Instance.Key("Right");
        shootKey = A_InputManager.Instance.Key("Shoot");

        PlayerInput.x = Input.GetKey(rightKey) ? 1 : Input.GetKey(leftKey) ? -1 : 0;
        PlayerInput.y = Input.GetKey(upKey) ? 1 : Input.GetKey(downKey) ? -1 : 0;

        if (PlayerInput.x != 0) _lastInput.x = PlayerInput.x;
        if (PlayerInput.y != 0) _lastInput.y = PlayerInput.y;
    }

    public virtual void Update()
    {
        if (ItemManager.Instance == null)
            return;

        CurStats = ItemManager.Instance.ItemPlayerStats();
        StatusEffects = ItemManager.Instance.AllStatusEffects();

        if (A_InputManager.Instance == null || A_LevelManager.Instance == null)
            return;        

        Vector2 targetScale = Teleporting ? Vector2.zero : Vector2.one * Mathf.Clamp(CurStats.Size, 0.4f, 1.5f);

        if (!_squashing && !InCutscene)
            transform.localScale = Vector2.Lerp(transform.localScale, targetScale, Time.deltaTime * 8);

        _magnet.radius = CurStats.MagnetRange;

        _invincibiltyFramesTimer += Time.deltaTime;

        _dermalShield.SetActive(FreeHits > 0);

        ClampHealth();
        FootStep();        

        if (Stopped || UsingAbility)
        {
            PlayerInput = Vector2.zero;
            _rb.velocity = Vector2.zero;
            return;
        }

        GetInput();

        _timeSinceClick += Time.deltaTime;

        if (ShootInputDown())
            _timeSinceClick = 0;

        if (ShootInput() && _timeSinceClick > 0.3f)
        {
            CurStats.Speed *= CurStats.RecoilMult;
        }
    }

    float _timeSinceClick = 999;

    private void FixedUpdate()
    {
        if (Stopped) 
            return;

        _knockBacksThisFrame = 0;

        float speed = Mathf.Clamp(CurStats.Speed, 1.5f, 7);
        _rb.velocity = PlayerInput.normalized * speed;
    }

    static bool _bugCatcherTriggered, _bugTrawlerTriggered, _heartyTriggered;
    public virtual void AddSoftStats(PlayerSoftStats add)
    {
        if (Dead || _exited)
            return;

        SoftStats = new PlayerSoftStats
        {
            GoldAmount = SoftStats.GoldAmount + add.GoldAmount,
            CurHealth = SoftStats.CurHealth + add.CurHealth,
            BonusHealth = SoftStats.BonusHealth + add.BonusHealth,
            MaxHealth = SoftStats.MaxHealth + add.MaxHealth,
            Keys = SoftStats.Keys + add.Keys,
            GoldKeys = SoftStats.GoldKeys + add.GoldKeys,
        };

        if (add.GoldAmount > 0)
        {
            A_EventManager.InvokePlaySFX("Gold");
            A_EventManager.InvokeCollectGold(add.GoldAmount);

            if (SoftStats.GoldAmount >= 100 && !_bugCatcherTriggered)
            {
                A_EventManager.InvokeUnlock("Bug Catcher");
                _bugCatcherTriggered = true;
            }                

            if (SoftStats.GoldAmount >= 300 && !_bugTrawlerTriggered)
            {
                A_EventManager.InvokeUnlock("Bug Trawler");
                _bugTrawlerTriggered = true;
            }                
        }

        if (add.CurHealth > 0 || add.BonusHealth > 0)
        {
            A_EventManager.InvokePlaySFX("Heal");
            _daredevilWorthy = false;
        }

        if (SoftStats.MaxHealth + SoftStats.BonusHealth >= 15 && !_heartyTriggered)
        {
            A_EventManager.InvokeUnlock("Hearty");
            _heartyTriggered = true;
        }            

        Squash(0.1f);
    }

    public virtual void TakeDamage(int amount, Vector2 from, string name)
    {
        if (HasInvincibilty || amount <= 0 || Stopped || TakingKnockback || Dead)
            return;

        if (DEBUG_INVINCIBLE)
        {
            Debug.Log("Damage blocked, debug invincible");
            return;
        }

        if (SoftStats.CurHealth > SoftStats.MaxHealth)
            SoftStats.CurHealth = SoftStats.MaxHealth;

        if (A_LevelManager.Instance.DifficultyModifier >= 4)
            amount *= 2;

        Squash(_invincibiltyFramesDur / 5);

        if (A_TimeManager.Instance != null && A_TimeManager.Instance.TimeRemaining >= 110 && A_LevelManager.Instance.CurrentLevel == 1)
            A_EventManager.InvokeUnlock("Expression");

        if (FreeHits > 0)
        {
            FreeHits--;
            _invincibiltyFramesTimer = 0;
            _leafPS.Play();
            A_EventManager.InvokeCameraShake(0.01f, 0.2f);
            A_EventManager.InvokePlaySFX("SwainBlock");
            return;
        }

        if (amount >= SoftStats.BonusHealth)
        {
            SoftStats.CurHealth -= amount - SoftStats.BonusHealth;
            SoftStats.BonusHealth = 0;
        }
        else SoftStats.BonusHealth = SoftStats.BonusHealth - amount;

        A_EventManager.InvokeDealtDamage(amount, name);

        if (Dead)
        {            
            Die();

            if (name == "Astral Shark")
                A_EventManager.InvokeUnlock("Crunch");

            if (name == "Bomb" || name == "Exploding Ant")
                A_EventManager.InvokeUnlock("Careless");

            return;
        }

        AfterDamage(from);
    }

    public void AfterDamage(Vector2 from)
    {
        _leafPS.Play();
        _potPS.Play();

        TookDamage = true;

        Vector2 dir = ((Vector2)transform.position - from).normalized;
        _rb.AddForce(dir * KnockbackMagnitude, ForceMode2D.Impulse);

        A_EventManager.InvokePlaySFX("Hurt");
        A_EventManager.InvokeCameraShake(0.01f, 0.2f);

        Animation.TookDamage();
        _invincibiltyFramesTimer = 0;
    }

    void Die()
    {
        if (A_LevelManager.Instance.ExtraLives > 0)
        {
            _leafPS.Play();
            _potPS.Play();
            A_EventManager.InvokeCameraShake(0.01f, 0.3f);
            A_EventManager.InvokePlaySFX("Dead");
            Squash(0.5f);

            SoftStats.CurHealth = 0;
            SoftStats.BonusHealth = 0;

            DisablePlayer();
            InCutscene = true;
            _invincibiltyFramesTimer = -5;

            StartCoroutine(C_Revive());

            return;
        }

        if (_alreadyDead)
            return;
        _alreadyDead = true;

        SoftStats.CurHealth = 0;
        SoftStats.BonusHealth = 0;

        _leafPS.Play();
        _potPS.Play();

        A_EventManager.InvokePlayerDied();
        A_EventManager.InvokeCameraShake(0.01f, 0.3f);
        A_EventManager.InvokePlaySFX("Dead");

        Squash(0.5f);        
        DisablePlayer();
    }

    IEnumerator C_Revive()
    {
        A_EventManager.InvokePlayerRevived();

        yield return new WaitForSeconds(2);

        A_LevelManager.Instance.ExtraLives--;
        A_EventManager.InvokeDestroy1Ups();
        A_EventManager.InvokePlaySFX("1UP");
        H_1UP.DestroyedOne = false;

        yield return new WaitForSeconds(0.4f);

        _invincibiltyFramesTimer = -5;
        TookDamage = true;

        if (SoftStats.MaxHealth > 0)
            SoftStats.CurHealth = 1;
        else
            SoftStats.BonusHealth = 1;

        A_EventManager.InvokePlaySFX("Revive");

        _mainCol.enabled = true;

        InCutscene = false;
    }

    public Vector3 HitBox => new Vector2(transform.position.x, transform.position.y - 0.25f);

    public GameObject TargetPos()
    {
        if (_ghosts.Length == 0)
        {
            Debug.Log("Missing Ghosts");
            return gameObject;
        }

        int rand = Random.Range(-1, _ghosts.Length);
        return rand == -1 ? gameObject : _ghosts[rand].gameObject;
    }

    void TouchedExit()
    {
        DisablePlayer();
        _exited = true;

        if (!TookDamage && !A_LevelManager.Instance.CurrentLevel.IsEven())
        {
            A_EventManager.InvokeUnlock("Perfect Day");
        }

        if (_daredevilWorthy && SoftStats.BonusHealth + SoftStats.CurHealth == 1 && !A_LevelManager.Instance.CurrentLevel.IsEven())
            A_EventManager.InvokeUnlock("Daredevil");
    }

    void DisablePlayer()
    {
        PlayerInput = Vector2.zero;
        _rb.velocity = Vector2.zero;
        _mainCol.enabled = false;
    }

    public void CheckHealth()
    {
        if (SoftStats.CurHealth > SoftStats.MaxHealth)
            SoftStats.CurHealth = SoftStats.MaxHealth;

        if (Dead)
        {
            Die();
        }
    }

    void FootStep()
    {
        if (PlayerInput != Vector2.zero)
            _footStepTimer += Time.deltaTime;
        else
            _footStepTimer = 0;

        if (_footStepTimer > 0.5f)
        {
            _footStepTimer = 0;

            if (A_LevelManager.Instance.CurrentLevel > 11)
            {
                A_EventManager.InvokePlaySFX("Step3");
                return;
            }
            if (A_LevelManager.Instance.CurrentLevel > 5)
            {
                A_EventManager.InvokePlaySFX("Step2");
                return;
            }
            A_EventManager.InvokePlaySFX("Step1");
            return;
        }
    }

    void ClampHealth()
    {
        SoftStats.MaxHealth = Mathf.Clamp(SoftStats.MaxHealth, 0, 15);
        int remaining = 15 - SoftStats.MaxHealth;
        SoftStats.BonusHealth = Mathf.Clamp(SoftStats.BonusHealth, 0, remaining);

        SoftStats.CurHealth = Mathf.Clamp(SoftStats.CurHealth, 0, SoftStats.MaxHealth);
    }

    public void Squash(float dur) => StartCoroutine(C_SqaushStretch(dur));
    IEnumerator C_SqaushStretch(float dur)
    {
        if (_squashing)
            yield break;
         
        _squashing = true;
        float elapsed = 0;
        Vector2 start = transform.localScale;

        while (elapsed < dur)
        {
            float humped = 1 - A_Extensions.HumpCurve(elapsed / dur, 0, 1);
            transform.localScale = start + new Vector2(0.1f * -humped, 0.1f * humped);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = start;
        _squashing = false;
    }

    public void KnockBack(Vector3 dir, float magnitude)
    {
        if (PlayerInput != Vector2.zero || _knockBacksThisFrame > 5)
            return;

        _knockBacksThisFrame++;
        transform.position += dir * magnitude;
    }

    public void UseKey()
    {
        if (SoftStats.Keys <= 0)
            return;

        if (Keys.Count > 0)
        {
            GameObject key = Keys[Keys.Count - 1];

            FollowItems.Remove(key.GetComponent<O_Follower>());
            Keys.Remove(key);
            Destroy(key);

            SoftStats.Keys--;
        }
    }

    public void UseBigKey()
    {
        GameObject key = GoldKeys[GoldKeys.Count - 1];

        FollowItems.Remove(key.GetComponent<O_Follower>());
        Keys.Remove(key);
        Destroy(key);        

        SoftStats.GoldKeys--;
    }

    public void GainKey()
    {
        GameObject go = Instantiate(_keyFollower, transform.position, Quaternion.identity);
        Keys.Add(go);

        SoftStats.Keys++;
    }

    public void GainBigKey()
    {
        GameObject go = Instantiate(_goldKeyFollower, transform.position, Quaternion.identity);
        GoldKeys.Add(go);
        SoftStats.GoldKeys++;
    }

    static protected bool _hidden;
    public void Hide(bool show)
    {
        Animation.enabled = show;
        GetComponent<SpriteRenderer>().enabled = show;
        foreach (var child in transform.GetComponentsInChildren<SpriteRenderer>())
        {
            child.enabled = show;
        }
        _hidden = !show;
    }
}

[System.Serializable]
public struct PlayerStats
{
    public float Speed;
    public float Size;
    public float Damage;
    public float BulletSize;
    public float Range;
    public float BulletSpeed;
    public float Luck;
    public float FireRate;
    public float MagnetRange;
    public float Homing;
    public float Piercing;
    public float Spread;
    public float Amount;
    public float AmountRange;
    public float Delay;
    public float RecoilMult;

    public static PlayerStats operator +(PlayerStats a, PlayerStats b)
    {
        float frScale = Mathf.Clamp01(Mathf.Abs(a.FireRate) / Player.Instance.DefaultStats.FireRate);
        float negHomScale = 1.0f - Mathf.Clamp01(Mathf.Abs(a.Homing) / 4.5f);
        float homScale = 1.0f - Mathf.Clamp01(Mathf.Abs(a.Homing) / 10f);
        float dmgScale = 1.0f - Mathf.Clamp01(Mathf.Abs(a.Damage) / 15f);
        float bszeScale = 1.0f - Mathf.Clamp01(Mathf.Abs(a.BulletSize) / 1.5f);

        return new PlayerStats
        {
            Speed = a.Speed + b.Speed,
            Size = a.Size + b.Size,
            Damage = a.Damage + b.Damage * dmgScale,
            BulletSize = a.BulletSize + b.BulletSize * bszeScale,
            Range = a.Range + b.Range,
            BulletSpeed = a.BulletSpeed + b.BulletSpeed,
            Luck = a.Luck + b.Luck,
            FireRate = a.FireRate + b.FireRate * frScale,
            MagnetRange = a.MagnetRange + b.MagnetRange,
            Homing = a.Homing + b.Homing * (b.Homing < 0 ? negHomScale : homScale),
            Piercing = a.Piercing + b.Piercing,
            Spread = a.Spread + b.Spread,
            Amount = a.Amount + b.Amount,
            AmountRange = a.AmountRange + b.AmountRange,
            Delay = a.Delay + b.Delay,
            RecoilMult = a.RecoilMult + b.RecoilMult,
        };
    }

    public static PlayerStats operator *(PlayerStats a, PlayerStats b)
    {
        a.Speed *= b.Speed == 0 ? 1 : b.Speed;
        a.Size *= b.Size == 0 ? 1 : b.Size;
        a.Damage *= b.Damage == 0 ? 1 : b.Damage;
        a.BulletSpeed *= b.BulletSpeed == 0 ? 1 : b.BulletSpeed;
        a.BulletSize *= b.BulletSize == 0 ? 1 : b.BulletSize;
        a.Range *= b.Range == 0 ? 1 : b.Range;
        a.Luck *= b.Luck == 0 ? 1 : b.Luck;
        a.FireRate *= b.FireRate == 0 ? 1 : b.FireRate; 
        a.MagnetRange *= b.MagnetRange == 0 ? 1 : b.MagnetRange;
        a.Piercing *= b.Piercing == 0 ? 1 : b.Piercing;
        a.Homing *= b.Homing == 0 ? 1 : b.Homing;
        a.Spread *= b.Spread == 0 ? 1 : b.Spread;
        a.Amount *= b.Amount == 0 ? 1 : b.Amount;
        a.AmountRange *= b.AmountRange == 0 ? 1 : b.AmountRange;
        a.Delay *= b.Delay == 0 ? 1 : b.Delay;
        a.RecoilMult *= b.RecoilMult == 0 ? 1 : b.RecoilMult;

        if (b.Damage == 404)
            a.Damage = 0;

        return a;
    }

    public static PlayerStats one
    {
        get
        {
            return new PlayerStats()
            {
                Speed = 1,
                Size = 1,
                Damage = 1,
                BulletSize = 1,
                Range = 1,
                BulletSpeed = 1,
                Luck = 1,
                FireRate = 1,
                MagnetRange = 1,
                Homing = 1,
                Piercing = 1,
                Spread = 1,
                Amount = 1,
                AmountRange = 1,
                Delay = 1,
                RecoilMult = 1,
            };
        }
    }
}

[System.Serializable]
public struct PlayerSoftStats
{
    public int GoldAmount;
    public int CurHealth;
    public int MaxHealth;
    public int BonusHealth;
    public int Keys;
    public int GoldKeys;
}

[System.Serializable]
public struct StatusEffect
{
    public string Name;
    public float Freq;
    public float FreqCounter;
    public float Dur;
    public float AddDur;
    public float Power;
    public float Chance;

    public static StatusEffect operator +(StatusEffect a, StatusEffect b)
    {
        return new StatusEffect
        {
            Name = a.Name,
            Freq = Mathf.Max(a.Freq, b.Freq),
            Dur = Mathf.Max(a.Dur, b.Dur) + a.AddDur + b.AddDur,
            Power = a.Power + b.Power,
            Chance = Mathf.Max(a.Chance, b.Chance) + (Mathf.Min(a.Chance, b.Chance) == -1 ? 0 : 0.05f),
        };
    }
}