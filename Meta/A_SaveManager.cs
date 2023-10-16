using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

public class A_SaveManager : MonoBehaviour
{
    string filePath;
    [SerializeField] GameObject[] Characters;
    [SerializeField] GenMode _normalMode, _hardGenMode;
    [SerializeField] GameObject _chest;
    [SerializeField] Vector2 _customSpawnPos;

    public static string Version = "v4.1";
    public static string ChallengeName = "None";

    LevelData _dataCopy;

    [System.Serializable]
    struct StartingItems
    {
        public int Character;
        public List<int> Items;
    }
    [SerializeField] List<StartingItems> AllStartingItems = new List<StartingItems>();

    private void OnEnable()
    {
        A_EventManager.OnSaveGame += SaveLevelData;
        A_EventManager.OnReloadSave += ReloadScene;
        A_EventManager.OnDeleteSave += DeleteLevelData;
        A_EventManager.OnMakeEmptySave += MakeEmptySave;
        A_EventManager.OnStartChallenge += MakeChallengeSave;
        A_EventManager.OnBossDie += CompleteChallenge;
    }
    private void OnDisable()
    {
        A_EventManager.OnSaveGame -= SaveLevelData;
        A_EventManager.OnReloadSave -= ReloadScene;
        A_EventManager.OnDeleteSave -= DeleteLevelData;
        A_EventManager.OnMakeEmptySave -= MakeEmptySave;
        A_EventManager.OnStartChallenge -= MakeChallengeSave;
        A_EventManager.OnBossDie -= CompleteChallenge;
    }

    private void Start()
    {
        filePath = Application.persistentDataPath + "/level.json";

        if (A_LevelManager.Instance == null || A_LevelManager.Instance.CurrentLevel == -1)
            return;

        if (!File.Exists(filePath))
        {
            Debug.LogWarning("Failed to load data, creating empty thicket save");
            MakeEmptySave(0, "", false);
        }

        LoadLevelData();
    }

    LevelData FindLevelData => new LevelData()
    {
        Seed = A_LevelManager.Instance.Seed,
        SeededRun = _dataCopy.SeededRun,
        Items = ItemManager.Instance.AllItemIndexes(),
        SoftStats = Player.Instance.SoftStats,
        CurLevel = A_LevelManager.Instance.CurrentLevel,
        TimeGraph = A_TimeManager.Instance == null ? default(TimeData) : A_TimeManager.Instance.GetData(),
        Kills = A_LevelManager.Instance.Kills,
        GamePadMode = A_InputManager.GamepadMode,
        Time = A_LevelManager.Instance.GlobalTime,
        GoldGained = A_LevelManager.Instance.GoldGained,
        GoldSpent = A_LevelManager.Instance.GoldSpent,
        DamageTaken = A_LevelManager.Instance.DamageTaken,
        DifficultyModifer = A_LevelManager.Instance.DifficultyModifier,
        TelestumpsUsed = A_LevelManager.Instance.TelestumpsUsed,
        ExtraLives = A_LevelManager.Instance.ExtraLives,
        MiniBossesKilled = A_LevelManager.Instance.MiniBossesKilled,
        PotsBroken = A_LevelManager.Instance.PotsBroken,
        ShopkeeperTalks = A_LevelManager.Instance.ShopkeeperTalks,
        HardMode = A_LevelManager.Instance.HardMode,
        Character = _dataCopy.Character,
        Mode = _dataCopy.Mode,
        Chall = _dataCopy.Chall,
        ChallengeActive = _dataCopy.ChallengeActive,
        Version = Version,
        UsedDialogue = V_DialogueManager.Instance == null ? _dataCopy.UsedDialogue : V_DialogueManager.Instance.UsedDialogue,
    };

    void SaveLevelData()
    {
        var levelData = FindLevelData;
        if ((Player.Instance != null && Player.Instance.TookDamage) || (A_LevelManager.Instance != null && A_LevelManager.Instance.TookDamage))
            levelData.TookDamage = true;

        string json = JsonUtility.ToJson(levelData);
        File.WriteAllText(filePath, json);

        Debug.Log("Saved, CL: " + levelData.CurLevel);
    }

    void DeleteLevelData()
    {
        A_EventManager.InvokeSaveStats();

        Debug.Log("Data Deleted");

        File.Delete(filePath);
    }

    void LoadLevelData()
    {
        if (!File.Exists(filePath))
            return;

        string data = File.ReadAllText(filePath);
        LevelData levelData = JsonUtility.FromJson<LevelData>(data);

        if (levelData.Version != Version)
            Debug.Log("Incorrect Version, SOMETHING HAS GONE WRONG!");

        _dataCopy = levelData;

        A_LevelManager.Instance.Seed = levelData.Seed;
        A_LevelManager.Instance.SeededRun = levelData.SeededRun;
        A_LevelManager.Instance.HardMode = levelData.HardMode;
        A_LevelManager.QuickSeed = levelData.Seed * (levelData.CurLevel + 32) * (levelData.DifficultyModifer + 623);

        Debug.Log("Making Player " + levelData.Character);
        if (Player.Instance == null)
        {
            Instantiate(Characters[levelData.Character], _customSpawnPos, Quaternion.identity);
        }

        if (GenDataManager.Instance != null && !levelData.CurLevel.IsEven())
        {
            GenDataManager.Instance.Mode = levelData.Mode;
        }

        A_LevelManager.Instance.SetVariables(levelData);
        A_EventManager.InvokeTimeGraph(levelData.TimeGraph);

        if (!levelData.SoftStats.Equals(default(PlayerSoftStats)))
            Player.Instance.SoftStats = levelData.SoftStats;

        ItemManager.Instance.AddItems(levelData.Items);

        A_LevelManager.Instance.TookDamage = levelData.TookDamage;

        A_LevelManager.Instance.ExtraLives = levelData.ExtraLives;
        A_LevelManager.Instance.ShopkeeperTalks = levelData.ShopkeeperTalks;

        //A_InputManager.GamepadMode = levelData.GamePadMode;

        bool chall = levelData.ChallengeActive && levelData.Chall.UnlocksDisabled;
        bool seeded = levelData.SeededRun;
        if (chall || seeded)
            A_EventManager.InvokeDisableUnlocks();

        if (V_DialogueManager.Instance != null)
            V_DialogueManager.Instance.UsedDialogue = _dataCopy.UsedDialogue;

        ChallengeName = levelData.ChallengeActive ? levelData.Chall.Name : "None";

        if (levelData.Chall.Name == "Late Bloomer" && levelData.CurLevel.Is(1, 3))
        {
            for (int x = -2; x < 3; x++)
            {
                GameObject go = Instantiate(_chest, new Vector2(x, 1), Quaternion.identity);
                O_Chest chest = go.GetComponent<O_Chest>();
                chest.Cost = 3;
                chest.ForceSpawn = true;
            }
        }

        if (levelData.HardMode && levelData.CurLevel == 1)
        {
            A_Factory.Instance.MakeItem(0, new Vector2(2, 0), ItemPool.BossBonus);
        }

        Debug.Log("Data applied, CL: " + levelData.CurLevel);
    }

    void ReloadScene()
    {
        if (!File.Exists(filePath))
            return;

        string data = File.ReadAllText(filePath);
        LevelData levelData = JsonUtility.FromJson<LevelData>(data);

        Debug.Log("Continued, CL: " + levelData.CurLevel);

        A_EventManager.InvokeLoadWorld(levelData.CurLevel);
    }

    void MakeEmptySave(int charIndex, string seed, bool hardMode)
    {       
        if (_dataCopy.ChallengeActive)
        {
            MakeChallengeSave(_dataCopy.Chall);
            return;
        }

        bool noSeed = seed == "" || seed.Contains("Enter seed...");
        LevelData data = new LevelData()
        {
            Seed = noSeed ? Random.Range(0, 100000) : DecodeSeedString(seed),
            SeededRun = !noSeed,
            HardMode = hardMode,
            CurLevel = 1,
            DifficultyModifer = 1,
            Character = charIndex,
            Mode = hardMode ? _hardGenMode : _normalMode,
            Version = Version,
        };
        data.Seed = Mathf.RoundToInt(data.Seed);

        Debug.Log("Making seed: (" + seed + ") - (" + data.Seed + ")");

        foreach (var startItems in AllStartingItems)
        {
            if (startItems.Character == charIndex)
            {
                data.Items = new List<int>(startItems.Items);
                break;
            }
        }

        string json = JsonUtility.ToJson(data);
        File.WriteAllText(filePath, json);
    }

    int DecodeSeedString(string seed)
    {
        if (int.TryParse(seed, out int i))
            return i;

        return Mathf.Abs(seed.GetHashCode()) % 100000;
    }

    void MakeChallengeSave(Challenge challenge)
    {
        LevelData data = new LevelData()
        {
            Seed = Random.Range(0, 100000),
            CurLevel = challenge.StartLevel,
            DifficultyModifer = challenge.Difficulty,
            Character = challenge.Character,
            Mode = challenge.Mode,
            Chall = challenge,
            ChallengeActive = true,
            Version = Version,
        };
        data.Seed = Mathf.RoundToInt(data.Seed);

        data.Items = new List<int>(challenge.Items);

        string json = JsonUtility.ToJson(data);
        File.WriteAllText(filePath, json);
    }

    void CompleteChallenge(EBoss _)
    {
        if (Player.Instance.Dead)
            return;

        if (!_dataCopy.ChallengeActive || _dataCopy.Chall.EndLevel != _dataCopy.CurLevel)
            return;

        StartCoroutine(C_Challenge());
    }

    IEnumerator C_Challenge()
    {
        A_EventManager.InvokeForceUnlock(_dataCopy.Chall.Name, false);
        Player.Instance.InCutscene = true;
        yield return new WaitForSeconds(3);
        A_EventManager.InvokeDeleteSave();
        A_EventManager.InvokeGoToMainMenu();
    }
}

public struct LevelData
{
    public float Seed;
    public bool SeededRun;
    public bool HardMode;

    public List<int> Items;
    public PlayerSoftStats SoftStats;
    public int CurLevel;

    public GenMode Mode;
    public bool GamePadMode;

    public bool ChallengeActive;
    public Challenge Chall;

    public TimeData TimeGraph;
    public int Kills;
    public float Time;
    public int GoldGained;
    public int GoldSpent;
    public int DamageTaken;
    public int DifficultyModifer;
    public int Character;
    public int ExtraLives;

    public bool TookDamage;
    public int TelestumpsUsed;
    public int MiniBossesKilled;
    public int PotsBroken;
    public int ShopkeeperTalks;

    public string Version;
    public List<string> UsedDialogue;
}

[System.Serializable]
public struct Challenge
{
    public GenMode Mode;
    public int Character;
    public List<int> Items;
    public bool UnlocksDisabled;
    public int Difficulty;
    public int StartLevel;
    public int EndLevel;
    public string Name;
}