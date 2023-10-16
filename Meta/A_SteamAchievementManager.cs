using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class A_SteamAchievementManager : MonoBehaviour
{
    public static A_SteamAchievementManager Instance;

    private void Awake()
    {
        Instance = this;

        //LockAllAch();
        //UnlockAllAchFromMeta();
    }

    public void UnlockAch(string id)
    {
        if (!SteamManager.Initialized)
        {
            Debug.Log("Steam not initialized");
            return;
        }

        if (SteamUserStats.RequestCurrentStats())
            Debug.Log("Steam stats received");
        else
        {
            Debug.LogError("Steam stats not received");
            return;
        }            

        if (!SteamUserStats.GetAchievement(id, out bool pb))
        {
            Debug.LogWarning("Achievement " + id + " not found");
            return;
        }

        if (SteamUserStats.SetAchievement(id))
            Debug.Log("Unlocked achievement " + id);
        else
        {
            Debug.LogError("Steam achievement didn't trigger " + id);
            return;
        }            

        if (SteamUserStats.StoreStats())
            Debug.Log("Steam stats stored");
        else
            Debug.LogError("Stats not stored");
    }

    public void LogAchInfo()
    {
        if (!SteamManager.Initialized)
        {
            Debug.Log("Steam not initialized");
            return;
        }

        if (SteamUserStats.RequestCurrentStats())
            Debug.Log("Steam stats received");
        else
            Debug.LogError("Steam stats not received");

        uint count = SteamUserStats.GetNumAchievements();
        Debug.Log("Achievement count: " + count);
        for (uint i = 0; i < count; i++)
        {
            string name = SteamUserStats.GetAchievementName(i);
            Debug.Log(name);
        }
    }

    public void UnlockAllAch()
    {
        if (!SteamManager.Initialized)
        {
            Debug.Log("Steam not initialized");
            return;
        }

        if (SteamUserStats.RequestCurrentStats())
            Debug.Log("Steam stats received");
        else
            Debug.LogError("Steam stats not received");

        uint count = SteamUserStats.GetNumAchievements();
        Debug.Log("Achievement count: " + count);
        for (uint i = 0; i < count; i++)
        {
            string id = SteamUserStats.GetAchievementName(i);

            if (SteamUserStats.SetAchievement(id))
                Debug.Log("Unlocked achievement " + id);
            else
                Debug.LogError("Steam achievement didn't trigger " + id);
        }

        if (SteamUserStats.StoreStats())
            Debug.Log("Steam stats stored");
        else
            Debug.LogError("Stats not stored");
    }

    public void UnlockAllAchFromMeta()
    {
        if (!SteamManager.Initialized)
        {
            Debug.Log("Steam not initialized");
            return;
        }

        if (SteamUserStats.RequestCurrentStats())
            Debug.Log("Steam stats received");
        else
            Debug.LogError("Steam stats not received");

        int count = A_SaveMetaManager.instance._achievements.Count;
        Debug.Log("Achievement count: " + count);
        for (int i = 0; i < count; i++)
        {
            string id = A_SaveMetaManager.instance._achievements[i];

            if (SteamUserStats.SetAchievement(id))
                Debug.Log("Unlocked achievement " + id);
            else
                Debug.LogError("Steam achievement didn't trigger " + id);
        }

        if (SteamUserStats.StoreStats())
            Debug.Log("Steam stats stored");
        else
            Debug.LogError("Stats not stored");
    }

    public void LockAllAch()
    {
        if (!SteamManager.Initialized)
        {
            Debug.Log("Steam not initialized");
            return;
        }

        if (SteamUserStats.RequestCurrentStats())
            Debug.Log("Steam stats received");
        else
            Debug.LogError("Steam stats not received");

        uint count = SteamUserStats.GetNumAchievements();
        Debug.Log("Achievement count: " + count);
        for (uint i = 0; i < count; i++)
        {
            string id = SteamUserStats.GetAchievementName(i);

            SteamUserStats.ClearAchievement(id);
        }

        if (SteamUserStats.StoreStats())
            Debug.Log("Steam stats stored");
        else
            Debug.LogError("Stats not stored");
    }

    public bool AchUnlocked(string id)
    {
        if (!SteamManager.Initialized)
        {
            Debug.Log("Steam not initialized");
            return false;
        }

        if (SteamUserStats.RequestCurrentStats())
            Debug.Log("Steam stats received");
        else
            Debug.LogError("Steam stats not received");

        SteamUserStats.GetAchievementAndUnlockTime(id, out bool unlocked, out uint time);
        Debug.Log("Achievemt " + id + " unlocked: " + unlocked + " at " + time);
        return unlocked;
    }

    public void ClearAch(string id)
    {
        if (!SteamManager.Initialized)
        {
            Debug.Log("Steam not initialized");
            return;
        }

        SteamUserStats.ClearAchievement(id);
        Debug.Log("Cleared achievement " + id);

        SteamUserStats.StoreStats();
    }
}
