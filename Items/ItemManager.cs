using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance;

    public List<ItemData> AllItems = new List<ItemData>();
    public List<Color> AllColors = new List<Color>();
    public List<Drop> AllDrops = new List<Drop>();

    void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        A_EventManager.OnGoldSpawn += OnGoldSpawn;
        A_EventManager.OnDealtDamage += OnHit;
        A_EventManager.OnEntitySpawn += OnEnemySpawn;
        A_EventManager.OnBulletEnemySpawn += OnBulletEnemy;
        A_EventManager.OnTileSpawn += OnTileSpawn;
        A_EventManager.OnChestSpawn += OnChestSpawn;
        A_EventManager.OnItemPricedSpawn += OnPricedItemSpawn;
        A_EventManager.OnCollectItem += OnCollectItem;
        A_EventManager.OnPlayerBulletCollide += OnPlayerBulletCollide;
        A_EventManager.OnPlayerSliceCollide += OnPlayerSliceCollide;
        A_EventManager.OnPlayerBulletMiss += OnPlayerBulletMiss;
        A_EventManager.OnPricedHeartSpawn += OnPricedHeartSpawn;
        A_EventManager.OnHedgeBreak += OnHedgeBreak;
        A_EventManager.OnChestOpen += OnChestOpen;
        A_EventManager.OnCollectGold += OnCollectGold;
        A_EventManager.OnPlayerBoomerangCollide += OnPlayerBoomerangCollide;
        A_EventManager.OnContact += OnContact;
        A_EventManager.OnPlayerBulletSpawn += OnPlayerBulletSpawn;
        A_EventManager.OnGetCollectable += OnGetCollectable;
        A_EventManager.OnStumpSpawn += OnStumpSpawn;
        A_EventManager.OnEntityDie += OnEntityDie;
        A_EventManager.OnUseAbility += OnUseAbility;
        A_EventManager.OnPlayerBoomerangReturn += OnPlayerBoomerangReturn;
        A_EventManager.OnPlayerBurSpawn += OnPlayerBurSpawn;
        A_EventManager.OnPlayerSlashSpawn += OnPlayerSlashSpawn;
        A_EventManager.OnPlayerBoomerSpawn += OnPlayerBoomerSpawn;
        A_EventManager.OnNextScene += OnNewScene;
    }
    private void OnDisable()
    {
        A_EventManager.OnGoldSpawn -= OnGoldSpawn;
        A_EventManager.OnDealtDamage -= OnHit;
        A_EventManager.OnEntitySpawn -= OnEnemySpawn;
        A_EventManager.OnBulletEnemySpawn -= OnBulletEnemy; 
        A_EventManager.OnTileSpawn -= OnTileSpawn;
        A_EventManager.OnChestSpawn -= OnChestSpawn;
        A_EventManager.OnItemPricedSpawn -= OnPricedItemSpawn;
        A_EventManager.OnCollectItem -= OnCollectItem;
        A_EventManager.OnPlayerBulletCollide -= OnPlayerBulletCollide;
        A_EventManager.OnPlayerSliceCollide -= OnPlayerSliceCollide;
        A_EventManager.OnPlayerBulletMiss -= OnPlayerBulletMiss;
        A_EventManager.OnPricedHeartSpawn -= OnPricedHeartSpawn;
        A_EventManager.OnHedgeBreak -= OnHedgeBreak;
        A_EventManager.OnChestOpen -= OnChestOpen;
        A_EventManager.OnCollectGold -= OnCollectGold;
        A_EventManager.OnPlayerBoomerangCollide -= OnPlayerBoomerangCollide;
        A_EventManager.OnContact -= OnContact;
        A_EventManager.OnPlayerBulletSpawn -= OnPlayerBulletSpawn;
        A_EventManager.OnGetCollectable -= OnGetCollectable;
        A_EventManager.OnStumpSpawn -= OnStumpSpawn;
        A_EventManager.OnEntityDie -= OnEntityDie;
        A_EventManager.OnUseAbility -= OnUseAbility;
        A_EventManager.OnPlayerBoomerangReturn -= OnPlayerBoomerangReturn;
        A_EventManager.OnPlayerBurSpawn -= OnPlayerBurSpawn;
        A_EventManager.OnPlayerSlashSpawn -= OnPlayerSlashSpawn;
        A_EventManager.OnPlayerBoomerSpawn -= OnPlayerBoomerSpawn;
        A_EventManager.OnNextScene -= OnNewScene;
    }

    void OnGoldSpawn(Vector3 pos, int _) => CallAll((item) => item.OnGoldSpawn(pos));
    void OnHit(int _, string __) => CallAll((item) => item.OnHit());
    void OnEnemySpawn(Entity entity) => CallAll((item) => item.OnEnemySpawn(entity));
    void OnBulletEnemy(BulletEnemy be) => CallAll((item) => item.OnBulletEnemySpawn(be));
    void OnTileSpawn(Tile tile) => CallAll((item) => item.OnTileSpawn(tile));
    void OnChestSpawn(O_Chest chest) => CallAll((item) => item.OnChestSpawn(chest));
    void OnChestOpen(O_Chest chest) => CallAll((item) => item.OnChestOpen(chest));
    void OnPricedItemSpawn(ItemPriced priced) => CallAll((item) => item.OnPricedItemSpawn(priced));
    void OnCollectItem(Item collected) => CallAll((item) => item.OnCollectItem(collected));
    void OnPlayerBulletCollide(BulletPlayer bullet, GameObject go) => CallAll((item) => item.OnPlayerBulletCollide(bullet, go));
    void OnPlayerBulletSpawn(BulletPlayer bullet) => CallAll((item) => item.OnBulletSpawn(bullet));
    void OnPlayerSlashSpawn(B_Slash bullet) => CallAll((item) => item.OnPlayerSliceSpawn(bullet));
    void OnPlayerBoomerSpawn(Boomerang bullet) => CallAll((item) => item.OnPlayerBoomerSpawn(bullet));
    void OnPlayerBurSpawn(B_Knife bullet) => CallAll((item) => item.OnPlayerBurSpawn(bullet));
    void OnPlayerSliceCollide(B_Slash slice, GameObject go) => CallAll((item) => item.OnPlayerSliceCollide(slice, go));
    void OnPlayerBoomerangCollide(Boomerang boom, GameObject go) => CallAll((item) => item.OnPlayerBoomerangCollide(boom, go));
    void OnPlayerBulletMiss(BulletPlayer bullet) => CallAll((item) => item.OnPlayerBulletMiss(bullet));
    void OnPricedHeartSpawn(O_PricedHeart pheart) => CallAll((item) => item.OnPricedHeartSpawn(pheart));
    void OnHedgeBreak(TileWall wall) => CallAll((item) => item.OnHedgeBreak(wall));
    void OnCollectGold(int amount) => CallAll((item) => item.OnCollectGold(amount));
    void OnContact() => CallAll((item) => item.OnContact());
    void OnGetCollectable(O_Collectable o) => CallAll((item) => item.OnGetCollectable(o));
    void OnStumpSpawn(OW_Portal ow) => CallAll((item) => item.OnStumpSpawn(ow));
    void OnEntityDie(Entity entity) => CallAll((item) => item.OnEntityDie(entity));
    void OnRecycleItem() => CallAll((item) => item.OnRecycleItem());
    void OnUseAbility() => CallAll((item) => item.OnUseAbility());
    void OnPlayerBoomerangReturn(Boomerang b) => CallAll((item) => item.OnPlayerBoomerangReturn(b));
    void OnNewScene() => CallAll((item) => item.OnNewScene());

    void Update()
    {
        CallAll((item) => item.AfterTime());
        AssignColors();

        AllDrops = new List<Drop>();
        foreach (ItemData item in AllItems)
        {
            if (item == null || !item.DropsEnabled)
                continue;

            AllDrops.AddRange(item.BulletDrops);
        }
    }

    static bool _stemTrig, _cultTrig;
    public PlayerStats ItemPlayerStats()
    {
        int famCount = 0;
        int bloodCount = 0;
        PlayerStats newStats = Player.Instance.DefaultStats;
        foreach (ItemData item in AllItems)
        {
            if (item.IsFamiliar)
                famCount++;

            if (item == null || !item.AddPlayerStatsEnabled)
                continue;

            newStats += item.AddPlayerStats;
        }

        if (famCount >= 4 && !_stemTrig)
        {
            A_EventManager.InvokeUnlock("Stem");
            _stemTrig = true;
        }            

        if (bloodCount >= 3 && !_cultTrig)
        {
            A_EventManager.InvokeUnlock("Cult");
            _cultTrig = true;
        }            

        PlayerStats multipliers = PlayerStats.one;
        foreach (ItemData item in AllItems)
        {
            if (item == null || !item.MultPlayerStatsEnabled)
                continue;

            multipliers *= item.MultiplyPlayerStats;
        }
        newStats *= multipliers;
        return newStats;
    }

    void CallAll(Action<ItemData> Trigger)
    {
        foreach (ItemData item in AllItems)
        {
            if (item == null)
                continue;

            Trigger(item);
        }
    }

    public void AddItems(List<int> indexes)
    {
        foreach (int index in indexes)
        {
            ItemData data = ItemHolder.Instance.Get(index);
            AllItems.Add(data);
            data.OnLoadItem();
        }
    }

    public List<int> AllItemIndexes()
    {
        List<int> indexes = new List<int>();
        foreach(ItemData item in AllItems)
        {
            if (item == null)
                continue;

            indexes.Add(ItemHolder.Instance.IndexOf(item));
        }
        return indexes;
    }

    static bool _mutTrig;
    public List<StatusEffect> AllStatusEffects()
    {
        List<StatusEffect> statusEffects = new List<StatusEffect>();
        foreach (ItemData item in AllItems)
        {
            if (item == null || !item.StatusEnabled)
                continue;

            foreach (StatusEffect itemStatus in item.StatusEffects)
            {
                bool used = false;
                for (int i = 0; i < statusEffects.Count; i++)
                {
                    if (statusEffects[i].Name == itemStatus.Name)
                    {
                        statusEffects[i] += itemStatus;
                        used = true;
                    }
                }
                if (!used && itemStatus.Dur > 0)
                    statusEffects.Add(itemStatus);
            }
        
        }

        if (!_mutTrig && Player.Instance.CharacterIndex == 2)
        {
            bool p = false, f = false, i = false, w = false, c = false;
            foreach (var st in statusEffects)
            {
                if (st.Name == "Poison")
                    p = true;
                if (st.Name == "Ice")
                    i = true;
                if (st.Name == "Fire")
                    f = true;
                if (st.Name == "Brittle")
                    w = true;
                if (st.Name == "Confusion")
                    c = true;
            }
            if (p && f && i && w && c)
            {
                A_EventManager.InvokeUnlock("Mutant");
                _mutTrig = true;
            }                
        }

        return statusEffects;
    }

    void AssignColors()
    {
        AllColors.Clear();
        foreach (ItemData item in AllItems)
        {
            if (item.ShotColors != null && item.ShotColors.a > 0.5f)
            {
                if (item.name == "Firewood" && Player.Instance.CharacterIndex == 5)
                    continue;

                if (item.name == "Gold Leaf" && Player.Instance.CharacterIndex == 7)
                    continue;

                AllColors.Add(item.ShotColors);
            }
        }
    }

    public MakeInfo? RandomDrop()
    {
        foreach (Drop drop in AllDrops)
        {
            float freq = drop.Frequency / Player.Instance.CurStats.Amount;
            if (UnityEngine.Random.Range(0, 100f) <= freq - Player.Instance.CurStats.Luck)
            {
                if (A_TimeManager.Instance != null && drop.DisabledAfterDark && A_TimeManager.Instance.TimePercent > 1)
                    return null;

                return drop.info;
            }
        }
        return null;
    }
}
