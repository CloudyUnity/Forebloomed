using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Spawner", menuName = "Item/Spawner", order = 1)]
public class J_Spawner : ItemData
{
    [SerializeField] MakeInfo _info;
    [SerializeField] float _offset;
    [SerializeField] bool _altActive;
    [SerializeField] MakeInfo _alt;
    [SerializeField] float _altChance;

    static Dictionary<string, int> CountDict = new Dictionary<string, int>();

    static bool DictReset;

    public override void OnLoadItem()
    {
        DictReset = false;
        MakeObject();
    }

    public override void OnPickup()
    {
        MakeObject();        
    }

    public void MakeObject()
    {
        if (!CountDict.ContainsKey(_info.Name))
            CountDict.Add(_info.Name, 0);

        Vector3 offset = new Vector3(_offset.AsRange(), _offset.AsRange());
        CountDict[_info.Name]++;

        Debug.Log(_info.Name + " " + CountDict[_info.Name]);

        if (_altActive && A_Extensions.RandomChance(_altChance, A_LevelManager.Instance.Seed * CountDict[_info.Name]))
        {
            A_Factory.Instance.MakeBasic(Player.Instance.transform.position + offset, _alt);
            return;
        }

        A_Factory.Instance.MakeBasic(Player.Instance.transform.position + offset, _info);
    }

    public override void OnNewScene()
    {
        Debug.Log("Resetting spawner dictionary");

        if (DictReset)
            return;

        var keys = new List<string>(CountDict.Keys);
        foreach (string key in keys)
        {
            CountDict[key] = 0;
        }
        DictReset = true;
    }
}
