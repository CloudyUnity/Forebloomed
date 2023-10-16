using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemInvIcon : MonoBehaviour
{
    [SerializeField] Image _img;
    [SerializeField] TextPair _countTxt;
    int count = 1;
    public ItemData Data;
    public void Init(ItemData data)
    {
        Data = data;
        LoadInfo();

        V_UIManager.Instance.AddButton(gameObject);
    }

    public void AddCount()
    {
        count++;
        LoadInfo();
    }

    public void LoadInfo()
    {
        _img.sprite = Data.Icon;
        _countTxt.text = "x" + count;
    }

    public void Update()
    {
        if (V_UIManager.Instance.IsHovered(gameObject))
        {
            if (V_ItemInvManager.Hovered == null)
                V_ItemInvManager.Hovered = this;
        }
        else if (V_ItemInvManager.Hovered == this)
            V_ItemInvManager.Hovered = null;
    }
}
