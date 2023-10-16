using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P_Jack : Player
{
    public int Ammo;
    [SerializeField] GameObject _ammoPrefab;
    public bool HasAmmo { get { return Ammo > 0; } }
    public int AmmoToGrab;

    [SerializeField] List<GameObject> _fireOrbitals = new List<GameObject>();
    [SerializeField] GameObject _fire;
    [SerializeField] SpriteRenderer _rend, _blackRend, _fireRend;

    [SerializeField] List<GameObject> _fires = new List<GameObject>();

    float _ammoToGrabFailSafe;
    int _ammoToGrabLast;

    public override void OnEnable()
    {
        base.OnEnable();
        A_EventManager.OnEntityDie += SpawnAmmo;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        A_EventManager.OnEntityDie -= SpawnAmmo;
    }

    void SpawnAmmo(Entity entity)
    {
        if (Random.Range(0, 100f) > entity.FireChance + CurStats.Luck)
            return;

        if (_fires.Count >= 10)
        {
            Destroy(_fires[0]);
            _fires.RemoveAt(0);
        }

        GameObject go = Instantiate(_ammoPrefab, entity.transform.position, Quaternion.identity);
        _fires.Add(go);
    }

    public override void Update()
    {
        base.Update();

        _blackRend.enabled = _rend.enabled;
        _fireRend.enabled = _rend.enabled;

        AbilityPercent = Ammo * 0.2f;

        _ammoToGrabFailSafe += Time.deltaTime;
        if (_ammoToGrabLast != AmmoToGrab)
        {
            _ammoToGrabFailSafe = 0;
        }       
        else if (_ammoToGrabFailSafe > 1)
        {
            AmmoToGrab = 0;
        }

        _ammoToGrabLast = AmmoToGrab;
    }

    public void GetAmmo()
    {
        Ammo = Mathf.Clamp(Ammo + 1, 0, 5);

        GameObject go = Instantiate(_fire, transform.position, Quaternion.identity);
        _fireOrbitals.Add(go);
    }

    public void UseAllAmmo()
    {
        Ammo = 0;
        Squash(0.1f);

        for (int i = _fireOrbitals.Count - 1; i >= 0; i--)
        {
            GameObject go = _fireOrbitals[i];
            A_OrbitalManager.Instance.Remove(0, go);
            StartCoroutine(C_Shrink(go));
        }
        A_OrbitalManager.Instance.RemoveNulls();
        A_EventManager.InvokeUseAbility();

        _fireOrbitals.Clear();
    }


    IEnumerator C_Shrink(GameObject go)
    {
        float elapsed = 0;
        float dur = 0.3f;
        Vector2 startSize = go.transform.localScale;
        go.GetComponent<H_Orbital>().enabled = false;
        Vector2 startPos = go.transform.position;

        while (elapsed < dur)
        {
            float curved = A_Extensions.CosCurve(elapsed / dur);
            go.transform.localScale = Vector2.Lerp(startSize, Vector2.zero, curved);
            go.transform.position = Vector2.Lerp(startPos, transform.position, curved);
            elapsed += Time.deltaTime;
            yield return null;
        }
        Destroy(go);
    }
}
