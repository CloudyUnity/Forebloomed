using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class O_PortalPart : MonoBehaviour
{
    [SerializeField] float _stretchiness;
    [SerializeField] GameObject _center;
    [SerializeField] bool _stretchMultiplyOn;

    Vector3 _scale;

    private void Start()
    {
        _scale = transform.localScale;

        if (!_stretchMultiplyOn)
            return;

        _stretchiness *= 20 / Vector3.Distance(_center.transform.position, new Vector3(0, -13));
    }

    private void Update()
    {
        if (Player.Instance == null)
            return;

        transform.localScale = _scale + Vector3.one * 0.01f * A_Extensions.CosCurve(Time.time * _stretchiness * 10);

        Vector3 dir = (Player.Instance.transform.position - _center.transform.position).normalized;
        transform.position = _center.transform.position + dir * _stretchiness;
    }
}
