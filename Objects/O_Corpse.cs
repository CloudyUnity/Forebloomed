using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class O_Corpse : MonoBehaviour
{
    Sprite _corpseSprite;
    [SerializeField] SpriteRenderer _rend;

    private void Start()
    {
        Squash(0.5f);
    }

    public void AssignSprites(Sprite white, Sprite corpse, Vector2 size, bool flipX)
    {
        transform.localScale = size;
        _corpseSprite = corpse;
        _rend.sprite = white;
        if (flipX) _rend.flipX = true;
        StartCoroutine(C_Fling());
    }

    float _timer;
    [SerializeField] float _whiteLength;
    private void Update()
    {
        if (_timer == -1)
            return;

        _timer += Time.deltaTime;
        if (_timer > _whiteLength)
        {
            _rend.sprite = _corpseSprite;
            _timer = -1;
        }
    }

    IEnumerator C_Fling()
    {
        float elapsed = 0;
        float dur = Random.Range(1, 2);

        Vector2 startPos = transform.position;
        Vector2 offset = new Vector2(Random.Range(0.5f, 1.5f), Random.Range(0.5f, 1.5f));
        Vector2 dir = (Player.Instance.transform.position - transform.position).normalized;
        Vector2 endPos = startPos - dir * offset;

        while (elapsed < dur)
        {
            float curved = A_Extensions.SlowingCurve(elapsed / dur);
            transform.position = Vector2.Lerp(startPos, endPos, curved);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    public void Squash(float dur) => StartCoroutine(C_SqaushStretch(dur));
    IEnumerator C_SqaushStretch(float dur)
    {
        float elapsed = 0;
        Vector2 start = transform.localScale;

        while (elapsed < dur)
        {
            float humped = 1 - A_Extensions.HumpCurve(elapsed / dur, 0, 1);
            transform.localScale = start + new Vector2(0.1f * -humped, 0.1f * humped);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = start;
    }
}
