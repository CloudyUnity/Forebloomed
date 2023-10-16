using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class E_Dummy : Entity
{
    [SerializeField] GameObject _dmgText;
    [SerializeField] Vector2 _bounds;
    [SerializeField] float _dmgDistance;
    [SerializeField] float _dmgDur;
    [SerializeField] List<Sprite> _sprites;
    [SerializeField] SpriteRenderer _rend;

    List<GameObject> _dmgTXTs = new List<GameObject>();

    public static float TimeSinceHit = 0;
    public static bool FamiliarsAllowedToHit => TimeSinceHit < 5;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, _bounds * 2);
    }

    public override void Start()
    {
        if (A_SaveManager.ChallengeName == "Halloween")
        {
            Destroy(gameObject);
        }

        _rend.sprite = _sprites.RandomItem();
        TimeSinceHit = 99;
    }

    public override void Update()
    {
        base.Update();
        LoopStatusEffects();
        string dps = CalculateDPS();

        foreach (TMP_Text txt in _dpsText)
        {
            txt.text = dps;
        }

        TimeSinceHit += Time.deltaTime;
        if (Input.GetKeyDown(A_InputManager.Instance.Key("Shoot")))
            TimeSinceHit = 0;
    }

    public override void TakeDamage(float dmg)
    {
        Anim.enabled = true;
        AnimDamage();

        A_EventManager.InvokePlaySFX("EnemyHit");

        AddDamage(dmg + Brittle, Time.time);

        GameObject go = Instantiate(_dmgText, RandomPos, Quaternion.identity);
        _dmgTXTs.Add(go);
        var text = go.GetComponent<TMP_Text>();
        text.text = (Mathf.Round(dmg * 100)/100).ToString();
        Squash(0.1f);
        StartCoroutine(C_MoveDMG(go, text));
    }

    Vector2 RandomPos => transform.position + new Vector3(_bounds.x.AsRange(), _bounds.y.AsRange());

    IEnumerator C_MoveDMG(GameObject go, TMP_Text text)
    {
        float elapsed = 0;
        Vector2 startPos = go.transform.position;
        Vector2 endPos = startPos + new Vector2(0, _dmgDistance);

        while (elapsed < _dmgDur)
        {
            float curved = A_Extensions.CosCurve(elapsed / _dmgDur);
            go.transform.position = Vector2.Lerp(startPos, endPos, curved);

            if (elapsed > _dmgDur / 2)
            {
                Color newColor = text.color;
                newColor.a = 2 - 2 * curved;
                text.color = newColor;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        _dmgTXTs.Remove(go);
        Destroy(go);
    }

    struct Damage
    {
        public float Dmg;
        public float Time;

        public Damage(float dmg, float time)
        {
            Dmg = dmg;
            Time = time;
        }
    }

    List<Damage> _dmgTracker = new List<Damage>();
    float _dpsCounter;
    [SerializeField] float _dpsLength;
    [SerializeField] List<TMP_Text> _dpsText;

    void AddDamage(float dmg, float time)
    {
        _dpsCounter += dmg;
        _dmgTracker.Add(new Damage(dmg, time));
    }

    static bool _heavyTrig, _nucTrig, _arcTrig;

    float _currentDPS;
    string CalculateDPS()
    {
        if (_dmgTracker.Count == 0)
        {
            return "Hit me!";
        }

        while (Time.time > _dmgTracker[0].Time + _dpsLength)
        {
            _dpsCounter -= _dmgTracker[0].Dmg;
            _dmgTracker.RemoveAt(0);

            if (_dmgTracker.Count == 0)
            {
                return "Hit me!";
            }
        }

        float dps = _dpsCounter / _dpsLength;
        dps = Mathf.Lerp(_currentDPS, dps, Time.deltaTime * 10);
        _currentDPS = dps;
        dps = dps.RoundTo(2);

        if (dps >= 15 && !_heavyTrig)
        {
            A_EventManager.InvokeUnlock("Heavy Weapons");
            _heavyTrig = true;
        }

        if (dps >= 50 && !_nucTrig)
        {
            A_EventManager.InvokeUnlock("Nuclear Weapons");
            _nucTrig = true;
        }

        if (dps >= 1000 && !_arcTrig)
        {
            A_EventManager.InvokeUnlock("Arcane Weapons");
            _arcTrig = true;
        }
        
        return "Dps:" + dps.ToString();    
    }

    public override void Die(GameObject toDie)
    {
        base.Die(toDie);
        A_EventManager.InvokeUnlock("Pyromaniac");
        A_EventManager.InvokePlaySFX("Dead");

        foreach(GameObject go in _dmgTXTs)
        {
            Destroy(go);
        }
    }
}
