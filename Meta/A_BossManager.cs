using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class A_BossManager : MonoBehaviour
{
    [SerializeField] GameObject _boss;
    [SerializeField] Sprite _mainIcon;
    [SerializeField] Sprite _subIcon;
    [SerializeField] Color _color;
    [SerializeField] string _name;
    [SerializeField] O_Exit _exit;
    [SerializeField] Vector2[] _spawnPoint;
    [SerializeField] LayerMask _wallLayer;
    bool _playerTookDamage;
    [SerializeField] bool _spawnEarly;
    [SerializeField] ItemData _bossItem;

    [SerializeField] GameObject _altBoss;
    [SerializeField] string _altName;
    [SerializeField] Color _altColor;
    [SerializeField] MakeInfo _heartInfo;

    List<BulletEnemy> _bullets = new List<BulletEnemy>();

    public static bool HideUIDebug;

    private void OnEnable()
    {
        A_EventManager.OnBulletEnemySpawn += AddBullet;
        A_EventManager.OnBossDie += BossDied;
        A_EventManager.OnDealtDamage += PlayerTookDamage;
    }

    private void OnDisable()
    {
        A_EventManager.OnBulletEnemySpawn -= AddBullet;
        A_EventManager.OnBossDie -= BossDied;
        A_EventManager.OnDealtDamage -= PlayerTookDamage;
    }

    private void Start()
    {
        Player.Instance.InCutscene = true;

        //A_SaveMetaManager.instance.Load();       
    }

    bool _start;
    private void Update()
    {
        if (_start || Player.Instance == null)
            return;

        _start = true;

        float chance = 0.1f * (A_LevelManager.Instance.DifficultyModifier - 1) + 0.1f;
        Debug.Log("ALT Boss Chance: " + chance);
        Debug.Log("Beaten game: " + A_SaveMetaManager.BeatenGame);
        bool probWon = A_Extensions.RandomChance(chance, A_LevelManager.QuickSeed * chance, out double val);
        if (_altBoss != null && A_SaveMetaManager.BeatenGame && probWon)
        {
            _boss = _altBoss;
            _name = _altName;
            _color = _altColor;
            A_LevelManager.Instance.IsAltBoss = true;
        }
        Debug.Log("Alt boss calculated at: " + val);

        A_EventManager.InvokePlaySFX("BossIntro");

        StartCoroutine(C_BossIntro());
    }

    void AddBullet(BulletEnemy bullet) => _bullets.Add(bullet);

    IEnumerator C_BossIntro()
    {
        if (_spawnEarly)
            Instantiate(_boss, _spawnPoint.RandomItem(A_LevelManager.QuickSeed), Quaternion.identity);

        float timer = 0;
        while (!Input.GetKeyDown(A_InputManager.Instance.Key("Interact")) && timer < 0.8f)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        Player.Instance.InCutscene = false;

        A_EventManager.InvokeBossIntro(_mainIcon, _subIcon, _color, _name);

        if (_spawnEarly)
            yield break;

        timer = 0;
        while (!Input.GetKeyDown(A_InputManager.Instance.Key("Interact")) && timer < 0.5f)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }
        Instantiate(_boss, _spawnPoint.RandomItem(A_LevelManager.QuickSeed), Quaternion.identity);
        // ^ OnBossSpawn > Health Bar
    }


    void BossDied(EBoss boss) => StartCoroutine(C_BossDefeat(boss));
    IEnumerator C_BossDefeat(EBoss boss)
    {
        foreach (BulletEnemy bullet in _bullets)
        {
            if (bullet != null)
            {
                bullet.Dissipate();
            }
        }

        A_EventManager.InvokeFlashWhiteScreen(1);

        yield return new WaitForSeconds(0.5f);

        MakeItems(boss.transform.position);
        _heartInfo.Amount = A_LevelManager.Instance.HardMode ? Vector2.one : new Vector2(2, 3);
        A_Factory.Instance.MakeBasic(boss.transform.position, _heartInfo);

        Vector2 pos = boss.transform.position - new Vector3(0, 2);
        pos.x = pos.x.RoundToNearest(1);
        pos.y = pos.y.RoundToNearest(1);
        if (pos.x == 0 && pos.y.Is(0, -2))
        {
            pos += new Vector2(-2, 2);
        }
        _exit.transform.position = pos;
        _exit.gameObject.SetActive(true);
        FlattenLand(boss.transform.position);

        boss.ActuallyDie();

        bool cholla = Player.Instance.CharacterIndex == 3 && ((P_Cholla)Player.Instance)._invincible;
        bool spring = Player.Instance.CharacterIndex == 2 && P_Springleaf._overclocked;
        if (cholla || spring)
        {
            A_EventManager.InvokeUnlock("Power Play");
            if (Player.Instance.CharacterIndex == 3)
                A_EventManager.InvokeUnlock("Angry");
        }            

        if (boss.EnemyName == "Katydid")
        {
            A_EventManager.InvokeUnlock("Novice");

            if ((ItemManager.Instance.AllItems.Count == 0) ||
                (Player.Instance.CharacterIndex == 2 && ItemManager.Instance.AllItems.Count <= 2) ||
                (Player.Instance.CharacterIndex == 4 && ItemManager.Instance.AllItems.Count <= 1) ||
                (Player.Instance.CharacterIndex == 5 && ItemManager.Instance.AllItems.Count <= 1))
                A_EventManager.InvokeUnlock("Minimalist");
        }

        if (boss.EnemyName == "Tella")
        {
            A_EventManager.InvokeUnlock("Not my universe");

            if ((ItemManager.Instance.AllItems.Count == 0) ||
                (Player.Instance.CharacterIndex == 2 && ItemManager.Instance.AllItems.Count <= 2) ||
                (Player.Instance.CharacterIndex == 4 && ItemManager.Instance.AllItems.Count <= 1) ||
                (Player.Instance.CharacterIndex == 5 && ItemManager.Instance.AllItems.Count <= 1))
                A_EventManager.InvokeUnlock("Minimalist");
        }

        if (boss.EnemyName.Is("Scolopendra", "Leviathan"))
        {
            A_EventManager.InvokeUnlock("Expert");

            if (boss.EnemyName == "Leviathan")
                A_EventManager.InvokeUnlock("Bullet");
        }

        if (boss.EnemyName.Is("Concheror", "Colossix"))
        {
            A_EventManager.InvokeUnlock("Slayer");

            if (boss.EnemyName == "Colossix")
                A_EventManager.InvokeUnlock("Blind");
        }

        if (boss.EnemyName == "Wallum Toad")
        {
            A_EventManager.InvokeUnlock("Revolution");
            A_EventManager.InvokeCameraShake(0.02f, 999f);

            if (Player.Instance.CharacterIndex == 1)
                A_EventManager.InvokeUnlock("Cherry Tree");
            else if (Player.Instance.CharacterIndex == 2)
                A_EventManager.InvokeUnlock("Solder Tree");
            else if (Player.Instance.CharacterIndex == 3)
                A_EventManager.InvokeUnlock("Saguaro Tree");
            else if (Player.Instance.CharacterIndex == 4)
                A_EventManager.InvokeUnlock("Shadow Tree");
            else if (Player.Instance.CharacterIndex == 5)
                A_EventManager.InvokeUnlock("Strangler Tree");
            else if (Player.Instance.CharacterIndex == 6)
                A_EventManager.InvokeUnlock("Accursed Tree");
            else
                A_EventManager.InvokeUnlock("Oak Tree");

            if (A_LevelManager.Instance.HardMode)
            {
                A_EventManager.InvokeUnlock("Hardened");

                if (Player.Instance.CharacterIndex == 1)
                    A_EventManager.InvokeUnlock("Blossom");
                else if (Player.Instance.CharacterIndex == 2)
                    A_EventManager.InvokeUnlock("Eyepatch");
                else if (Player.Instance.CharacterIndex == 3)
                    A_EventManager.InvokeUnlock("Wildfrost");
                else if (Player.Instance.CharacterIndex == 4)
                    A_EventManager.InvokeUnlock("Horns");
                else if (Player.Instance.CharacterIndex == 5)
                    A_EventManager.InvokeUnlock("Twigs");
                else if (Player.Instance.CharacterIndex == 6)
                    A_EventManager.InvokeUnlock("Peas");
                else
                    A_EventManager.InvokeUnlock("Leaf");
            }

            if (!A_LevelManager.Instance.TookDamage)
                A_EventManager.InvokeUnlock("Perfect Week");

            if (A_LevelManager.Instance.GlobalTime <= 1200)
                A_EventManager.InvokeUnlock("Speedster");

            if (A_LevelManager.Instance.GlobalTime <= 660)
                A_EventManager.InvokeUnlock("Lightning");

            if (A_LevelManager.Instance.Kills == 0)
                A_EventManager.InvokeUnlock("Entomophile");

            if (A_LevelManager.Instance.MiniBossesKilled >= 8)
                A_EventManager.InvokeUnlock("Good work ethic");

            if ((ItemManager.Instance.AllItems.Count == 0) ||
                (Player.Instance.CharacterIndex == 2 && ItemManager.Instance.AllItems.Count <= 2) ||
                (Player.Instance.CharacterIndex == 4 && ItemManager.Instance.AllItems.Count <= 1) ||
                (Player.Instance.CharacterIndex == 5 && ItemManager.Instance.AllItems.Count <= 1))
                A_EventManager.InvokeUnlock("Vanilla");

            if (A_LevelManager.Instance.ShopkeeperTalks >= 11)
                A_EventManager.InvokeUnlock("Psycho");
        }

        yield return new WaitForSeconds(3);

        _exit.Open();
    }

    void MakeItems(Vector3 bossPos)
    {
        if (!_playerTookDamage)
        {
            A_Factory.Instance.MakeItem(808, bossPos + new Vector3(2, 0), ItemPool.BossBonus, out int _);
            A_EventManager.InvokeUnlock("Flawless");
        }

        if (A_Extensions.RandomChance(0.125f, A_LevelManager.QuickSeed + 4653, out double ch))
        {
            Debug.Log("Pet Won: " + ch);
            A_Factory.Instance.TurnToItem(bossPos + new Vector3(0, 2), _bossItem, 0);
            A_EventManager.InvokeUnlock("BFF");
            return;
        }
        Debug.Log("Pet Failed: " + ch);

        A_Factory.Instance.MakeItem(909, bossPos + new Vector3(0, 2), ItemPool.Boss, out int _);
    }

    void FlattenLand(Vector2 pos)
    {
        var hits = Physics2D.OverlapCircleAll(pos, 5, _wallLayer);
        foreach (var hit in hits)
        {
            if (hit == null)
                continue;

            hit.GetComponent<TileWall>().Convert(true);
        }
    }

    void PlayerTookDamage(int _, string __) => _playerTookDamage = true;
}
