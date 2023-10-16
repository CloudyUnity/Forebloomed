using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class A_HatManager : MonoBehaviour
{
    [SerializeField] List<GameObject> _hats;

    private void Start()
    {
        List<int> hats = A_SaveMetaManager.instance.Hats;

        foreach (int i in hats)
            _hats[i].SetActive(true);
    }
}
