using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class V_ItemInvManager : MonoBehaviour
{
    [SerializeField] GameObject _item;
    [SerializeField] Transform _itemsParent;
    [SerializeField] TextPair _name;
    [SerializeField] TextPair _desc;
    [SerializeField] TextPair _quip;

    public static ItemInvIcon Hovered;

    public void LoadItems()
    {
        _itemsParent.DestroyChildren();

        List<ItemData> items = ItemManager.Instance.AllItems;
        Dictionary<string, ItemInvIcon> dict = new Dictionary<string, ItemInvIcon>();

        int c = 0;
        for (int i = 0; i < items.Count; i++)
        {
            if (dict.ContainsKey(items[i].Name))
            {
                dict[items[i].Name].AddCount();
                continue;
            }

            int row = Mathf.FloorToInt(c / 11f);
            GameObject go = Instantiate(_item, _itemsParent);
            go.transform.localPosition = new Vector2(-375 + (c - 11 * row) * 75, 365 - row * 105);
            ItemInvIcon icon = go.GetComponent<ItemInvIcon>();
            icon.Init(items[i]);
            dict.Add(items[i].Name, icon);
            c++;
        }
    }

    public void Update()
    {
        if (Hovered == null)
        {
            _name.text = "Item Inventory";
            _desc.text = "Hover on item to view!";
            _quip.text = "";
            return;
        }

        _name.text = Hovered.Data.Name;
        _desc.text = Hovered.Data.GetDescription();
        _quip.text = Hovered.Data.Quip;
    }
}
