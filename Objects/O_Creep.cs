using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class O_Creep : MonoBehaviour
{
    [SerializeField] float _lifetime;
    [SerializeField] float _inflictCooldown;
    [SerializeField] List<StatusEffect> _effect;
    Entity _eCache = null;
    float _timer;
    float _timer2;

    private void Start()
    {
        StartCoroutine(C_Grow());
        _timer2 = 999;
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        _timer2 += Time.deltaTime;

        if (_timer >= _lifetime)
        {
            StartCoroutine(C_Dissipate());
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (_eCache != null && collision.gameObject == _eCache.gameObject)
        {
            if (_eCache.Flying)
                return;

            _eCache.InflictStatus(_effect);
            _timer2 = 0;
            return;
        }

        if (collision.tag == "Enemy" && _timer2 >= _inflictCooldown)
        {
            _eCache = collision.GetComponent<Entity>();
            if (_eCache.Flying)
                return;

            _eCache.InflictStatus(_effect);
            _timer2 = 0;
        }
    }

    IEnumerator C_Grow()
    {
        float elapsed = 0;
        float dur = 0.2f;
        Vector2 end = transform.localScale + 0.5f.AsRange() * Vector3.one;

        while (elapsed < dur)
        {
            float curved = A_Extensions.CosCurve(elapsed / dur);
            transform.localScale = Vector2.Lerp(Vector2.zero, end, curved);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = end;
    }

    IEnumerator C_Dissipate()
    {
        float elapsed = 0;
        float dur = 1f;
        Vector2 start = transform.localScale;

        while (elapsed < dur)
        {
            float curved = A_Extensions.CosCurve(elapsed / dur);
            transform.localScale = Vector2.Lerp(start, Vector2.zero, curved);
            elapsed += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }
}
