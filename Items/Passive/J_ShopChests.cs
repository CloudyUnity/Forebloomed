using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopChests", menuName = "Item/ShopChests", order = 1)]
public class J_ShopChests : ItemData
{
    [SerializeField] MakeInfo _info;
    public override void OnLoadItem()
    {
        if (A_LevelManager.Instance != null && A_LevelManager.Instance.CurrentLevel.IsEven())
        {
            Vector2 pos = new Vector2(Random.Range(3, 6), Random.Range(-1, 2));
            A_Factory.Instance.MakeBasic(pos, _info);
        }
    }
}
