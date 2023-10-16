using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public string EnemyName;

    public float HitPoints;
    [SerializeField] protected float SightRange;
    [SerializeField] protected float DespawnRange;
    [SerializeField] protected float PathFindingSize;
    [SerializeField] protected int NodesRequired;
    [SerializeField] protected float MoveDur;
    [SerializeField] protected float MoveDurDiff;
    [SerializeField] protected float ObjectPermanace;
    bool Remembers => opTimer <= ObjectPermanace;
    [SerializeField] protected int ContactDamage;
    [SerializeField] bool _noContact;
    [SerializeField] protected bool Ranged;
    public float FireChance;
    public bool Flying;
    float opTimer;
    bool _dead;

    protected LayerMask BlocksVision = 1 << 11 | 1 << 14;
    public float KnockbackMagnitude = 0.05f;

    protected bool TakingKnockback;

    [SerializeField] List<Drop> _drops = new List<Drop>();

    [SerializeField] ParticleSystem _poisonPS, _icePS, _brittlePS, _confusionPS, _shockPS;
    [SerializeField] protected Animator Anim;
    [SerializeField] protected SpriteRenderer Rend;
    [SerializeField] bool _flipX;
    [SerializeField] Sprite _whiteCorpse;
    [SerializeField] Sprite _corpse;
    [SerializeField] GameObject _corpseGO;
    [SerializeField] int _customCorpseLayer = 511;
    [SerializeField] ParticleSystem _deathPS;
    [SerializeField] bool _isMiniBoss;
    [SerializeField] string _customDeathSFX;

    [SerializeField] List<StatusEffect> statusEffects = new List<StatusEffect>();
    protected float Freeze;
    protected float Brittle;
    protected float Confused;
    protected float Shock;
    protected bool IsMoving;
    protected bool PauseMovement;

    GameObject _curGhost;
    float _ghostSwitchTimer = 0;

    protected Vector2 NormalSize;

    protected Coroutine _movingC;

    public static readonly bool ENTITY_CAP = true;
    public static readonly float ENTITY_HP_SCALING = 1.5f;
    public static readonly float MINIBOSS_HP_SCALING = 2.5f;

    public static readonly bool DEBUG_PATHS = true;

    public virtual void Start()
    {
        if (ENTITY_CAP && A_LevelManager.Instance.EntityCount > 500)
        {
            enabled = false;
            Destroy(gameObject);
            return;
        }

        A_LevelManager.Instance.EntityCount++;
        float l = A_LevelManager.Instance.DifficultyModifier;
        if (A_LevelManager.Instance.DifficultyModifier > 1)
            HitPoints *= (l - 1) * ENTITY_HP_SCALING;
        if (A_LevelManager.Instance.DifficultyModifier > 1 && _isMiniBoss)
            HitPoints *= (l - 1) * MINIBOSS_HP_SCALING;
        if (A_LevelManager.Instance.HardMode)
            HitPoints *= A_LevelManager.HARD_MODE_ENTITY_HP_MULT;

        NormalSize = transform.localScale;

        A_EventManager.InvokeEntitySpawn(this);

        StartCoroutine(C_Grow());
    }


    IEnumerator C_Grow()
    {
        float elapsed = 0;
        float dur = 0.15f;
        Vector2 scale = transform.localScale;

        while (elapsed < dur)
        {
            transform.localScale = Vector2.Lerp(Vector2.zero, scale, elapsed / dur);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = scale;
    }

    public virtual void TakeDamage(float dmg)
    {
        if (dmg <= 0)
            return;

        HitPoints -= dmg + Brittle;
        AnimDamage();

        A_EventManager.InvokePlaySFX("EnemyHit");

        if (Brittle > 0)
            A_EventManager.InvokePlaySFX("Brittle");

        if (Freeze > 0)
            A_EventManager.InvokePlaySFX("Ice");

        Squash(0.1f);

        if (HitPoints <= 0 && !Player.Instance.Dead)
        {
            Die(gameObject);
        }
    }

    public virtual void AnimDamage()
    {
        if (Anim != null)
            Anim.SetTrigger("Damage");
    }

    public virtual void InflictStatus() => InflictStatus(false);

    public virtual void InflictStatus(bool shockOnly)
    {
        List<StatusEffect> effects = Player.Instance.StatusEffects;

        for (int e = 0; e < effects.Count; e++)
        {
            if (effects[e].Name == "Shock" && !shockOnly)
                continue;
            else if (shockOnly && effects[e].Name != "Shock")
                continue;

            if (effects[e].Dur == 0)
                continue;

            if (effects[e].FreqCounter == 0)
            {
                StatusEffect newE = effects[e];
                newE.FreqCounter = effects[e].Dur;
                effects[e] = newE;
            }

            bool newSE = true;
            for (int i = 0; i < statusEffects.Count; i++)
            {                
                if (statusEffects[i].Name == effects[e].Name)
                {
                    statusEffects[i] = new StatusEffect
                    {
                        Name = effects[e].Name,
                        Dur = A_Extensions.Greater(effects[e].Dur, statusEffects[i].Dur),
                        Freq = A_Extensions.Greater(effects[e].Freq, statusEffects[i].Freq),
                        FreqCounter = A_Extensions.Greater(effects[e].Dur, statusEffects[i].Dur),
                        Power = A_Extensions.Greater(effects[e].Power, statusEffects[i].Power),
                    };
                    newSE = false;
                    break;
                }
            }
            if (newSE)
                statusEffects.Add(effects[e]);
        }
    }

    public virtual void InflictStatus(List<StatusEffect> effects)
    {
        for (int e = 0; e < effects.Count; e++)
        {
            if (Random.Range(0, 1f) - (Player.Instance.CurStats.Luck * 0.05f) > effects[e].Chance / Player.Instance.CurStats.Amount)
                continue;

            if (effects[e].Dur == 0)
                continue;

            if (effects[e].FreqCounter == 0)
            {
                StatusEffect newE = effects[e];
                newE.FreqCounter = effects[e].Dur;
                effects[e] = newE;
            }

            bool newSE = true;
            for (int i = 0; i < statusEffects.Count; i++)
            {
                if (statusEffects[i].Name == effects[e].Name)
                {
                    statusEffects[i] = new StatusEffect
                    {
                        Name = effects[e].Name,
                        Dur = A_Extensions.Greater(effects[e].Dur, statusEffects[i].Dur),
                        Freq = A_Extensions.Greater(effects[e].Freq, statusEffects[i].Freq),
                        FreqCounter = A_Extensions.Greater(effects[e].Dur, statusEffects[i].Dur),
                        Power = A_Extensions.Greater(effects[e].Power, statusEffects[i].Power),
                    };
                    newSE = false;
                    break;
                }
            }
            if (newSE)
                statusEffects.Add(effects[e]);
        }
    }

    public virtual void Update()
    {
        if (_flipX)
            Rend.flipX = Player.Instance.transform.position.x - transform.position.x < 0;

        if (Player.Instance.Dead)
            return;

        float distance = Vector2.Distance(transform.position, Player.Instance.transform.position);

        if (distance > DespawnRange && !_isMiniBoss)
            Despawn(gameObject);

        opTimer += Time.deltaTime;
        if (CanSeePlayer())
            opTimer = 0;

        _ghostSwitchTimer += Time.deltaTime;
        if (_ghostSwitchTimer > 5 || _curGhost == null)
        {
            _curGhost = Player.Instance.TargetPos();
            _ghostSwitchTimer = 0;
        }            

        if (statusEffects.Count == 0 || V_HUDManager.Instance.IsPaused)
            return;

        LoopStatusEffects();
    }

    public virtual void Despawn(GameObject toDespawn) => Destroy(toDespawn);

    public void LoopStatusEffects()
    {
        List<StatusEffect> removal = new List<StatusEffect>();
        for (int i = 0; i < statusEffects.Count; i++)
        {
            if (statusEffects[i].Name == "Poison") PoisonDamage(i);

            else if (statusEffects[i].Name == "Ice") FreezeEffect(i);

            else if (statusEffects[i].Name == "Brittle") BrittleDamage(i);

            else if (statusEffects[i].Name == "Confusion") ConfusedDamage(i);

            else if (statusEffects[i].Name == "Shock") ShockDamage(i);

            else if (statusEffects[i].Name == "Fire") continue;

            else Debug.Log("Unimplemented Status: " + statusEffects[i].Name);

            if (statusEffects[i].Dur <= 0)
                removal.Add(statusEffects[i]);
        }

        foreach(StatusEffect remove in removal)
        {
            statusEffects.Remove(remove);
        }
    }

    void Countdown(int i)
    {
        StatusEffect updated = statusEffects[i];
        updated.Dur -= Time.deltaTime;
        statusEffects[i] = updated;
    }

    bool FrequencyHit(int i)
    {
        bool hit = statusEffects[i].Dur <= statusEffects[i].FreqCounter;
        if (hit)
        {
            StatusEffect updated = statusEffects[i];
            updated.FreqCounter -= updated.Freq;
            statusEffects[i] = updated;
        }
        return hit;
    }

    void PoisonDamage(int i)
    {
        if (FrequencyHit(i))
        {
            TakeDamage(statusEffects[i].Power * 0.1f);
            A_EventManager.InvokePlaySFX("Poison");
        }

        int count = Random.Range(0f, 100f) < 3 ? 1 : 0;
        _poisonPS.Emit(count);

        Countdown(i);
    }

    void FreezeEffect(int i)
    {
        Freeze = statusEffects[i].Power;

        int count = Random.Range(0f, 100f) < Freeze ? 1 : 0;
        _icePS.Emit(count);

        Countdown(i);

        if (statusEffects[i].Dur <= 0)
            Freeze = 0;
    }

    void BrittleDamage(int i)
    {
        Brittle = statusEffects[i].Power;

        int count = Random.Range(0f, 50f) < Brittle ? 1 : 0;
        _brittlePS.Emit(count);

        Countdown(i);

        if (statusEffects[i].Dur <= 0)
            Brittle = 0;
    }

    void ConfusedDamage(int i)
    {
        Confused = statusEffects[i].Power;

        int count = Random.Range(0f, 50f) < Confused ? 1 : 0;
        _confusionPS.Emit(count);

        Countdown(i);

        if (statusEffects[i].Dur <= 0)
            Confused = 0;
    }

    void ShockDamage(int i)
    {
        Shock = statusEffects[i].Power;

        int count = Random.Range(0f, 50f) < Shock ? 1 : 0;
        _shockPS.Emit(count);

        Countdown(i);

        if (statusEffects[i].Dur <= 0)
            Shock = 0;
    }

    public void DealShockDamage()
    {
        TakeDamage(Shock);
    }

    static bool _hyahTrig;
    public virtual void Die(GameObject toDie)
    {
        if (_dead)
            return;

        _dead = true;

        MakeInfo drop = RandomDrop();
        if (!drop.Equals(default(MakeInfo)))
            A_Factory.Instance.MakeBasic(transform.position, RandomDrop());

        if (_customDeathSFX == null || _customDeathSFX == "")
            _customDeathSFX = "EnemyDie";

        A_EventManager.InvokePlaySFX(_customDeathSFX);
        A_EventManager.InvokeCameraShake(0.01f, 0.1f);

        if (_isMiniBoss)
        {
            A_EventManager.InvokeUnlock("Overfed");
            A_LevelManager.Instance.MiniBossesKilled++;

            if (EnemyName == "Butterfly")
                A_EventManager.InvokeUnlock("Wings");
        }

        if (EnemyName == "Pot")
            A_LevelManager.Instance.PotsBroken++;

        if (A_LevelManager.Instance.PotsBroken > 30 && !_hyahTrig)
        {
            A_EventManager.InvokeUnlock("Hyah");
            _hyahTrig = true;
        }

        MakeCorpse();

        A_EventManager.InvokeEntityDie(this);

        A_LevelManager.Instance.Sleep(20);
        A_LevelManager.Instance.EntityCount--;

        Destroy(toDie);
    }

    public void MakeCorpse()
    {
        if (!A_LevelManager.Instance._withinCorpseCap || Rend == null)
        {
            return;
        }
        A_LevelManager.Instance.CorpseCount++;

        GameObject go = Instantiate(_corpseGO, transform.position, Quaternion.identity);
        go.GetComponent<O_Corpse>().AssignSprites(_whiteCorpse, _corpse, NormalSize, Rend.flipX);
        go.transform.rotation = transform.rotation;
        if (_customCorpseLayer != 511)
            go.GetComponent<SpriteRenderer>().sortingOrder = _customCorpseLayer;

        if (_deathPS != null)
            Instantiate(_deathPS, transform.position, Quaternion.identity);
    }

    public void MakeCorpse(Vector2 pos, Vector2 size, bool flipX)
    {
        if (!A_LevelManager.Instance._withinCorpseCap)
        {
            return;
        }
        A_LevelManager.Instance.CorpseCount++;

        GameObject go = Instantiate(_corpseGO, pos, Quaternion.identity);
        go.GetComponent<O_Corpse>().AssignSprites(_whiteCorpse, _corpse, size, flipX);
        go.transform.rotation = transform.rotation;

        if (_deathPS != null)
            Instantiate(_deathPS, transform.position, Quaternion.identity);
    }

    public MakeInfo RandomDrop()
    {
        if (_drops.Count == 0)
            return default(MakeInfo);

        float totalPerc = 0;
        foreach (Drop drop in _drops)
        {
            totalPerc += drop.Frequency;
            if (drop.Favorable)
                totalPerc += Player.Instance.CurStats.Luck;
        }

        while (true)
        {
            foreach (Drop drop in _drops)
            {
                float perc = drop.Frequency + (drop.Favorable ? Player.Instance.CurStats.Luck : 0);
                if (Random.Range(0, totalPerc) < perc)
                {
                    return drop.info;
                }
            }
        }
    }

    public bool CanSeePlayer()
    {
        float distance = Vector2.Distance(Player.Instance.HitBox, transform.position);
        if (distance > SightRange)
            return false;

        Vector2 dir = (Player.Instance.HitBox - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, distance, BlocksVision);
        return hit.collider == null;
    }

    public IEnumerator C_InfinitePath(bool useFollowers)
    {
        if (_isMiniBoss && !CanSeePlayer())
        {
            if (DEBUG_PATHS)
                Debug.Log("Can't see you");
            yield return null;
        }            

        if (NodesRequired <= 1)
            throw new System.Exception();

        while (TakingKnockback || PauseMovement)
        {
            if (DEBUG_PATHS)
                Debug.Log("Pausing");
            yield return null;
        }

        List<Node> path = null;
        while (path == null || path.Count < NodesRequired)
        {
            while (!Remembers)
            {
                if (DEBUG_PATHS)
                    Debug.Log("Remembering");
                yield return null;
            }

            Vector2 target = useFollowers ? _curGhost.transform.position : Player.Instance.HitBox;
            target += new Vector2(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));
            path = NodeManager.Instance.FindPath(transform.position, target, PathFindingSize + Random.Range(0, 2));

            if (path == null || path.Count < NodesRequired)
            {
                if (DEBUG_PATHS)
                    Debug.Log("Invalid path, trying again soon");
                yield return new WaitForSeconds(0.2f);
            }                

            yield return null;
        }

        IsMoving = true;
       
        Vector2 startPos = transform.position;
        Vector2 endPos = path[1].pos + new Vector2(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f));

        float elapsed = 0;
        float dur = Random.Range(MoveDur - MoveDurDiff, MoveDur + MoveDurDiff) * (Freeze + 1);
        if (A_LevelManager.Instance.HardMode)
            dur *= A_LevelManager.HARD_MODE_ENTITY_SPEED_MULT;
        dur *= Vector2.Distance(startPos, endPos);

        while (elapsed < dur && !TakingKnockback && !PauseMovement)
        {
            transform.position = Vector2.Lerp(startPos, endPos, elapsed / (dur + Freeze));
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (Ranged && CanSeePlayer() && !TakingKnockback && !PauseMovement)
            yield return new WaitForSeconds(Random.Range(0, 3));

        _movingC = StartCoroutine(C_InfinitePath(useFollowers));
    }

    public IEnumerator C_MoveOneNode(Vector2 to, float dur)
    {
        if (NodesRequired <= 1)
            yield break;

        if (IsMoving)
            yield break;

        if (DEBUG_PATHS)
            Debug.Log("Starting path search");

        to += new Vector2(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));

        List<Node> path = NodeManager.Instance.FindPath(transform.position, to, PathFindingSize);

        if (path == null || path.Count < NodesRequired)
        {
            if (DEBUG_PATHS)
                Debug.Log("Path failed");
            yield break;
        }

        if (DEBUG_PATHS)
            Debug.Log("Path succeded");

        IsMoving = true;

        Vector2 startPos = transform.position;
        Vector2 endPos = path[1].pos + new Vector2(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f));
        float elapsed = 0;
        if (A_LevelManager.Instance.HardMode)
            dur *= A_LevelManager.HARD_MODE_ENTITY_SPEED_MULT;
        dur *= Vector2.Distance(startPos, endPos);

        while (elapsed < dur + Freeze && !TakingKnockback)
        {
            transform.position = Vector2.Lerp(startPos, endPos, elapsed / (dur + Freeze));
            elapsed += Time.deltaTime;
            yield return null;
        }

        IsMoving = false;
    }

    public void KnockBack(Vector2 dir)
    {
        if (KnockbackMagnitude == 0 || TakingKnockback)
            return;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, KnockbackMagnitude, BlocksVision);
        Vector2 pos = hit.collider == null ? (Vector2)transform.position + dir * KnockbackMagnitude : hit.point;

        StartCoroutine(C_KnockBack(pos));
    }

    IEnumerator C_KnockBack(Vector2 pos)
    {
        TakingKnockback = true;

        float elapsed = 0;
        float dur = 0.2f;
        Vector2 start = transform.position;

        while (elapsed < dur)
        {
            float c = A_Extensions.CosCurve(elapsed / dur);
            transform.position = Vector2.Lerp(start, pos, c);
            elapsed += Time.deltaTime;
            yield return null;
        }

        TakingKnockback = false;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            A_EventManager.InvokeDealDamage(ContactDamage, transform.position, EnemyName);
            if (_noContact)
                return;
            A_EventManager.InvokeContact();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            A_EventManager.InvokeDealDamage(ContactDamage, transform.position, EnemyName);
            if (_noContact)
                return;
            A_EventManager.InvokeContact();
        }
    }

    bool _squashing;
    public void Squash(float dur) => StartCoroutine(C_SqaushStretch(dur));
    IEnumerator C_SqaushStretch(float dur)
    {
        if (_squashing || NormalSize == Vector2.zero)
            yield break;
        _squashing = true;

        float elapsed = 0;

        while (elapsed < dur)
        {
            float humped = 1 - A_Extensions.HumpCurve(elapsed / dur, 0, 1);
            transform.localScale = NormalSize + new Vector2(0.1f * -humped, 0.1f * humped);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = NormalSize;
        _squashing = false;
    }
}

[System.Serializable]
public struct Drop
{
    public MakeInfo info;
    public float Frequency;
    public bool Favorable;
    public bool DisabledAfterDark;
}
