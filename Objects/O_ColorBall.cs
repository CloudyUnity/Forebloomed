using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class O_ColorBall : MonoBehaviour
{
    [SerializeField] float _collectTime;
    [SerializeField] Vector2 arcSize;

    private void Start()
    {
        StartCoroutine(C_BezierMove2());
    }

    IEnumerator C_BezierMove2()
    {
        float elapsed = 0;
        Vector2 p0 = transform.position;
        float randArcSize = arcSize.AsRange();
        float startSize = transform.localScale.x;
        int flip = Random.value > 0.5f ? -1 : 1;
        float dur = _collectTime.AsRange();

        while (elapsed < dur)
        {
            Vector2 p3 = Player.Instance.transform.position;

            Vector2 dir03 = (p3 - p0).normalized;
            Vector2 dir01 = Quaternion.Euler(0, 0, 90 * flip) * dir03;
            Vector2 p1 = p0 + dir01 * randArcSize;
            Vector2 p2 = p3 + dir01 * randArcSize;

            float curved = A_Extensions.CosCurve(elapsed / dur);

            transform.position = A_Extensions.BezierCube(p0, p1, p2, p3, curved);
            curved = A_Extensions.HumpCurve(curved, startSize * 2, startSize);
            transform.localScale = curved * Vector2.one;

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
