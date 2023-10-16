using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class V_MenuFloaterManager : MonoBehaviour
{
    [SerializeField] GameObject _floater;
    [SerializeField] int _count;
    [SerializeField] Sprite[] _sprites;
    [SerializeField] Vector2 _speedRange;
    [SerializeField] Vector2 _sinAmpRange;
    [SerializeField] Vector2 _sinFreqRange;

    [SerializeField] float _endX;
    [SerializeField] float _height;

    private void Start()
    {
        for (int i = 0; i < _count; i++)
        {
            Vector2 randPos = new Vector2(_endX.AsRange(), _height.AsRange());

            GameObject go = Instantiate(_floater, randPos, Quaternion.identity);
            go.GetComponent<SpriteRenderer>().sprite = _sprites[Random.Range(0, _sprites.Length)];

            StartCoroutine(C_MoveFloater(go));
        }
    }

    IEnumerator C_MoveFloater(GameObject floater)
    {
        float speed = _speedRange.AsRange();
        float amp = _sinAmpRange.AsRange();
        float freq = _sinFreqRange.AsRange();

        while (floater.transform.position.x < _endX)
        {
            Vector2 newPos = floater.transform.position;
            newPos.x += speed;
            newPos.y += amp * Mathf.Sin(2 * Mathf.PI / freq * Time.time);
            floater.transform.position = newPos;
            yield return null;
        }

        yield return new WaitForSeconds(Random.Range(0, 1));

        floater.transform.position = new Vector2(-_endX, _height.AsRange());
        StartCoroutine(C_MoveFloater(floater));
    }
}
