using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class O_Chest : MonoBehaviour
{
    public int Cost;
    public bool CanUseKey;
    public bool ForceSpawn;
    [SerializeField] TextPair Text;
    [SerializeField] MeshRenderer _textMesh, _textMesh2;
    [SerializeField] SpriteRenderer _rend;
    [SerializeField] Sprite _openedSprite;
    [SerializeField] SpriteRenderer _flasher;
    [SerializeField] BoxCollider2D _col;
    [SerializeField] GameObject _goldIcon;
    [SerializeField] ParticleSystem _chestPS;
    [SerializeField] Color _discountColor;
    [SerializeField] LayerMask _tileLayer;

    bool _opened;

    bool _eventTriggered;

    bool _selected { get { return Player.Selected == gameObject; } }

    public static readonly bool BRUTE_FORCE_ENABLED = false;

    private void Start()
    {
        if (!ForceSpawn && A_LevelManager.Instance.TimeSinceChestSpawn <= 8 && !A_LevelManager.Instance.CurrentLevel.IsEven())
        {
            Destroy(gameObject);
            return;
        }

        _rend.enabled = true;
        A_LevelManager.Instance.TimeSinceChestSpawn = 0;
    }

    public void UpdateCost(int newCost, bool isSale)
    {
        Cost = newCost;
        Text.text = Cost.ToString();
        Text.fontSize = Cost / 100 > 10 ? 1 : (Cost / 10 < 10 ? 1.8f : 1.4f);

        if (!isSale)
            return;

        Text.color = _discountColor;
    }

    private void Update()
    {
        if (!_eventTriggered)
        {
            if (Cost == 0)
            {
                Cost = A_LevelManager.ItemCost(8, 10, transform.position.x + transform.position.y);
            }

            Text.text = Cost.ToString();
            Text.fontSize = Cost / 100 > 10 ? 1 : (Cost / 10 < 10 ? 1.8f : 1.4f);

            A_EventManager.InvokeChestSpawn(this);
            _eventTriggered = true;
        }

        _textMesh.sortingOrder = _rend.sortingOrder + 1;
        _textMesh2.sortingOrder = _rend.sortingOrder + 1;

        if (_opened)
            return;

        _flasher.sortingOrder = _rend.sortingOrder + 10;

        if (!_selected)
            return;

        if (Input.GetKeyDown(A_InputManager.Instance.Key("Interact")))
        {
            if (Player.Instance.SoftStats.GoldAmount < Cost)
            {
                if (CanUseKey && BRUTE_FORCE_ENABLED && Player.Instance.SoftStats.Keys > 0)
                {
                    OpenWithKey();
                    return;
                }

                A_EventManager.InvokePlaySFX("Error");
                if (V_HUDManager.Instance != null) 
                    V_HUDManager.Instance.AssignErrorHelpMessage("Not enough gold!");
                return;
            }

            Open();
        }
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

    void Open()
    {
        Player.Instance.SoftStats.GoldAmount -= Cost;
        A_LevelManager.Instance.TotalGoldSpent += Cost;

        A_EventManager.InvokeGoldChange();
        A_EventManager.InvokeSpendGold(Cost);
        StartCoroutine(C_Shake());
        StartCoroutine(C_Shrink());
    }

    void OpenWithKey()
    {
        Player.Instance.UseKey();
        StartCoroutine(C_Shake());
    }

    void SpawnItem()
    {
        GameObject item = A_Factory.Instance.MakeItem(0, transform.position + new Vector3(0, 0.35f, 0), ItemPool.Chest);
        StartCoroutine(C_DropItem(item));

        _rend.sprite = _openedSprite;
        _col.enabled = false;
        Text.text = "";
        _goldIcon.SetActive(false);
        _chestPS.Play();

        A_EventManager.InvokeChestOpen(this);
        Squash(0.1f);
    }

    public void Squash(float dur) => StartCoroutine(C_SqaushStretch(dur));
    IEnumerator C_SqaushStretch(float dur)
    {
        float elapsed = 0;
        Vector2 start = transform.localScale;

        while (elapsed < dur)
        {
            float humped = 1 - A_Extensions.HumpCurve(elapsed / dur, 0, 1);
            transform.localScale = start + new Vector2(0.1f * -humped, 0.1f * humped);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = start;
    }

    IEnumerator C_DropItem(GameObject item)
    {
        float radius = Random.Range(0.75f, 0.9f);
        Vector2 end = transform.position + Random.onUnitSphere * radius;

        for (int i = 0; i < 50 && Physics2D.Raycast(end, Vector3.back, 1, _tileLayer).collider != null; i++)
        {
            radius = Random.Range(0.75f, 0.9f);
            end = transform.position + Random.onUnitSphere * radius;
        }

        float elapsed = 0;
        float dur = 0.3f;
        Vector2 start = item.transform.position;
        Vector3 rise = transform.position + new Vector3(0, 1f, 0);

        while (elapsed < dur)
        {
            if (Vector3.Distance(item.transform.position, Player.Instance.transform.position) > 50)
                yield break;

            float humped = 1 - A_Extensions.HumpCurve(elapsed / dur, 0, 1);

            if (elapsed < dur/2)
                item.transform.position = Vector3.Lerp(start, rise, humped);
            else
                item.transform.position = Vector3.Lerp(end, rise, humped);

            elapsed += Time.deltaTime;
            yield return null;
        }
        item.transform.position = end;
    }

    IEnumerator C_Shake()
    {
        _opened = true;
        Player.Selected = null;

        Destroy(_flasher);
        A_EventManager.InvokeCameraShake(0.01f, 0.3f);
        A_EventManager.InvokePlaySFX("ChestShake");

        A_LevelManager.Instance.Sleep(5);

        float dur = 0.3f;
        Vector2 pos = transform.position;
        float elapsed = 0;

        while (elapsed < dur)
        {
            transform.position = pos + new Vector2(Random.value * 0.1f, Random.value * 0.1f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        A_EventManager.InvokeCameraShake(0.03f, 0.05f);
        A_LevelManager.Instance.Sleep(30);
        A_EventManager.InvokePlaySFX("Chest Open");

        transform.position = pos;

        SpawnItem();
    }


    IEnumerator C_Shrink()
    {
        yield return new WaitForSeconds(2);

        float elapsed = 0;
        float dur = 1f;
        Vector2 scale = transform.localScale;

        while (elapsed < dur)
        {
            float c = A_Extensions.CosCurve(elapsed / dur);
            transform.localScale = Vector2.Lerp(scale, Vector2.zero, c);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}
