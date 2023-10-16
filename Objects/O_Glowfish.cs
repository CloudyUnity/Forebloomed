using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class O_Glowfish : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] LayerMask FishLayer;
    [SerializeField] float range;
    [SerializeField] float speedVariance;
    [SerializeField] float turningDamping;
    [SerializeField] float despawnDistance;
    [SerializeField] SpriteRenderer rend;
    [SerializeField] Material[] _colors;

    float noiseOffset;

    private void Start()
    {
        noiseOffset = Random.value * 10;
        rend.material = _colors.RandomItem();
    }

    Vector3 GetSeparationVector(Transform target)
    {
        Vector3 diff = transform.position - target.transform.position;
        float diffLen = diff.magnitude;
        float scaler = Mathf.Clamp01(1 - diffLen / range);
        return diff * (scaler / diffLen);
    }

    private void Update()
    {
        float noise = Mathf.PerlinNoise(Time.time, noiseOffset) * 2 - 1;
        float velocity = speed * (1 + noise * speedVariance);

        Vector3 seperation = Vector2.zero;
        Vector3 align = transform.right * turningDamping;
        Vector3 cohesion = transform.position;

        Collider2D[] nearbyBoids = Physics2D.OverlapCircleAll(transform.position, range, FishLayer);

        foreach (Collider2D hit in nearbyBoids)
        {
            if (hit.gameObject == gameObject)
                continue;

            Transform t = hit.transform;
            seperation += GetSeparationVector(t);
            align += t.right;
            cohesion += t.position;
        }

        align /= nearbyBoids.Length;

        Vector3 avgCohesion = cohesion / nearbyBoids.Length;
        cohesion = (avgCohesion - transform.position).normalized;

        transform.right = seperation + align + cohesion;
        transform.position += transform.right * velocity * Time.deltaTime;

        Vector3 basePos = Player.Instance != null ? Player.Instance.transform.position : Vector3.zero;

        if (Vector2.Distance(basePos, transform.position) >= despawnDistance)
        {
            A_EventManager.InvokeMakeFish();
            Destroy(gameObject);
        }

        if (V_HUDManager.Instance == null)
            return;

        Color newColor = rend.color;
        float perc = A_TimeManager.Instance.TimePercent;
        float curved = A_Extensions.CosCurve(perc - (1 - perc));
        newColor.a = Mathf.Lerp(0, 1, curved);
        rend.color = newColor;
    }
}
