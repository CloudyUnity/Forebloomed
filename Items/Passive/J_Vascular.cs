using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Vascular", menuName = "Item/Vascular", order = 1)]
public class J_Vascular : ItemData
{
    [SerializeField] GameObject _invisiStump;
    OW_Portal _stump;

    public override void OnPickup()
    {
        GameObject go = Instantiate(_invisiStump, Vector3.zero, Quaternion.identity);
        _stump = go.GetComponent<OW_Portal>();
    }

    public override void OnLoadItem()
    {
        GameObject go = Instantiate(_invisiStump, Vector3.zero, Quaternion.identity);
        _stump = go.GetComponent<OW_Portal>();
    }

    public override void OnStumpSpawn(OW_Portal ow)
    {
        if (_stump != null)
            ow.SetOther(_stump);
    }
}
