using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class A_LevelManager : MonoBehaviour
{
    public static A_LevelManager Instance;

    public float Seed;
    public bool SeededRun = false;
    public bool HardMode = false;

    public static float QuickSeed;

    public int CurrentLevel;
    public int Kills;
    public float GlobalTime;
    public float SceneTime;
    public int GoldGained;
    public int GoldSpent;
    public int DamageTaken;
    public int DifficultyModifier = 1;
    public bool TookDamage;
    public float TimeSinceChestSpawn;
    public int BoughtOutItems;

    public int ExtraLives = 0;

    public int AiderbotCount;
    public int TelestumpsUsed;
    public int MiniBossesKilled;
    public int PotsBroken;
    public int ShopkeeperTalks; 
    public int TotalGoldSpent;
    public bool BossLevel => CurrentLevel.Is(5, 11, 17, 23);
    public bool IsAltBoss;

    float _retryTime;
    int _loadingLevel;

    public int BulletCount;
    public int EntityCount;
    public int CorpseCount;
    public int HeartCount;    

    public Vector3 MiniBossPos;

    KeyCode _retryKey = KeyCode.None;

    public List<ObjectWorld> AllWorldObjects = new List<ObjectWorld>();

    const int CORPSE_CAP = 9999;

    GameObject _cursorGO = null;
    [SerializeField] GameObject _cursorFP = null;

    public static float HARD_MODE_BULLET_MULT = 1.3f;
    public static float HARD_MODE_ENTITY_HP_MULT = 1.25f;
    public static float HARD_MODE_ENTITY_SPEED_MULT = 0.8f;
    public static int HARD_MODE_COST_INCREASE = 2;

    // Makes you able to see levels generate, bad
    public static readonly bool CURSOR_FP_ACTIVE = false;

    public bool _withinCorpseCap { get { return CorpseCount < CORPSE_CAP; } }

    public static int ItemCost(int rangeMin, int rangeMax, float seed = 726547)
    {
        if (Instance.HardMode)
        {
            int costH = (A_Extensions.RandomBetween(rangeMin, rangeMax, QuickSeed * seed + seed) + HARD_MODE_COST_INCREASE) * Instance.DifficultyModifier + Instance.CurrentLevel * 5;
            costH += (Instance.DifficultyModifier - 1) * 105;
            return costH;
        }

        int cost = A_Extensions.RandomBetween(rangeMin, rangeMax, QuickSeed * seed + seed) * Instance.DifficultyModifier + Instance.CurrentLevel * 5;
        cost += (Instance.DifficultyModifier - 1) * 100;
        return cost;
    }

    private void Start()
    {
        Debug.Log("Todays seed is " + QuickSeed);

        if (_cursorFP != null && CURSOR_FP_ACTIVE)
            _cursorFP = Instantiate(_cursorFP, Vector2.zero, Quaternion.identity);
    }

    private void OnEnable()
    {
        A_EventManager.OnNextScene += NextScene;
        A_EventManager.OnLoadWorld += LoadWorld;
        A_EventManager.OnEntityDie += AddKill;
        A_EventManager.OnGoToMainMenu += ReturnToMainMenu;
        A_EventManager.OnTransitionEnd += TryAdBeforeLoad;
        A_EventManager.OnCollectGold += AddGold;
        A_EventManager.OnSpendGold += AddGoldSpent;
        A_EventManager.OnDealtDamage += AddDamageTaken;
    }

    private void OnDisable()
    {
        A_EventManager.OnLoadWorld -= LoadWorld;
        A_EventManager.OnNextScene -= NextScene;
        A_EventManager.OnEntityDie -= AddKill;
        A_EventManager.OnGoToMainMenu -= ReturnToMainMenu;
        A_EventManager.OnTransitionEnd -= TryAdBeforeLoad;
        A_EventManager.OnCollectGold -= AddGold;
        A_EventManager.OnSpendGold -= AddGoldSpent;
        A_EventManager.OnDealtDamage -= AddDamageTaken;
    }

    public int CurrentLevelDecimal() => (CurrentLevel + 1) / 2;

    public int WorldIndex()
    {
        if (CurrentLevel <= 5) return 1;
        if (CurrentLevel <= 11) return 2;
        if (CurrentLevel <= 17) return 3;
        if (CurrentLevel <= 24) return 4;
        return -1;
    }

    void Awake() => Instance = this;

    static bool DisableCursor = false;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1) && Input.GetKey(KeyCode.LeftControl))
            DisableCursor = !DisableCursor;
        Cursor.visible = !DisableCursor && !A_InputManager.GamepadMode;

        if (A_InputManager.ChangedModeThisFrame || (_retryKey == KeyCode.None && A_InputManager.Instance != null))
            _retryKey = A_InputManager.Instance.Key("Retry");

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.nearClipPlane;
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePos);

        if (_cursorGO != null)
        {            
            _cursorGO.transform.position = worldPosition;
        }
        if (_cursorFP != null && CURSOR_FP_ACTIVE)
        {
            _cursorFP.transform.position = Player.Instance.transform.position + (worldPosition - Player.Instance.transform.position).normalized * 3;
        }

        if (Player.Instance == null || (Player.Instance != null && Player.Instance.Stopped))
            return;

        _sleepTimer += Time.deltaTime;

        GlobalTime += Time.deltaTime;
        SceneTime += Time.unscaledDeltaTime;
        TimeSinceChestSpawn += Time.deltaTime;

        if (Input.GetKey(_retryKey))
            _retryTime += Time.deltaTime;
        else
            _retryTime = 0;

        if (_retryTime > 0.5f && _loadingLevel == 0 && GlobalTime >= 3)
        {
            A_EventManager.InvokeUnlock("Not my tempo");
            A_EventManager.InvokeDeleteSave();
            if (SeededRun)
                A_EventManager.InvokeMakeSave(Player.Instance.CharacterIndex, Seed.ToString(), HardMode);
            else
                A_EventManager.InvokeMakeSave(Player.Instance.CharacterIndex, "", HardMode);
            LoadWorld(1);
        }

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.O))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        if (Input.GetKeyDown(KeyCode.L))
        {
            Seed = Random.Range(0, 10000f);
            A_EventManager.InvokeSaveGame();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
#endif
    }

    void NextScene() => StartCoroutine(C_NextScene());

    IEnumerator C_NextScene()
    {
        yield return new WaitForSecondsRealtime(1);

        CurrentLevel++;

        if (Player.Instance.CurStats.Amount >= 10)
            A_EventManager.InvokeUnlock("Astral ready");

        if (TotalGoldSpent >= 1000)
            A_EventManager.InvokeUnlock("Big Spender");

        A_EventManager.InvokeSaveGame();
        LoadWorld(CurrentLevel);
    }

    void LoadWorld(int nextLevel)
    {
        A_EventManager.InvokeLowerMusic(0.33f, false);
        _loadingLevel = nextLevel;
        A_EventManager.InvokeTransition();
    }

    void TryAdBeforeLoad()
    {
        bool useAds;
#if USING_ADS
        useAds = true;
#else
        useAds = false;
#endif

        Debug.Log("CL: " + CurrentLevel);
        if (CurrentLevel == -1 || !useAds || (CurrentLevel - 1).IsEven())
        {
            LoadWorldAfterTrans();
            return;
        }

        Debug.Log("Playing Ad");
        CrazyGames.CrazyAds.Instance.beginAdBreak(LoadWorldAfterTrans, FailedAd);
    }

    void FailedAd()
    {
        Debug.Log("Ad Failed!");
        LoadWorldAfterTrans();
    }

    void LoadWorldAfterTrans()
    {
        int nextLevel = _loadingLevel;

        if (nextLevel == -1)
        {
            Debug.Log("Loading Main Menu");
            SceneManager.LoadScene("Main Menu");
            return;
        }

        if (nextLevel == 24)
        {
            Debug.Log("Loading LoopMachine");
            SceneManager.LoadScene("LoopMachine");
            return;
        }

        if (nextLevel == -2)
        {
            Debug.Log("Loading Intro");
            SceneManager.LoadScene("Intro");
            return;
        }

        if (nextLevel.IsEven())
        {
            Debug.Log("Loading Break: " + nextLevel / 2);
            SceneManager.LoadScene("Break");
            return;
        }

        nextLevel = (nextLevel + 1) / 2;

        Debug.Log("Loading World: " + nextLevel);
        SceneManager.LoadScene("World " + nextLevel);
    }

    void AddKill(Entity entity)
    {
        if (entity is EBoss || entity.EnemyName == "Tadpole" || entity.EnemyName == "Pot")
            return;

        Kills++;
    }

    void AddGold(int amount) => GoldGained += amount;

    void AddGoldSpent(int amount) => GoldSpent += amount;

    void AddDamageTaken(int amount, string _) => DamageTaken += amount;

    public void SetVariables(LevelData data)
    {
        CurrentLevel = data.CurLevel;
        Kills = data.Kills;
        GlobalTime = data.Time;
        DifficultyModifier = data.DifficultyModifer;
        TelestumpsUsed = data.TelestumpsUsed;
        MiniBossesKilled = data.MiniBossesKilled;
        PotsBroken = data.PotsBroken;
        GoldGained = data.GoldGained;
        GoldSpent = data.GoldSpent;
        DamageTaken = data.DamageTaken;
    }

    string[] LevelNames = new string[] { "Garden 1", "Garden 2", "Katydid", "Decay 1", "Decay 2", "Scolopendra", "Oasis 1", "Oasis 2", "Concheror", "Wetlands 1", "Wetlands 2", "Wallum Toad" };
    string[] AltLevelNames = new string[] { "Garden 1", "Garden 2", "Tella", "Decay 1", "Decay 2", "Leviathan", "Oasis 1", "Oasis 2", "Colossix", "Wetlands 1", "Wetlands 2", "Wallum Toad" };
    public string GetCurLevelName()
    {
        if (CurrentLevel.IsEven() && CurrentLevel != 24) 
            return "The Shop";
        if (CurrentLevel == 24)
            return RandomString();
        if (IsAltBoss)
            return AltLevelNames[CurrentLevelDecimal() - 1];
        return LevelNames[CurrentLevelDecimal() - 1];
    }

    string RandomString()
    {
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var stringChars = new char[6];
        var random = new System.Random();

        for (int i = 0; i < stringChars.Length; i++)
        {
            stringChars[i] = chars[random.Next(chars.Length)];
        }

        return new string(stringChars);
    }

    void ReturnToMainMenu()
    {
        _loadingLevel = -1;
        A_EventManager.InvokeTransition();
    }

    float _sleepDecreaser = 1;
    float _sleepTimer;
    bool _deactivateSleep = true;

    public float StoppedTimeTotal;
    public void Sleep(float ms)
    {
        if (Time.timeScale != 1 || Player.Instance.Stopped || V_HUDManager.Instance.IsPaused)
            return;

        if (_sleepTimer <= 1)
            _sleepDecreaser *= 0.75f;
        if (_sleepTimer > 1)
            _sleepDecreaser = 1;
        _sleepTimer = 0;

        if (_sleepDecreaser < 0.1f || _deactivateSleep)
            return;

        StartCoroutine(C_Sleep(ms * _sleepDecreaser));
    }

    IEnumerator C_Sleep(float ms)
    {
        if (_deactivateSleep)
            yield break;

        Debug.Log("Sleeping for: " + ms);

        V_HUDManager.Instance.AllowTimeScaleChanges = true;

        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(ms / 1000);
        Time.timeScale = 1;

        StoppedTimeTotal += ms;

        V_HUDManager.Instance.AllowTimeScaleChanges = false;
    }

    public GameObject GetCursor()
    {
        if (_cursorGO == null)
            _cursorGO = Instantiate(new GameObject(), Vector2.zero, Quaternion.identity);
        return _cursorGO;
    }

    public void OnDrawGizmos()
    {
        if (_cursorGO == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(_cursorGO.transform.position, 5f);
    }
}
