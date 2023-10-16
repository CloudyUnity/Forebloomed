using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class A_OrbitalManager : MonoBehaviour
{
    public static A_OrbitalManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    Dictionary<int, List<GameObject>> OrbitalList = new Dictionary<int, List<GameObject>>();

    public void Add(int index, GameObject go)
    {
        if (!OrbitalList.ContainsKey(index))
            OrbitalList.Add(index, new List<GameObject>());

        OrbitalList[index].Add(go);
    }

    public int GetIndex(int index, GameObject go) => OrbitalList[index].IndexOf(go);

    public void RemoveNulls()
    {
        foreach (var list in OrbitalList.Values)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i] == null)
                    list.RemoveAt(i);
            }
        }
    }

    public void Remove(int index, GameObject go) => OrbitalList[index].Remove(go);

    public int Count(int index) => OrbitalList[index].Count;
}
