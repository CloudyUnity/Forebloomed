using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "Stat-Up", menuName = "Item/Stat-Up", order = 1)]
public class ItemData : ScriptableObject
{
    public string Name;
    [TextArea][SerializeField] string Description;
    [TextArea] public string Quip;
    public Color[] Colors;
    public Color ShotColors;
    public Sprite Icon;
    public bool IsFamiliar;
    public bool IsBlood;
    [Space(20)]
    public bool AddPlayerStatsEnabled;
    public PlayerStats AddPlayerStats;
    [Space(20)]
    public bool MultPlayerStatsEnabled;
    public PlayerStats MultiplyPlayerStats;
    [Space(20)]
    public bool AddSoftStatsEnabled;
    public PlayerSoftStats AddPlayerSoftStats;
    [Space(20)]
    public bool StatusEnabled;
    public List<StatusEffect> StatusEffects;
    [Space(20)]
    public bool DropsEnabled;
    public List<Drop> BulletDrops;

    public static readonly string BLUE = "7ca7d7";
    public static readonly string GREEN = "9dc384";
    public static readonly string RED = "d16d6a";
    public static readonly string ORANGE = "ecb576";
    public static readonly string YELLOW = "f9da78";
    public static readonly string PURPLE = "8b7dbe";
    public static readonly string PINK = "b87e9e";

    public string GetDescription()
    {
        string s = Description;
        s = s.Replace("#blue", "#" + BLUE);
        s = s.Replace("#green", "#" + GREEN);
        s = s.Replace("#red", "#" + RED);
        s = s.Replace("#orange", "#" + ORANGE);
        s = s.Replace("#yellow", "#" + YELLOW);
        s = s.Replace("#purple", "#" + PURPLE);
        s = s.Replace("#pink", "#" + PINK);

        for (int i = 0; i < s.Length; i++)
        {
            if (s[i] == '#' && (i == 0 || s[i - 1] != '='))
            {
                string hex = s.Substring(i, 7);
                int length = s.IndexOf(")") - s.IndexOf("(") - 1;
                string text = s.Substring(i + 8, length);
                string newTxt = "<color=" + hex + ">" + text + "</color>";
                s = s.Replace(hex + "(" + text + ")", newTxt);
            }
        }    
        return s;
    }

    public virtual void OnGoldSpawn(Vector2 pos) { }
    public virtual void OnHit() { }
    public virtual void OnEnemySpawn(Entity entity) { }
    public virtual void OnBulletEnemySpawn(BulletEnemy be) { }
    public virtual void OnTileSpawn(Tile tile) { }
    public virtual void OnPickup() { }
    public virtual void OnLoadItem() { }
    public virtual void AfterTime() { }
    public virtual void OnBulletSpawn(BulletPlayer bullet) { }
    public virtual void OnPlayerSliceSpawn(B_Slash s) { }
    public virtual void OnPlayerBoomerSpawn(Boomerang s) { }
    public virtual void OnPlayerBurSpawn(B_Knife s) { }
    public virtual void OnChestSpawn(O_Chest chest) { }
    public virtual void OnPricedItemSpawn(ItemPriced priced) { }
    public virtual void OnPricedHeartSpawn(O_PricedHeart pheart) { }
    public virtual void OnCollectItem(Item item) { }
    public virtual void OnPlayerBulletCollide(BulletPlayer bullet, GameObject go) { }
    public virtual void OnPlayerSliceCollide(B_Slash slice, GameObject go) { }
    public virtual void OnPlayerBoomerangCollide(Boomerang boom, GameObject go) { }
    public virtual void OnPlayerBulletMiss(BulletPlayer bullet) { }
    public virtual void OnHedgeBreak(TileWall wall) { }
    public virtual void OnChestOpen(O_Chest chest) { }
    public virtual void OnCollectGold(int _) { }
    public virtual void OnContact() { }
    public virtual void OnGetCollectable(O_Collectable o) { }
    public virtual void OnStumpSpawn(OW_Portal ow) { }
    public virtual void OnEntityDie(Entity e) { }
    public virtual void OnRecycleItem() { }
    public virtual void OnUseAbility() { }
    public virtual void OnPlayerBoomerangReturn(Boomerang b) { }
    public virtual void OnNewScene() { }
}
