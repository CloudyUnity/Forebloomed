using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

public class ItemHolder : MonoBehaviour
{
    public static ItemHolder Instance;

    public List<StoredItem> EveryItemByIndex = new List<StoredItem>();

    private void Awake() => Instance = this;

    public Dictionary<int, int> GeneratedItems = new Dictionary<int, int>();

    private void Start()
    {
        //DebugLogAllItems();
    }

    int GetGenItems(ItemPool pool)
    {
        if (GeneratedItems.ContainsKey((int)pool))
        {
            GeneratedItems[(int)pool] += A_LevelManager.Instance.CurrentLevel * 100;
            return GeneratedItems[(int)pool];
        }
        GeneratedItems.Add((int)pool, 1);
        return 1;
    }

    public ItemData GetRandomItem(float seed, out int index)
    {
        if (A_SaveManager.ChallengeName == "Healthy Brains")
        {
            index = 57;
            return EveryItemByIndex[57].Item;
        }

        if (seed == 0)
            seed = GetGenItems(0);

        index = A_Extensions.RandomBetween(0, EveryItemByIndex.Count, A_LevelManager.QuickSeed + seed);
        return EveryItemByIndex[index].Item;
    }

    public ItemData GetRandomItem(float seed, ItemPool pool, out int index)
    {
        if (A_SaveManager.ChallengeName == "Brain")
        {
            index = 57;
            return EveryItemByIndex[57].Item;
        }            

        if (seed == 0)
            seed = GetGenItems(pool);

        List<ItemData> items = FullItemPool(pool);
        ItemData item = items.RandomItem(A_LevelManager.QuickSeed * seed);

        bool chollaBanned = Player.Instance.CharacterIndex == 3 && item.Name.Is("Boomerang", "Trowel", "Dandelion", "Gnome Eyes", "Thistle", "Plantwave");
        bool swainBanned = Player.Instance.CharacterIndex == 6 && item.Name.Is("Trowel", "PlantWave");
        bool solBanned = Player.Instance.CharacterIndex == 7 && item.Name.Is("Trowel", "Grub", "Passion Flower", "Plantwave", "Boomerang", "Ecballium", "Acorn Leaf", "Watermelon"
            , "Roach Milk", "Gourd");
        if (chollaBanned || swainBanned || solBanned)
        {
            return GetRandomItem(seed + 4373, pool, out index);
        }

        index = IndexOf(item);
        return item;
    }

    public ItemData Get(int index) => EveryItemByIndex[index].Item;

    public List<ItemData> GetItems(List<int> list)
    {
        List<ItemData> items = new List<ItemData>();
        foreach (int i in list)
        {
            items.Add(EveryItemByIndex[i].Item);
        }
        return items;
    }

    public List<ItemData> FullItemPool(ItemPool pool)
    {
        List<ItemData> items = new List<ItemData>();
        foreach (StoredItem si in EveryItemByIndex)
        {
            if (si.Pool.HasFlag(pool))
            {
                items.Add(si.Item);
            }
        }
        return items;
    }

    public int IndexOf(ItemData data)
    {
        for (int i = 0; i < EveryItemByIndex.Count; i++)
        {
            if (EveryItemByIndex[i].Item == data)
                return i;
        }
        return -1;
    }

    void DebugLogAllItems()
    {
        string str = "";

        foreach (var item in EveryItemByIndex)
        {
            str += $"Name: {item.Item.Name} , ID: {EveryItemByIndex.IndexOf(item)} , Pools: {item.Pool} , Desc: {item.Item.GetDescription().Replace("\n", " ")} , Quip: {item.Item.Quip} ";

            if (item.Item.AddPlayerStatsEnabled)
                str += "\nPlayerStats Added:\n" + ListStruct(item.Item.AddPlayerStats);

            if (item.Item.MultPlayerStatsEnabled)
                str += "\nPlayerStats Multiplied:\n" + ListStruct(item.Item.MultiplyPlayerStats);

            if (item.Item.AddSoftStatsEnabled)
                str += "\nPlayerSoftStats Added:\n" + ListStruct(item.Item.AddPlayerSoftStats);

            if (item.Item.StatusEnabled)
            {
                str += "\nStatus Effects:\n";
                foreach (var status in item.Item.StatusEffects)
                    str += ListStruct(status);
            }

            if (item.Item.DropsEnabled)
            {
                str += "\nBulletDrops:\n";
                foreach (var drop in item.Item.BulletDrops)
                    str += ListStruct(drop);
            }

            str += "\n================================================================\n\n";
        }

        File.WriteAllText(Application.persistentDataPath + @"\items.txt", str);
    }

    string ListStruct(object obj)
    {
        string str = "";

        FieldInfo[] infos = obj.GetType().GetFields();
        foreach (FieldInfo info in infos)
        {
            if (info.FieldType.IsValueType && info.FieldType != typeof(int) && info.FieldType != typeof(float) && info.FieldType != typeof(bool))
            {
                str += info.Name + "\n";

                str += ListStruct(info.GetValue(obj));
                continue;
            }

            if (!IsValueEqualToZeroOrNull(info.GetValue(obj)))
                str += $"{info.Name} : {info.GetValue(obj)} \n";
        }
        return str;
    }

    bool IsValueEqualToZeroOrNull(object value)
    {
        if (value == null) return true; // Check for null
        System.Type valueType = value.GetType();

        // Check for common numeric types
        if (valueType == typeof(int) && (int)value == 0)
            return true;
        else if (valueType == typeof(float) && (float)value == 0.0f)
            return true;

        // Add more type-specific checks here if needed

        return false;
    }
}

[System.Flags]
public enum ItemPool
{
    Shop = 1,
    Chest = 2,
    Boss = 4,
    Blood = 8,
    BossBonus = 16,
    LockBox = 32,
};

[System.Serializable]
public struct StoredItem
{
    public ItemData Item;
    public ItemPool Pool;
}
