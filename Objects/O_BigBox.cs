using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class O_BigBox : MonoBehaviour
{
    [SerializeField] SpriteRenderer _rend;
    [SerializeField] ParticleSystem _lockBoxPS;
    [SerializeField] MakeInfo _rewards;
    [SerializeField] ParticleSystem _burstPS;
    [SerializeField] BoxCollider2D _coll;
    [SerializeField] GameObject _stickers;
    [SerializeField] ParticleSystem _ps;
    [SerializeField] BoxCollider2D _coll2;

    bool _opened = false;

    bool _selected { get { return Player.Selected == gameObject; } }

    KeyCode _key;

    private void Start()
    {
        _key = A_InputManager.Instance.Key("Interact");
    }

    private void Update()
    {
        if (Player.Instance != null && Player.Instance.SoftStats.GoldKeys > 0 && !_ps.isPlaying)
            _ps.Play();

        if (!_selected || _opened)
            return;

        _key = A_InputManager.Instance.Key("Interact");

        if (Input.GetKeyDown(_key))
        {
            if (Player.Instance.SoftStats.GoldKeys == 0)
            {
                A_EventManager.InvokePlaySFX("Error");
                if (V_HUDManager.Instance != null)
                {
                    if (Player.Instance.SoftStats.Keys > 0) V_HUDManager.Instance.AssignErrorHelpMessage("Wrong key!");
                    else V_HUDManager.Instance.AssignErrorHelpMessage("No gold keys!");
                }
                return;
            }

            Open();
        }
    }

    void Open()
    {
        Player.Instance.UseBigKey();

        _lockBoxPS.Play();
        _ps.Stop();
        _coll2.enabled = false;

        A_EventManager.InvokeCameraShake(0.01f, 0.5f);
        A_EventManager.InvokePlaySFX("Chest Open");
        A_EventManager.InvokeUnlock("Treasure!");

        Player.Selected = null;

        StartCoroutine(C_ExplodeOpen());
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (_opened)
            return;

        if (Player.Selected != null)
            return;

        if (!collision.gameObject.CompareTag("Player"))
            return;

        Player.Selected = gameObject;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (_selected && collision.gameObject.tag == "Player")
            Player.Selected = null;
    }

    IEnumerator C_DropItem(GameObject item)
    {
        float radius = Random.Range(1.2f, 1.5f);

        float elapsed = 0;
        float dur = 0.3f;
        Vector3 rise = transform.position + new Vector3(0, 2f, 0);
        Vector3 end = transform.position + Random.onUnitSphere * radius;

        while (elapsed < dur)
        {
            if (Vector3.Distance(item.transform.position, Player.Instance.transform.position) > 50)
                yield break;

            float humped = 1 - A_Extensions.HumpCurve(elapsed / dur, 0, 1);

            if (elapsed < dur / 2)
                item.transform.position = Vector3.Lerp(transform.position, rise, humped);
            else
                item.transform.position = Vector3.Lerp(end, rise, humped);

            elapsed += Time.deltaTime;
            yield return null;
        }
        item.transform.position = end;
    }

    IEnumerator C_ExplodeOpen()
    {
        float dur = 0.5f;
        Vector2 pos = transform.position;
        float elapsed = 0;

        A_EventManager.InvokePlaySFX("BigBoxShake");

        while (elapsed < dur)
        {
            transform.position = pos + new Vector2(Random.value * 0.1f, Random.value * 0.1f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        Instantiate(_burstPS, pos, Quaternion.identity);

        if (A_Extensions.RandomChance(0.1f, A_LevelManager.QuickSeed))
        {
            GameObject item1 = A_Factory.Instance.MakeItem(98, transform.position + new Vector3(0, 0.35f, 0), ItemPool.Boss);
            StartCoroutine(C_DropItem(item1));
        }
        else
        {
            GameObject item1 = A_Factory.Instance.MakeItem(5, transform.position + new Vector3(0, 0.35f, 0), ItemPool.LockBox);
            StartCoroutine(C_DropItem(item1));
        }

        GameObject item2 = A_Factory.Instance.MakeItem(93.6f, transform.position + new Vector3(0, 0.35f, 0), ItemPool.LockBox);
        StartCoroutine(C_DropItem(item2));

        A_Factory.Instance.MakeBasic(transform.position, _rewards);
        A_EventManager.InvokeCameraShake(0.05f, 0.1f);
        A_LevelManager.Instance.Sleep(50);
        A_EventManager.InvokePlaySFX("Crack");

        _rend.enabled = false;
        _coll.enabled = false;        

        _stickers.SetActive(false);

        yield return new WaitForSeconds(6);

        Destroy(gameObject);
    }
}
