using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Soft", menuName = "Item/Soft", order = 1)]
public class J_SoftPerFloor : ItemData
{
    [SerializeField] MakeInfo _info;
    public override void OnLoadItem()
    {
        if (A_LevelManager.Instance.CurrentLevel.IsEven())
            return;

        Player.Instance.StartCoroutine(C_Wait());
    }

    IEnumerator C_Wait()
    {
        yield return new WaitForSecondsRealtime(0.5f);

        if (Player.Instance != null)
        {
            Player.Instance.AddSoftStats(new PlayerSoftStats { BonusHealth = 1 });
            yield break;
        }

        A_Factory.Instance.MakeBasic(new Vector3(0, -2f), _info);
    }
}
