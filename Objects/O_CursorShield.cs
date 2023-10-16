using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class O_CursorShield : MonoBehaviour
{
    [SerializeField] float _radius;
    [SerializeField] float _speed;

    private void Update()
    {
        Vector2 targetPos = Player.Instance.HitBox + (GetDir() * _radius);
        transform.right = (Vector2)GetDir();
        transform.position = Vector2.Lerp(transform.position, targetPos, Time.deltaTime * _speed);
    }

    Vector3 GetDir()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.nearClipPlane;
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(mousePos);
        return (worldPosition - (Vector2)Player.Instance.transform.position).normalized;
    }
}
