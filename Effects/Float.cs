using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Float : MonoBehaviour
{
    Vector2 _startPos;
    [SerializeField] float _amplitude;
    [SerializeField] float frequency;
    float _offset;

    private void Start()
    {
        _startPos = transform.localPosition;
        _offset = Random.value * frequency;
    }

    private void Update()
    {
        Vector2 newPos = _startPos;
        newPos.y = _startPos.y + _amplitude * Mathf.Sin(2 * Mathf.PI / frequency * (Time.time + _offset));
        transform.localPosition = newPos;
    }
}
