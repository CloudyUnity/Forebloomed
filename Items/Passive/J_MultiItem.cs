using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MultiItem", menuName = "Item/MultiItem", order = 1)]
public class J_MultiItem : ItemData
{
    [SerializeField] float _duplicateChance;
    [SerializeField] float _displacement;
    public override void OnCollectItem(Item item)
    {
        if (item.DuplicatedItem || item.Data.Name == "Soul of the Tree")
            return;

        if (Random.Range(0, 100f) < _duplicateChance + Player.Instance.CurStats.Luck)
        {
            GameObject go = A_Factory.Instance.TurnToItem(item.transform.position, item.Data, 1);
            go.GetComponent<Item>().DuplicatedItem = true;
            item.DuplicatedItem = true;

            A_EventManager.InvokeUnlock("Magician");
            A_EventManager.InvokePlaySFX("Duplicate");
        }
    }
}
