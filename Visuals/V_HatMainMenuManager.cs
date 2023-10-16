using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class V_HatMainMenuManager : MonoBehaviour
{
    [SerializeField] GameObject _hatsParent;

    private void OnEnable()
    {
        A_EventManager.OnInitHatButtons += Init;
    }

    private void OnDisable()
    {
        A_EventManager.OnInitHatButtons -= Init;
    }

    void Init(List<int> list)
    {
        V_HatButton[] buttons = _hatsParent.GetComponentsInChildren<V_HatButton>();        
        foreach (var b in buttons)
        {
            if (list.Contains(b.HatIndex))
                b.SetEnabled();
        }
    }
}
