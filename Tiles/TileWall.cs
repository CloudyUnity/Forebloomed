using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileWall : Tile
{
    [SerializeField] TilePreset FloorTile;
    public float HitPoints;
    [SerializeField] SpriteRenderer[] _flashers;
    [SerializeField] SpriteRenderer[] _allParts;
    [SerializeField] GameObject[] _wedgedGold;
    [SerializeField] bool _useSandSFX;
    [SerializeField] GameObject _fakeFloor = null;
    [SerializeField] GameObject _interactables = null;
    [SerializeField] bool _invincible;

    [SerializeField] bool _debugMode = false;

    [SerializeField] LayerMask WallTileLayer;
    public float TargetAlpha = 1;
    bool _dropped;

    [SerializeField] MakeInfo _chest;

    bool _reEnabled = false;

    MakeInfo _infoDrop;

    public static readonly float[] HitPointsList = new float[]
    {
        1, 
        1.25f,
        1.5f,
        2
    };

    private void OnEnable()
    {
        transform.position = _myPos;

        if (_reEnabled)
            RecentreAndDeFlash();
        else
            _reEnabled = true;
    }

    public float GetHitPoints()
    {
        if (A_LevelManager.Instance == null)
            return 1;

        if (A_LevelManager.Instance.DifficultyModifier > 1)
        {
            return 2 * A_LevelManager.Instance.WorldIndex() + (A_LevelManager.Instance.DifficultyModifier - 1) * 5;
        }

        if (A_LevelManager.Instance.WorldIndex() == 1)
            return 0.95f;
        if (A_LevelManager.Instance.WorldIndex() == 2)
            return 1.25f;
        if (A_LevelManager.Instance.WorldIndex() == 3)
            return 1.5f;
        if (A_LevelManager.Instance.WorldIndex() == 4)
            return 2.5f;

        return 999999999;
    }

    public void Awake()
    {
        SetMyPos(transform.position);

        HitPoints = GetHitPoints();

        if (TileDrops.Instance == null)
            return;

        _infoDrop = TileDrops.Instance.GetGridType(_myPos);

        if (_infoDrop.Name == "Gold")
        {
            Instantiate(_wedgedGold.RandomItem(A_LevelManager.QuickSeed + (transform.position.x * 472376) + (transform.position.y * 642573)), transform);
        }
    }

    public void ActivateFakeFloor()
    {
        _fakeFloor?.SetActive(true);
        if (_interactables != null)
            _interactables.SetActive(true);
    }

    protected override void Update()
    {
        base.Update();

        if (!Explored || _interactables == null || !_interactables.activeSelf || _fakeFloor == null || !_fakeFloor.activeSelf)
            return;

        if (_allParts[0].color.a != TargetAlpha)
        {
            foreach (var rend in _allParts)
            {
                rend.SetAlpha(Mathf.Lerp(rend.color.a, TargetAlpha, Time.deltaTime * 8));
            }
        }

        if (_fire.Dur > 0)
        {
            FireDamage();
        }
    }

    public void TakeDamage(float dmg)
    {
        if (dmg <= 0 || _invincible)
            return;

        HitPoints -= dmg;

        A_EventManager.InvokePlaySFX("EnemyHit");

        StartCoroutine(C_ShakeIt(0.03f, 0.05f));

        if (_interactables != null)
            _interactables.SetActive(true);

        if (HitPoints <= 0 && !_dropped)
        {
            _dropped = true;
            Death();
            return;
        }

        if (!Explored)
            return;

        StartCoroutine(Flash());
    }

    void Death()
    {
        Instantiate(GroundBurstPS, _myPos, Quaternion.identity);
        Instantiate(LeafDropPS, _myPos, Quaternion.identity);
        A_EventManager.InvokeCameraShake(0.005f, 0.07f);

        if (_useSandSFX)
            A_EventManager.InvokePlaySFX("SandWall");
        else
            A_EventManager.InvokePlaySFX("Wall");

        Convert(true);

        A_EventManager.InvokeHedgeBreak(this);
        A_LevelManager.Instance.Sleep(10);

        if (A_LevelManager.Instance.TimeSinceChestSpawn > 70)
        {
            A_Factory.Instance.MakeBasic(_myPos, _chest);
            return;
        }

        _infoDrop = TileDrops.Instance.GetGridType(_myPos);

        if (_infoDrop.Prefab == null)
            return;

        A_Factory.Instance.MakeBasic(_myPos, _infoDrop);
    }

    public void Convert(bool explored)
    {
        if (_debugMode)
        {
            Destroy(gameObject);
            return;
        }

        Tile tile = Instantiate(FloorTile.TilePrefab, transform.parent);
        tile.transform.position = _myPos;
        tile.transform.parent = transform.parent;

        tile.SetSprites(FloorTile.GetSprites());
        tile.SetMyPos(_myPos);

        if (explored)
            tile.MarkExplored();

        if (TileMapBoss.Instance != null)
        {
            TileMapBoss.Instance.SetTile(_myPos, tile);
            Destroy(gameObject);
            return;
        }

        if (TileMapBoss2.Instance != null)
        {
            TileMapBoss2.Instance.SetTile(_myPos, tile);
            Destroy(gameObject);
            return;
        }

        if (TileMapBoss3.Instance != null)
        {
            TileMapBoss3.Instance.SetTile(_myPos, tile);
            Destroy(gameObject);
            return;
        }

        TileMap.Instance.SetTile(_myPos, tile);
        Destroy(gameObject);
    }

    IEnumerator Flash()
    {
        float elapsed = 0;
        float dur = 0.18f;

        while (elapsed < dur)
        {
            float newAlpha = 0.5f - 0.5f * Mathf.Cos(2 * Mathf.PI * elapsed / dur);

            foreach (var flash in _flashers)
            {
                flash.SetAlpha(newAlpha);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        foreach (var flash in _flashers)
        {
            flash.SetAlpha(0);
        }
    }

    public void RecentreAndDeFlash()
    {       
        if (_flashers.Length != 0 && _flashers[0].color.a == 0)
            return;

        foreach (var flash in _flashers)
        {
            flash.SetAlpha(0);
        }
    }

    public void SetType(float hitPoints)
    {
        if (HitPoints > 9999999999999999999f)
            return;

        HitPoints = hitPoints;
    }

    IEnumerator C_ShakeIt(float mag, float dur)
    {
        float elapsed = 0;

        while (elapsed < dur)
        {
            Vector2 newPos = _myPos;
            newPos.y += mag.AsRange();
            transform.position = newPos;

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        transform.position = _myPos;
    }

    StatusEffect _fire = new StatusEffect();
    public virtual void InflictStatus()
    {
        List<StatusEffect> effects = Player.Instance.StatusEffects;
        foreach (var e in effects)
        {
            if (e.Name == "Fire")
            {
                if (e.Dur == 0)
                    return;

                _fire = new StatusEffect
                {
                    Name = "Fire",
                    Dur = A_Extensions.Greater(e.Dur, e.Dur),
                    Freq = A_Extensions.Greater(e.Freq, e.Freq),
                    FreqCounter = A_Extensions.Greater(e.Dur, e.Dur),
                    Power = A_Extensions.Greater(e.Power, e.Power),
                };
                return;
            }                
        }        
    }

    [SerializeField] ParticleSystem _firePS;
    void FireDamage()
    {
        _firePS.gameObject.SetActive(true);
        if (_fire.Dur <= _fire.FreqCounter)
        {
            _fire.FreqCounter -= _fire.Freq;
            TakeDamage(_fire.Power * 0.1f);
            A_EventManager.InvokePlaySFX("Poison");
        }

        int count = Random.Range(0f, 100f) < 9 ? 1 : 0;
        _firePS.Emit(count);

        _fire.Dur -= Time.deltaTime;
    }

    public override void SetHealthToMaximum()
    {
        HitPoints = 9999999999999999999999f;
    }
}
