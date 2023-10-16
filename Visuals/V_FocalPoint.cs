using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class V_FocalPoint : MonoBehaviour
{
    public int Bias;
    private void Start()
    {
        A_EventManager.InvokeAddFocalPoint(this);
    }
}
