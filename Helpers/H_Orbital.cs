using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class H_Orbital : MonoBehaviour
{
    [SerializeField] float _radius;
    [SerializeField] float _speed;
    [SerializeField] int _typeIndex;

    private void Start()
    {
        A_OrbitalManager.Instance.Add(_typeIndex, gameObject);
    }

    private void Update()
    {
        float angleByIndex = 360 / A_OrbitalManager.Instance.Count(_typeIndex) * A_OrbitalManager.Instance.GetIndex(_typeIndex, gameObject);

        float angle = Mathf.Repeat(angleByIndex + (Time.time * _speed), 360);
        Vector3 dir = Quaternion.Euler(0, 0, angle) * Vector2.right;

        transform.position = Player.Instance.HitBox + (dir.normalized * _radius);
    }

    private void OnDestroy()
    {
        A_OrbitalManager.Instance.Remove(_typeIndex, gameObject);
    }
}
