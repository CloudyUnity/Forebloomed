using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ConstEffect", menuName = "Item/Extralife", order = 1)]
public class J_ExtraLife :ItemData
{
    [SerializeField] MakeInfo _info;
    [SerializeField] float _offset;

    public override void OnLoadItem()
    {
        Vector3 offset = new Vector3(_offset.AsRange(), _offset.AsRange());
        A_Factory.Instance.MakeBasic(Player.Instance.transform.position + offset, _info);
    }

    public override void OnPickup()
    {
        Vector3 offset = new Vector3(_offset.AsRange(), _offset.AsRange());
        A_Factory.Instance.MakeBasic(Player.Instance.transform.position + offset, _info);

        A_LevelManager.Instance.ExtraLives++;
    }
}
