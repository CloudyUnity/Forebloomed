using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemPriced : MonoBehaviour
{
    public ShopItem ShopData;
    public bool CanUseKey;

    [SerializeField] TextPair _costTXT;
    [SerializeField] TextPair _nameTXT;
    [SerializeField] TextPair _descTXT;
    [SerializeField] TextPair _quipTXT;
    [SerializeField] GameObject _infoCard;
    [SerializeField] SpriteRenderer _rend;
    [SerializeField] Material _baseMat;
    [SerializeField] Material _glowMat;
    [SerializeField] SpriteRenderer _infoCardIcon;
    [SerializeField] SinWave _infoWave;
    [SerializeField] SpriteRenderer _iconBG;
    [SerializeField] Color _discountColor;

    bool _finished;

    bool _selected { get { return Player.Selected == gameObject; } }

    private void Start()
    {
        _sqaushTimer = Random.Range(0, 1.5f);
    }

    public void UpdateCost(int newCost, bool isSale)
    {
       ShopData.Cost = newCost;
       _costTXT.text = newCost.ToString();

        if (!isSale)
            return;

        _costTXT.color = _discountColor;
    }

    private void Update()
    {
        if (ShopData.Cost != 0 && !_finished)
        {
            _costTXT.text = ShopData.Cost.ToString();
            _rend.sprite = ShopData.Item.Icon;
            _infoCardIcon.sprite = ShopData.Item.Icon;
            _nameTXT.text = ShopData.Item.Name;
            _descTXT.text = ShopData.Item.GetDescription();
            _quipTXT.text = ShopData.Item.Quip;
            _iconBG.color = ShopData.Item.Colors[0];
            _iconBG.SetAlpha(0.8f);

            A_EventManager.InvokePricedItemSpawn(this);
            _finished = true;
        }

        _rend.material = _selected ? _glowMat : _baseMat;
        float targetSize = _selected ? 0.7f : 0.6f;
        transform.localScale = Vector2.Lerp(transform.localScale, Vector2.one * targetSize, Time.deltaTime * 5);

        Vector2 cardSize = _selected ? Vector2.one * 3 : Vector2.zero;
        _infoCard.transform.localScale = Vector2.Lerp(_infoCard.transform.localScale, cardSize, Time.deltaTime * 7);
        _infoCard.transform.localPosition = new Vector2(0, A_Extensions.GetSinWave(_infoWave, Time.time));

        _sqaushTimer += Time.deltaTime;
        if (_sqaushTimer > 1.5f)
        {
            Squash(0.4f);
            _sqaushTimer = 0;
        }

        if (!_selected || V_HUDManager.Instance.IsPaused)
            return;

        if (!Input.GetKeyDown(A_InputManager.Instance.Key("Interact")))
            return;

        if (Player.Instance.SoftStats.GoldAmount < ShopData.Cost)
        {
            if (CanUseKey && Player.Instance.SoftStats.Keys > 0)
            {
                PickUpItemWithKey();
                A_EventManager.InvokePlaySFX("Item Buy");
                return;
            }

            A_EventManager.InvokePlaySFX("Error");
            if (V_HUDManager.Instance != null) V_HUDManager.Instance.AssignErrorHelpMessage("Not enough gold!");
            return;
        }

        PickUpItem();
        A_EventManager.InvokePlaySFX("Item Buy");
    }

    float _sqaushTimer;
    public void Squash(float dur) => StartCoroutine(C_SqaushStretch(dur));
    IEnumerator C_SqaushStretch(float dur)
    {
        float elapsed = 0;
        Vector2 start = _rend.transform.localScale;

        while (elapsed < dur)
        {
            float humped = 1 - A_Extensions.HumpCurve(elapsed / dur, 0, 1);
            _rend.transform.localScale = start + new Vector2(0.1f * -humped, 0.1f * humped);
            elapsed += Time.deltaTime;
            yield return null;
        }
        _rend.transform.localScale = start;
    }

    void PickUpItem()
    {
        Player.Instance.SoftStats.GoldAmount -= ShopData.Cost;
        A_EventManager.InvokeSpendGold(ShopData.Cost);

        _sqaushTimer = -999;

        int newCount = ShopData.Count + 1;
        int cost = A_LevelManager.ItemCost(11 + newCount * 2, 14 + newCount * 2, transform.position.x);
        cost = Mathf.RoundToInt(cost * (1 + (0.5f * newCount)));

        if (newCount <= 3)
            A_EventManager.InvokeRestock(transform.position, cost, newCount);
        else
            A_LevelManager.Instance.BoughtOutItems++;

        GameObject go = A_Factory.Instance.TurnToItem(new Vector2(400, 400), ShopData.Item, 0);
        var item = go.GetComponent<Item>();
        item.DuplicatedItem = true;
        item.OnPickup();       

        _infoCard.SetActive(false);

        if (A_LevelManager.Instance.BoughtOutItems >= 4)
            A_EventManager.InvokeUnlock("Capitalist");

        Player.Selected = null;
        Destroy(gameObject);
    }

    void PickUpItemWithKey()
    {
        Player.Instance.UseKey();

        int newCount = ShopData.Count + 1;
        if (newCount <= 3)
            A_EventManager.InvokeRestock(transform.position, (int)(ShopData.Cost * 1.5f), newCount);
        else
            A_LevelManager.Instance.BoughtOutItems++;

        GameObject go = A_Factory.Instance.TurnToItem(new Vector2(400, 400), ShopData.Item, 0);
        go.GetComponent<Item>().OnPickup();

        _infoCard.SetActive(false);

        if (A_LevelManager.Instance.BoughtOutItems >= 4)
            A_EventManager.InvokeUnlock("Capitalist");

        Player.Selected = null;
        Destroy(gameObject);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (Player.Selected != null || collision.gameObject.tag != "Player")
            return;

        Player.Selected = gameObject;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (_selected && collision.gameObject.tag == "Player")
            Player.Selected = null;
    }
}
