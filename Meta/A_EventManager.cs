using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class A_EventManager
{
    public static event Action<Vector2, float> OnNewGrid;
    public static void InvokeMakeGrid(Vector2 pos, float size) => OnNewGrid?.Invoke(pos, size);

    public static event Action OnNextScene;
    public static void InvokeNextScene() => OnNextScene?.Invoke();

    public static event Action OnReloadSave;
    public static void InvokeReloadSave() => OnReloadSave?.Invoke();

    public static event Action<int> OnLoadWorld;
    public static void InvokeLoadWorld(int num) => OnLoadWorld?.Invoke(num);

    public static event Action<int, string, bool> OnMakeEmptySave;
    public static void InvokeMakeSave(int num, string seed, bool hardMode) => OnMakeEmptySave?.Invoke(num, seed, hardMode);

    public static event Action OnSaveGame;
    public static void InvokeSaveGame() => OnSaveGame?.Invoke();

    public static event Action OnDeleteSave;
    public static void InvokeDeleteSave() => OnDeleteSave?.Invoke();

    public static event Action OnDeleteMeta;
    public static void InvokeDeleteMeta() => OnDeleteMeta?.Invoke();

    public static event Action OnMakeFish;
    public static void InvokeMakeFish() => OnMakeFish?.Invoke();

    public static event Action<Vector2, int, int> OnRestock;
    public static void InvokeRestock(Vector2 pos, int cost, int count) => OnRestock?.Invoke(pos, cost, count);

    public static event Action<float, float> OnCameraShake;
    public static void InvokeCameraShake(float mag, float dur) => OnCameraShake?.Invoke(mag, dur);

    public static event Action<int> OnCollectGold;    
    public static void InvokeCollectGold(int amount)
    {
        OnCollectGold?.Invoke(amount);
        InvokeGoldChange();
    }

    public static event Action OnGoldChange;
    public static void InvokeGoldChange() => OnGoldChange?.Invoke();

    public static event Action<int> OnSpendGold;
    public static void InvokeSpendGold(int x) => OnSpendGold?.Invoke(x);

    public static event Action<Item> OnCollectItem;
    public static void InvokeCollectItem(Item item) => OnCollectItem?.Invoke(item);

    public static event Action<int, Vector2, string> OnDealDamage;
    public static void InvokeDealDamage(int dmg, Vector2 from, string name) => OnDealDamage?.Invoke(dmg, from, name);

    public static event Action<int, string> OnDealtDamage;
    public static void InvokeDealtDamage(int dmg, string name) => OnDealtDamage?.Invoke(dmg, name);

    public static event Action OnPlayerDied;
    public static void InvokePlayerDied() => OnPlayerDied?.Invoke();

    public static event Action OnGoToMainMenu;
    public static void InvokeGoToMainMenu() => OnGoToMainMenu?.Invoke();

    public static event Action<Vector3, int> OnGoldSpawn;
    public static void InvokeGoldSpawn(Vector3 pos, int amount) => OnGoldSpawn?.Invoke(pos, amount);

    public static event Action<Entity> OnEntitySpawn;
    public static void InvokeEntitySpawn(Entity entity) => OnEntitySpawn?.Invoke(entity);

    public static event Action<Entity> OnEntityDie;
    public static void InvokeEntityDie(Entity entity) => OnEntityDie?.Invoke(entity);

    public static event Action<BulletEnemy> OnBulletEnemySpawn;
    public static void InvokeBulletEnemySpawn(BulletEnemy be) => OnBulletEnemySpawn?.Invoke(be);

    public static event Action<Tile> OnTileSpawn;
    public static void InvokeTileSpawn(Tile tile) => OnTileSpawn?.Invoke(tile);

    public static event Action<ItemPriced> OnItemPricedSpawn;
    public static void InvokePricedItemSpawn(ItemPriced priced) => OnItemPricedSpawn?.Invoke(priced);

    public static event Action<O_Chest> OnChestSpawn;
    public static void InvokeChestSpawn(O_Chest chest) => OnChestSpawn?.Invoke(chest);

    public static event Action<O_Chest> OnChestOpen;
    public static void InvokeChestOpen(O_Chest chest) => OnChestOpen?.Invoke(chest);

    public static event Action<string> OnPlaySFX;
    public static void InvokePlaySFX(string name) => OnPlaySFX?.Invoke(name);

    public static event Action<string> OnUnlock;
    public static void InvokeUnlock(string name) => OnUnlock?.Invoke(name);

    public static event Action<string, bool> OnForceUnlock;
    public static void InvokeForceUnlock(string name, bool show) => OnForceUnlock?.Invoke(name, show);

    public static event Action<string> OnShowUnlock;
    public static void InvokeShowUnlock(string name) => OnShowUnlock?.Invoke(name);

    public static event Action<string> OnOpenMeta;
    public static void InvokeOpenMeta(string name) => OnOpenMeta?.Invoke(name);

    public static event Action<TimeData> OnDisplayTimeGraph;
    public static void InvokeTimeGraph(TimeData data) => OnDisplayTimeGraph?.Invoke(data);

    public static event Action OnTransition;
    public static void InvokeTransition() => OnTransition?.Invoke();

    public static event Action OnTransitionEnd;
    public static void InvokeTransitionEnd() => OnTransitionEnd?.Invoke();

    public static event Action<BulletPlayer, GameObject> OnPlayerBulletCollide;
    public static void InvokePlayerBulletCollide(BulletPlayer bullet, GameObject go) => OnPlayerBulletCollide?.Invoke(bullet, go);

    public static event Action<B_Knife, GameObject> OnPlayerBurLodge;
    public static void InvokePlayerBurLodge(B_Knife bullet, GameObject go) => OnPlayerBurLodge?.Invoke(bullet, go);

    public static event Action<B_Slash, GameObject> OnPlayerSliceCollide;
    public static void InvokePlayerSliceCollide(B_Slash slice, GameObject go) => OnPlayerSliceCollide?.Invoke(slice, go);

    public static event Action<Boomerang, GameObject> OnPlayerBoomerangCollide;
    public static void InvokePlayerBoomerangCollide(Boomerang boom, GameObject go) => OnPlayerBoomerangCollide?.Invoke(boom, go);

    public static event Action<Boomerang> OnPlayerBoomerangReturn;
    public static void InvokePlayerBoomerangReturn(Boomerang boom) => OnPlayerBoomerangReturn?.Invoke(boom);

    public static event Action<BulletPlayer> OnPlayerBulletMiss;
    public static void InvokePlayerBulletMiss(BulletPlayer bullet) => OnPlayerBulletMiss?.Invoke(bullet);

    public static event Action<B_Knife> OnPlayerBurMiss;
    public static void InvokePlayerBurMiss(B_Knife bullet) => OnPlayerBurMiss?.Invoke(bullet);

    public static event Action<float, bool> OnLowerMusic;
    public static void InvokeLowerMusic(float dur, bool res) => OnLowerMusic?.Invoke(dur, res);

    public static event Action<List<Dialogue>> OnDialogue;
    public static void InvokeDialogue(List<Dialogue> list) => OnDialogue?.Invoke(list);

    public static event Action<List<int>> OnTryRemoveUsed;
    public static void InvokeTryRemoveUsed(List<int> list) => OnTryRemoveUsed?.Invoke(list);

    public static event Action<List<int>> OnSaveShopData;
    public static void InvokeSaveShopData(List<int> list) => OnSaveShopData?.Invoke(list);

    public static event Action<List<int>> OnStockShopData;
    public static void InvokeStockShopData(List<int> list) => OnStockShopData?.Invoke(list);

    public static event Action<int> OnSaveUsed;
    public static void InvokeSaveUsed(int x) => OnSaveUsed?.Invoke(x);

    public static event Action<O_Exit> OnExitOpen;
    public static void InvokeExitOpen(O_Exit exit) => OnExitOpen?.Invoke(exit);

    public static event Action<Sprite, Sprite, Color, string> OnStartBossIntro;
    public static void InvokeBossIntro(Sprite main, Sprite sub, Color color, string name) => OnStartBossIntro?.Invoke(main, sub, color, name);

    public static event Action<EBoss> OnBossSpawn;
    public static void InvokeBossSpawn(EBoss boss) => OnBossSpawn?.Invoke(boss);

    public static event Action<EBoss> OnBossDie;
    public static void InvokeBossDie(EBoss boss) => OnBossDie?.Invoke(boss);

    public static event Action<float> OnFlashWhiteScreen;
    public static void InvokeFlashWhiteScreen(float dur) => OnFlashWhiteScreen?.Invoke(dur);

    public static event Action<OW_Portal> OnFindPortal;
    public static void InvokeFindPortal(OW_Portal portal) => OnFindPortal?.Invoke(portal);

    public static event Action<O_PricedHeart> OnPricedHeartSpawn;
    public static void InvokePHeartSpawn(O_PricedHeart pheart) => OnPricedHeartSpawn?.Invoke(pheart);

    public static event Action<Challenge> OnStartChallenge;
    public static void InvokeStartChallenge(Challenge ch) => OnStartChallenge?.Invoke(ch);

    public static event Action OnDisableUnlocks;
    public static void InvokeDisableUnlocks() => OnDisableUnlocks?.Invoke();

    public static event Action OnDestroyWeapons;
    public static void InvokeDestroyWeapons() => OnDestroyWeapons?.Invoke();

    public static event Action OnUseAbility;
    public static void InvokeUseAbility() => OnUseAbility?.Invoke();

    public static event Action OnSubBreak;
    public static void InvokeSubBreak() => OnSubBreak?.Invoke();

    public static event Action OnSaveStats;
    public static void InvokeSaveStats() => OnSaveStats?.Invoke();

    public static event Action OnContact;
    public static void InvokeContact() => OnContact?.Invoke();

    public static event Action OnPlayerRevived;
    public static void InvokePlayerRevived() => OnPlayerRevived?.Invoke();

    public static event Action Destroy1Ups;
    public static void InvokeDestroy1Ups() => Destroy1Ups?.Invoke();

    public static event Action<TileWall> OnHedgeBreak;
    public static void InvokeHedgeBreak(TileWall wall) => OnHedgeBreak?.Invoke(wall);

    public static event Action<BulletPlayer> OnPlayerBulletSpawn;
    public static void InvokePlayerBulletSpawn(BulletPlayer bp) => OnPlayerBulletSpawn?.Invoke(bp);

    public static event Action<B_Knife> OnPlayerBurSpawn;
    public static void InvokePlayerBurSpawn(B_Knife b) => OnPlayerBurSpawn?.Invoke(b);

    public static event Action<B_Slash> OnPlayerSlashSpawn;
    public static void InvokePlayerSlashSpawn(B_Slash b) => OnPlayerSlashSpawn?.Invoke(b);

    public static event Action<Boomerang> OnPlayerBoomerSpawn;
    public static void InvokePlayerBoomerSpawn(Boomerang b) => OnPlayerBoomerSpawn?.Invoke(b);

    public static event Action<int, int> OnSaveBossItems;
    public static void InvokeSaveBossItems(int a, int b) => OnSaveBossItems?.Invoke(a, b);

    public static event Action<int, int> OnLoadBossItems;
    public static void InvokeLoadBossItems(int a, int b) => OnLoadBossItems?.Invoke(a, b);

    public static event Action<O_Collectable> OnGetCollectable;
    public static void InvokeGetCollectable(O_Collectable o) => OnGetCollectable?.Invoke(o);

    public static event Action<OW_Portal> OnStumpSpawn;
    public static void InvokeStumpSpawn(OW_Portal ow) => OnStumpSpawn?.Invoke(ow);

    public static event Action<int> OnDisplayPerc;
    public static void InvokeDisplayPerc(int i) => OnDisplayPerc?.Invoke(i);

    public static event Action<V_FocalPoint> OnAddFocalPoint;
    public static void InvokeAddFocalPoint(V_FocalPoint point) => OnAddFocalPoint.Invoke(point);

    public static event Action<List<int>> OnInitHatButtons;
    public static void InvokeInitHatButtons(List<int> list) => OnInitHatButtons?.Invoke(list);
}
