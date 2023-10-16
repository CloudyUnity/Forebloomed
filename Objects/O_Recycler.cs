using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class O_Recycler : MonoBehaviour
{
    [SerializeField] SpriteRenderer _itemRend;
    [SerializeField] SpriteRenderer _bgRend;
    [SerializeField] SpriteRenderer _rend;
    [SerializeField] ParticleSystem _ps;
    [SerializeField] Color _bgOffColor;
    [SerializeField] MakeInfo _makeInfo;
    bool _randomMode = false;
    public ItemData Item;
    KeyCode _interactKey;
    Vector2 _defaultScale;
    Color _color = Color.white;

    public static int AssignedItems = 0;

    public static List<ItemData> AvailableItems;

    bool _used;

    private void Start()
    {
        AssignedItems = 0;
        if (AvailableItems != null)
            AvailableItems.Clear();

        _interactKey = A_InputManager.Instance.Key("Interact");
        _defaultScale = transform.localScale;        
    }

    private void Update()
    {
        _color.a = 0.59f + Mathf.PingPong(Time.time * 0.2f, 0.2f);
        _itemRend.color = _color;
        _itemRend.sortingOrder = _rend.sortingOrder - 10;
        _bgRend.sortingOrder = _rend.sortingOrder - 20;

        if (_used)
            return;

        if (Item == null)
        {
            if (AvailableItems == null || AvailableItems.Count == 0 || AssignedItems == 0)
            {
                AvailableItems = new List<ItemData>(ItemManager.Instance.AllItems);
                AvailableItems.RemoveAll(x => x.Name.Is("Acorn Leaf", "Aiderbot", "Fuilberries", "Watermelon", "Grub", "Passion Flower", "Spare Pot"));
            }

            TryGetItem(out Item);
        }

        _interactKey = A_InputManager.Instance.Key("Interact");

        if (Player.Selected == gameObject && Input.GetKeyDown(_interactKey))
        {
            if (Item.IsFamiliar)
                A_EventManager.InvokeUnlock("Landlord");

            Item.OnRecycleItem();
            ItemManager.Instance.AllItems.Remove(Item);
            _makeInfo.Amount = (A_LevelManager.ItemCost(11, 13) * 0.5f + Random.Range(-3, 3)) * Vector2.one;
            A_Factory.Instance.MakeBasic(Player.Instance.transform.position, _makeInfo);
            A_EventManager.InvokePlaySFX("Machine");
            Player.Selected = null;
            _itemRend.sprite = null;
            _bgRend.color = _bgOffColor;

            if (ItemManager.Instance.AllItems.Count == 0)
                A_EventManager.InvokeUnlock("Bad Idea");

            _ps.Play();
            Squash(0.5f);
            _used = true;
        }
    }

    bool TryGetItem(out ItemData item)
    {
        item = null;
        if (_randomMode)
        {
            _itemRend.sprite = ItemHolder.Instance.GetRandomItem(transform.position.x * A_LevelManager.QuickSeed, out int _).Icon;
            _used = true;
            return false;
        }

        if (AssignedItems == AvailableItems.Count)
        {
            _itemRend.sprite = null;
            _bgRend.color = _bgOffColor;
            _used = true;
            return false;
        }

        item = AvailableItems.RandomItem(transform.position.x * A_LevelManager.QuickSeed + AssignedItems);
        AvailableItems.Remove(item);
        AssignedItems++;
        _itemRend.sprite = Item.Icon;
        return true;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (_used)
            return;

        if (Player.Selected != null)
            return;

        if (!collision.gameObject.CompareTag("Player"))
            return;

        Squash(0.2f);
        A_EventManager.InvokePlaySFX("Click");
        Player.Selected = gameObject;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (Player.Selected == gameObject && collision.gameObject.tag == "Player")
            Player.Selected = null;
    }

    bool _squashing;
    public void Squash(float dur) => StartCoroutine(C_SqaushStretch(dur));
    IEnumerator C_SqaushStretch(float dur)
    {
        if (_squashing || _defaultScale == Vector2.zero)
            yield break;

        _squashing = true;

        float elapsed = 0;

        while (elapsed < dur)
        {
            float humped = 1 - A_Extensions.HumpCurve(elapsed / dur, 0, 1);
            transform.localScale = _defaultScale + new Vector2(0.1f * -humped, 0.1f * humped);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = _defaultScale;
        _squashing = false;
    }
}
