using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flasher : MonoBehaviour
{
    [SerializeField] float _farPoint;
    [SerializeField] float _dur;
    [SerializeField] Vector2 _cooldown;

    Vector2 _startPos;
    Vector2 _endPos;

    private void Start()
    {
        _startPos = new Vector2(_farPoint, _farPoint);
        _endPos = -_startPos;

        StartCoroutine(C_Flash());
    }

    IEnumerator C_Flash()
    {
        float elapsed = 0;

        while (elapsed < _dur)
        {
            float curved = A_Extensions.CosCurve(elapsed / _dur);   
            transform.localPosition = Vector2.Lerp(_startPos, _endPos, curved);
            elapsed += Time.deltaTime;
            yield return null;  
        }

        transform.localPosition = new Vector2(100, 100);
        yield return new WaitForSeconds(_cooldown.AsRange());

        StartCoroutine(C_Flash());
    }
}
