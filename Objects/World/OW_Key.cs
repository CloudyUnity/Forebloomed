using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OW_Key : ObjectWorld
{
    [SerializeField] Vector2 _arcSize;
    [SerializeField] Vector2 _collectTime;
    [SerializeField] bool _golden;
    [SerializeField] ParticleSystem _collectPS;

    protected override void Start()
    {
        base.Start();
        StartCoroutine(C_Grow());
    }

    IEnumerator C_Grow()
    {
        float elapsed = 0;
        float dur = 0.15f;
        Vector2 scale = transform.localScale;

        while (elapsed < dur)
        {
            transform.localScale = Vector2.Lerp(Vector2.zero, scale, elapsed / dur);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = scale;
    }

    private void Update()
    {
        if (Interact)
        {
            Selectable = false;
            StartCoroutine(C_BezierMove2());
        }
    }

    IEnumerator C_BezierMove2()
    {
        float elapsed = 0;
        Vector2 p0 = transform.position;
        float randArcSize = _arcSize.AsRange();
        float startSize = transform.localScale.x;
        int flip = Random.value > 0.5f ? -1 : 1;
        float dur = _collectTime.AsRange();

        float size = _golden ? 0.7f : 0.4f;

        while (elapsed < dur)
        {
            Vector2 p3 = Player.Instance.transform.position;

            Vector2 dir03 = (p3 - p0).normalized;
            Vector2 dir01 = Quaternion.Euler(0, 0, 90 * flip) * dir03;
            Vector2 p1 = p0 + dir01 * randArcSize;
            Vector2 p2 = p3 + dir01 * randArcSize;

            float curved = A_Extensions.CosCurve(elapsed / dur);

            transform.position = A_Extensions.BezierCube(p0, p1, p2, p3, curved);
            transform.localScale = Mathf.Lerp(startSize, size, curved) * Vector2.one;

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (_golden)
            Player.Instance.GainBigKey();
        else
            Player.Instance.GainKey();

        A_EventManager.InvokePlaySFX("Key");

        Instantiate(_collectPS, Player.Instance.transform.position, Quaternion.identity);

        if (Player.Instance.SoftStats.Keys + Player.Instance.SoftStats.GoldKeys >= 10)
            A_EventManager.InvokeUnlock("Keymaster");

        Destroy(gameObject);
    }
}
