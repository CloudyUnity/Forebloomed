using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class A_SaveMetaManager : MonoBehaviour
{
    public static A_SaveMetaManager instance;

    string filePath;
    [SerializeField] public List<string> _achievements;
    public List<int> Hats = new List<int>();
    public static bool BeatenGame = false;
    public static bool SolanaUnlocked = false;

    static List<string> _unlockedAch = new List<string>();

    bool _unlocksDisabled;

    public List<int> EmblemsPlaced;

    private void OnEnable()
    {
        A_EventManager.OnUnlock += Unlock;
        A_EventManager.OnDeleteMeta += Delete;
        A_EventManager.OnMakeEmptySave += AddRun;
        A_EventManager.OnBossDie += EndRun;
        A_EventManager.OnDisableUnlocks += DisableUnlocks;
        A_EventManager.OnForceUnlock += RealUnlock;
        A_EventManager.OnSaveStats += UpdateData;
    }

    private void OnDisable()
    {
        A_EventManager.OnUnlock -= Unlock;
        A_EventManager.OnDeleteMeta -= Delete;
        A_EventManager.OnMakeEmptySave -= AddRun;
        A_EventManager.OnBossDie -= EndRun;
        A_EventManager.OnDisableUnlocks -= DisableUnlocks;
        A_EventManager.OnForceUnlock -= RealUnlock;
        A_EventManager.OnSaveStats -= UpdateData;
    }

    private void Awake()
    {
        instance = this;

        if (_unlockedAch == null)
            _unlockedAch = new List<string>();
    }

    void DisableUnlocks() => _unlocksDisabled = true;

    private void Start()
    {
        //string str = "";
        //foreach (var a in _achievements)
        //{
        //    str += a + "\n";
        //}
        //File.WriteAllText(Application.persistentDataPath + @"\ach.txt", str);

        filePath = Application.persistentDataPath + "/meta.json";

        if (!File.Exists(filePath))
        {
            CreateNewData();
            A_EventManager.InvokeLoadWorld(-2);
            return;
        } 

        Load();
    }

    void CreateNewData()
    {
        MetaData defaultData = new MetaData()
        {
            Achievements = new List<int>(),
            ActiveHats = new List<int>(),
            AllStats = new List<CharacterStats>(),
            SolanaStatueEmblemsPlaced = new List<int>(),
        };

        string json = JsonUtility.ToJson(defaultData);
        File.WriteAllText(filePath, json);

        Apply(defaultData);
    }

    void Save(MetaData data)
    {
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(filePath, json);
        Debug.Log("Saved Meta");
    }

    void Delete()
    {
        Debug.Log("Meta Deleted");

        File.Delete(filePath);
        CreateNewData();
    }

    public void Load()
    {
        if (!File.Exists(filePath))
            return;

        string data = File.ReadAllText(filePath);
        MetaData metaData = JsonUtility.FromJson<MetaData>(data);

        Apply(metaData);

        Debug.Log("Loaded Meta");
    }

    void Apply(MetaData metaData)
    {
        float percent = (float)metaData.Achievements.Count / (_achievements.Count - 1);
        percent = Mathf.Floor(percent * 100);
        A_EventManager.InvokeDisplayPerc((int)percent);

        if (!metaData.Achievements.Contains(55) && metaData.Achievements.Count >= _achievements.Count - 1)
        {
            RealUnlock("Final Growth", false);
            metaData.Achievements.Add(55);
        }

        if (metaData.Achievements.Count >= 50)
            RealUnlock("King", false);

        foreach (int i in metaData.Achievements)
        {
            if (_achievements[i] == null)
                continue;

            A_EventManager.InvokeOpenMeta(_achievements[i]);
        }

        EmblemsPlaced = metaData.SolanaStatueEmblemsPlaced;

        BeatenGame = metaData.Achievements.Contains(45);
        SolanaUnlocked = metaData.Achievements.Contains(106);

        Hats = metaData.ActiveHats;

        A_EventManager.InvokeInitHatButtons(Hats);
    }

    void Unlock(string name)
    {
        if (_unlocksDisabled)
            return;

        RealUnlock(name, true);
    }    

    void RealUnlock(string name, bool show)
    {
        if (_unlockedAch.Contains(name))
            return;
        _unlockedAch.Add(name);

        if (A_SteamAchievementManager.Instance != null)
            A_SteamAchievementManager.Instance.UnlockAch(name);
        else
            Debug.LogWarning("Achievement missed by steam: " + name);

        filePath = Application.persistentDataPath + "/meta.json";

        if (!File.Exists(filePath))
            return;

        string data = File.ReadAllText(filePath);
        MetaData metaData = JsonUtility.FromJson<MetaData>(data);

        for (int i = 0; i < _achievements.Count; i++)
        {
            if (_achievements[i] != name)
                continue;

            if (metaData.Achievements.Contains(i))
                return;

            Debug.Log("Unlocked Achievement: " + name);
            A_EventManager.InvokePlaySFX("Achievement");

            metaData.Achievements.Add(i);
            Save(metaData);
            if (show)
                A_EventManager.InvokeShowUnlock(name);
            return;
        }

        throw new System.Exception("Unlockable name not found " + name);
    }

    public void ToggleHat(int index, bool enable)
    {
        filePath = Application.persistentDataPath + "/meta.json";

        if (!File.Exists(filePath))
            return;

        string data = File.ReadAllText(filePath);
        MetaData metaData = JsonUtility.FromJson<MetaData>(data);

        if (Hats.Contains(index) && !enable)
        {
            Hats.Remove(index);
            metaData.ActiveHats = Hats;
            Save(metaData);
            return;
        }

        else if (!Hats.Contains(index) && enable)
        {
            Hats.Add(index);
            metaData.ActiveHats = Hats;
            Save(metaData);
            return;
        }

        throw new System.Exception("Hat problems");
    }

    void AddRun(int character, string _, bool __)
    {
        if (!File.Exists(filePath))
            return;

        string data = File.ReadAllText(filePath);
        MetaData current = JsonUtility.FromJson<MetaData>(data);

        int indexOfChar = GetCharStatsIndex(current, character);

        CharacterStats stat = current.AllStats[indexOfChar];
        stat.Runs++;
        current.AllStats[indexOfChar] = stat;

        Save(current);
        Debug.Log("Begun Stat Tracker");
    }

    void EndRun(EBoss boss)
    {
        if (boss.EnemyName != "Wallum Toad" || A_LevelManager.Instance.DifficultyModifier != 1)
            return;

        if (!File.Exists(filePath))
            return;

        string data = File.ReadAllText(filePath);
        MetaData current = JsonUtility.FromJson<MetaData>(data);

        string filePath2 = Application.persistentDataPath + "/level.json";
        string data2 = File.ReadAllText(filePath2);
        LevelData levelData = JsonUtility.FromJson<LevelData>(data2);
        if (levelData.ChallengeActive)
            return;

        int indexOfChar = GetCharStatsIndex(current, Player.Instance.CharacterIndex);

        CharacterStats stat = current.AllStats[indexOfChar];

        if (A_LevelManager.Instance.GlobalTime < stat.FastestTime)
            stat.FastestTime = A_LevelManager.Instance.GlobalTime;

        stat.Wins++;
        current.WinStreak++;

        current.AllStats[indexOfChar] = stat;

        Save(current);
        Debug.Log("Added final stats, new records, +1 win");
    }

    bool _updated = false;

    void UpdateData()
    {
        if (_updated)
            return;

        if (Player.Instance != null)
            A_EventManager.InvokeSaveGame();

        string filePath2 = Application.persistentDataPath + "/level.json";

        if (!File.Exists(filePath2))
            return;

        string data = File.ReadAllText(filePath);
        MetaData current = JsonUtility.FromJson<MetaData>(data);

        string data2 = File.ReadAllText(filePath2);
        LevelData levelData = JsonUtility.FromJson<LevelData>(data2);

        if (levelData.ChallengeActive)
            return;

        _updated = true;

        int indexOfChar = GetCharStatsIndex(current, levelData.Character);
        CharacterStats stat = current.AllStats[indexOfChar];
        stat.Kills += levelData.Kills;
        stat.Time += levelData.Time;
        stat.GoldEarned += levelData.GoldGained;
        stat.GoldSpent += levelData.GoldSpent;
        stat.DamageTaken += levelData.DamageTaken;

        bool higherWorld = levelData.DifficultyModifer > stat.FurthestLevel.x;
        bool higherLevel = levelData.DifficultyModifer == stat.FurthestLevel.x && levelData.CurLevel > stat.FurthestLevel.y;

        if ((higherWorld || higherLevel) && !levelData.CurLevel.IsEven())
            stat.FurthestLevel = new Vector2(levelData.DifficultyModifer, levelData.CurLevel);

        current.AllStats[indexOfChar] = stat;

        if (Player.Instance != null && Player.Instance.Dead)
            current.WinStreak = 0;

        Save(current);
        Debug.Log("Updated Stats");
    }

    int GetCharStatsIndex(MetaData current, int character)
    {
        for (int i = 0; i < current.AllStats.Count; i++)
        {
            if (current.AllStats[i].Character == character)
            {
                return i;
            }
        }

        current.AllStats.Add(new CharacterStats(character));
        return current.AllStats.Count - 1;
    }

    public void SaveEmblemPlaced(List<int> emblemIDs)
    {
        foreach (int id in emblemIDs)
            EmblemsPlaced.Add(id);

        filePath = Application.persistentDataPath + "/meta.json";

        if (!File.Exists(filePath))
            return;

        string data = File.ReadAllText(filePath);
        MetaData metaData = JsonUtility.FromJson<MetaData>(data);
        metaData.SolanaStatueEmblemsPlaced = EmblemsPlaced;

        Save(metaData);
    }
}

public struct MetaData
{
    public List<int> Achievements;
  
    public List<CharacterStats> AllStats;

    public List<int> ActiveHats;

    public List<int> SolanaStatueEmblemsPlaced;

    public int WinStreak;
}

[System.Serializable]
public struct CharacterStats
{
    public int Character;
    public int Kills;
    public float Time;
    public int Runs;
    public int Wins;
    public int GoldEarned;
    public int GoldSpent;
    public int DamageTaken;

    public Vector2 FurthestLevel;
    public float FastestTime;

    public CharacterStats(int character)
    {
        Character = character;
        Kills = 0;
        Time = 0;
        Runs = 0;
        Wins = 0;
        GoldEarned = 0;
        GoldSpent = 0;
        DamageTaken = 0;
        FurthestLevel = Vector2.zero;
        FastestTime = 9999;
    }

    public static CharacterStats operator +(CharacterStats a, CharacterStats b)
    {
        return new CharacterStats()
        {
            Character = a.Character,
            Kills = a.Kills + b.Kills,
            Time = a.Time + b.Time,
            Runs = a.Runs + b.Runs,
            Wins = a.Wins + b.Wins,
            GoldEarned = a.GoldEarned + b.GoldEarned,
            GoldSpent = a.GoldSpent + b.GoldSpent,
            DamageTaken = a.DamageTaken + b.DamageTaken,
            FurthestLevel = a.FurthestLevel,
            FastestTime = a.FastestTime,
        };
    }
}
