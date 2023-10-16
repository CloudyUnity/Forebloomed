using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class V_HatButton : MonoBehaviour
{
    public int HatIndex;
    public bool HatEnabled;
    [SerializeField] GameObject _cover;
    [SerializeField] GameObject _unlockCover;

    public void SetEnabled()
    {
        HatEnabled = true;
        _cover.SetActive(false);
    }

    public void ToggleHat()
    {
        if (_unlockCover.activeSelf)
            return;

        HatEnabled = !HatEnabled;
        _cover.SetActive(!HatEnabled);
        A_SaveMetaManager.instance.ToggleHat(HatIndex, HatEnabled);
    }
}
