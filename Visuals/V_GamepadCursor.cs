using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class V_GamepadCursor : MonoBehaviour
{
    public static Vector2 CursorPos;
    [SerializeField] float _dis = 2;
    [SerializeField] GameObject _sprite;
    Vector3 _lastUnitC = Vector2.one;

    public static Vector2 GetCursorPos()
    {
        if (CursorPos != null)
            return CursorPos;

        return Vector2.zero;
    }

    private void Start()
    {
        CursorPos = new Vector2(1, 0);
    }

    private void Update()
    {
        _sprite.SetActive(A_InputManager.GamepadMode);
        if (!A_InputManager.GamepadMode)
            return;

        Vector3 unitCircle = new Vector3(Input.GetAxis("HorizontalR"), Input.GetAxis("VerticalR"));
        if (unitCircle.magnitude > 0.1f)
            _lastUnitC = unitCircle.normalized * _dis;

        CursorPos = Player.Instance.transform.position + _lastUnitC;        

        _sprite.transform.position = CursorPos;
    }
}
