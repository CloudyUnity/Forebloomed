using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunctionTester : MonoBehaviour
{
    void Update()
    {
        Vector2 pos = transform.position;
        pos.x += Time.deltaTime;
        pos.y = A_Extensions.FlatCurve(pos.x);
        transform.position = pos;
    }
}
