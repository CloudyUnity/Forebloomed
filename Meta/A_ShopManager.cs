using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class A_ShopManager : MonoBehaviour
{
    [SerializeField] GameObject _shopPrefab;

    [SerializeField] Vector2[] _shopPlaces;
    [SerializeField] ItemData _aiderBot;
    [SerializeField] ParticleSystem _restockPS;
    bool _spawnedBot;

    List<int> _spawned = new List<int>();
    bool _stocked;

    [SerializeField] GameObject _gardenDecor;
    [SerializeField] GameObject _decayDecor;
    [SerializeField] GameObject _oasisDecor;
    [SerializeField] GameObject _wetlandsDecor;
    [SerializeField] GameObject _bossDecor;

    [SerializeField] List<GameObject> _charNPCs = new List<GameObject>();
    [SerializeField] List<GameObject> _secretNPCs = new List<GameObject>();

    [SerializeField] List<GameObject> _pots = new List<GameObject>();

    [SerializeField] GameObject _bigBox;
    [SerializeField] GameObject _recycler;
    [SerializeField] GameObject _normalModeRecyclers;
    [SerializeField] int _debugFixedItemData;
    [SerializeField] ParticleSystem _dustPS;

    bool _fakeStart;

    private void OnEnable()
    {
        A_EventManager.OnRestock += Restock;
        A_EventManager.OnNextScene += SaveNull;
        A_EventManager.OnNextScene += AscendPlayer;
    }
    private void OnDisable()
    {
        A_EventManager.OnRestock -= Restock;
        A_EventManager.OnNextScene -= SaveNull;
        A_EventManager.OnNextScene -= AscendPlayer;
    }

    void SaveNull() => A_EventManager.InvokeSaveShopData(new List<int>());    

    private void FakeStart()
    {
        if (A_LevelManager.Instance.HardMode)
        {
            _normalModeRecyclers.SetActive(false);

            if (A_Extensions.Rand(A_LevelManager.QuickSeed + 473621))
            {
                _bigBox.SetActive(true);
                _recycler.SetActive(false);
            }                
            else
            {
                _bigBox.SetActive(false);
                _recycler.SetActive(true);
            }                
        }

        StartCoroutine(C_DropPlayerIn());

        int world = A_LevelManager.Instance.WorldIndex();
        Debug.Log("WORLD: " + world);
        switch (world)
        {
            case 1:
                _gardenDecor.SetActive(true);
                break;
            case 2:
                _decayDecor.SetActive(true);
                break;
            case 3:
                _oasisDecor.SetActive(true);
                break;
            case 4:
                _wetlandsDecor.SetActive(true);
                break;
            default:
                break;
        }
        if (A_LevelManager.Instance.CurrentLevel.Is(4, 10, 16, 22))
            _bossDecor.SetActive(true);

        int potAmount = A_Extensions.RandomBetween(2, 5, A_LevelManager.QuickSeed);
        for (int i = 0; i < potAmount; i++)
        {
            GameObject pot = _pots.RandomItem(A_LevelManager.QuickSeed + i);
            pot.SetActive(true);
            _pots.Remove(pot);
        }

        if (A_Extensions.RandomChance(0.03f, A_LevelManager.QuickSeed + 3)) 
        {
            SpawnRandomCorpses(world);
            return;
        }

        float chance = 0.035f + (A_LevelManager.Instance.CurrentLevel.Is(6, 12, 18) ? 0.02f : 0f);
        if (A_Extensions.RandomChance(chance, A_LevelManager.QuickSeed + 436, out double val))
        {
            if (!A_SaveMetaManager.SolanaUnlocked)
                _charNPCs.RemoveAt(7);

            Debug.Log("NPC WON!");
            _charNPCs.RemoveAt(Player.Instance.CharacterIndex);
            _charNPCs.RandomItem(A_LevelManager.QuickSeed).SetActive(true);
            return;
        }
        Debug.Log("NPC Failed at chance: " + val);

        if (A_Extensions.RandomChance(0.002f, A_LevelManager.QuickSeed + 327324))
        {
            _secretNPCs.RandomItem(A_LevelManager.QuickSeed).SetActive(true);
            return;
        }
    }

    IEnumerator C_DropPlayerIn()
    {
        Player.Instance.InCutscene = true;
        Player.Instance.transform.position = new Vector3(0, 8);
        yield return new WaitForSecondsRealtime(0.3f);

        float elapsed = 0;
        float dur = 0.6f;
        Vector2 start = new Vector2(0, 8);
        Vector2 end = new Vector2(0, -1);

        while (elapsed < dur)
        {
            float t = (elapsed / dur).Pow(2);
            Player.Instance.transform.position = Vector2.Lerp(start, end, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        Player.Instance.InCutscene = false;
        A_EventManager.InvokeCameraShake(0.02f, 0.2f);
        A_EventManager.InvokePlaySFX("Drop");
        _dustPS.Play();
    }

    public void AscendPlayer() => StartCoroutine(C_AscendPlayer());
    IEnumerator C_AscendPlayer()
    {
        Player.Instance.InCutscene = true;        

        float elapsed = 0;
        float dur = 0.1f;
        Vector2 start = Player.Instance.transform.position;
        Vector2 end = new Vector2(5, 3.2f);

        while (elapsed < dur)
        {
            float t = (elapsed / dur).Pow(2);
            Player.Instance.transform.position = Vector2.Lerp(start, end, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0;
        dur = 0.5f;
        start = end;
        end = new Vector2(5, 10);

        while (elapsed < dur)
        {
            float t = (elapsed / dur).Pow(2);
            Player.Instance.transform.position = Vector2.Lerp(start, end, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private void Update()
    {
        if (A_LevelManager.Instance == null)
            return;

        if (!_fakeStart)
            FakeStart();
        _fakeStart = true;

        if (_stocked)
            return;

        for (int i = 0; i < _shopPlaces.Length; i++)
        {
            if (A_LevelManager.Instance.HardMode && i == _shopPlaces.Length - 1)
                break;

            Vector2 pos = _shopPlaces[i];
            SpawnShopItem(pos, A_LevelManager.ItemCost(11, 14, pos.x), 1);
        }
        _stocked = true;
    }

    void SpawnShopItem(Vector2 pos, int cost, int count)
    {
        ShopItem shopItem = new ShopItem()
        {
            Item = ItemHolder.Instance.GetRandomItem(count * 3.4f + cost * pos.x, ItemPool.Shop, out int index),
            Cost = cost,
            Count = count,
        };

#if UNITY_EDITOR
        if (_debugFixedItemData != 0 && pos.x == -0.5f)
        {
            shopItem.Item = ItemHolder.Instance.Get(_debugFixedItemData);
        }
#endif

        for (int i = 1; _spawned.Contains(index) && i < 1000; i += 15)
        {
            shopItem.Item = ItemHolder.Instance.GetRandomItem(count * i + cost, ItemPool.Shop, out index);
        }

        if (ItemManager.Instance.AllItemIndexes().Contains(39) && !_spawnedBot && A_LevelManager.Instance.CurrentLevel.Is(6, 12, 18, 24))
        {
            shopItem.Item = _aiderBot;
            index = 39;
        }

        _spawned.Add(index);
        _spawnedBot = true;

        GameObject pricedItem = Instantiate(_shopPrefab, pos, Quaternion.identity);
        ItemPriced pricedItemScript = pricedItem.GetComponent<ItemPriced>();
        pricedItemScript.ShopData = shopItem;
    }

    void Restock(Vector2 pos, int cost, int count) => StartCoroutine(C_Restock(pos, cost, count));

    IEnumerator C_Restock(Vector2 pos, int cost, int count)
    {
        yield return new WaitForSeconds(1);
        SpawnShopItem(pos, cost, count);
        Instantiate(_restockPS, pos, Quaternion.identity);
    }

    [SerializeField] GameObject _emptyCorpse;
    [SerializeField] List<Sprite> _corpseGarden = new List<Sprite>();
    [SerializeField] List<Sprite> _corpseDecay = new List<Sprite>();
    [SerializeField] List<Sprite> _corpseOasis = new List<Sprite>();
    [SerializeField] List<Sprite> _corpseWetlands = new List<Sprite>();
    void SpawnRandomCorpses(int world)
    {
        List<Sprite> corpses = world == 1 ? _corpseGarden : world == 2 ? _corpseDecay : world == 3 ? _corpseOasis : _corpseWetlands;
        for (int i = 0; i < 3; i++)
        {
            Vector2 pos = new Vector2();
            pos.x = A_Extensions.RandomBetween(-1, 5f, A_LevelManager.QuickSeed * (i + 1));
            pos.y = A_Extensions.RandomBetween(3f, -0.3f, A_LevelManager.QuickSeed * (i + 8362f));
            GameObject go = Instantiate(_emptyCorpse, pos, Quaternion.identity);

            SpriteRenderer rend = go.GetComponent<SpriteRenderer>();
            rend.sprite = corpses.RandomItem(A_LevelManager.QuickSeed * (i + 83f));
            if (A_Extensions.Rand())
                rend.flipX = true;

            float scale = A_Extensions.RandomBetween(0.4f, 0.8f, A_LevelManager.QuickSeed * (i + 463f));
            go.transform.localScale = Vector2.one * scale;
        }
    }
}

[System.Serializable]
public struct ShopItem
{
    public ItemData Item;
    public int Cost;
    public int Count;
}
