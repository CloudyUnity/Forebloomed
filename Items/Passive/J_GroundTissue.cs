using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DoubleHeal", menuName = "Item/DoubleHeal", order = 1)]
public class J_GroundTissue : ItemData
{
    [SerializeField] float _chance;
    public override void OnGetCollectable(O_Collectable coll)
    {
        if (Random.Range(0, 100f) > _chance)
            return;

        if (coll.Name == "Heart")
        {
            Player.Instance.AddSoftStats(new PlayerSoftStats { CurHealth = 1 });
        }
    }
}
