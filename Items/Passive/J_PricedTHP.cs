using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "THP", menuName = "Item/THP", order = 1)]
public class J_PricedTHP : ItemData
{
    [SerializeField] GameObject _pricedTHP;
    public override void OnPricedHeartSpawn(O_PricedHeart pheart)
    {
        if (Random.Range(0, 100f) > 25)
            return;

        Instantiate(_pricedTHP, pheart.transform.position, Quaternion.identity);

        Destroy(pheart.gameObject);
    }
}
