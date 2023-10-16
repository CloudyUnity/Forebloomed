using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletMortar : MonoBehaviour
{
    [SerializeField] float _speed;
    [SerializeField] float _lifetime;
    [SerializeField] float _dropDur;
    [SerializeField] float _chaseWithin;
    float _timer;

    private void Update()
    {
        if (Vector2.Distance(Player.Instance.transform.position, transform.position) < _chaseWithin)
        {
            _timer += Time.deltaTime;
        }

        if (_timer < _lifetime)
        {
            Vector3 dir = (Player.Instance.transform.position - transform.position).normalized;
            transform.position += dir * _speed * Time.deltaTime;
            return;
        }

        if (_timer < _lifetime + _dropDur)
        {
            // Drop bomba
            return;
        }

        Destroy(gameObject);
    }
}
