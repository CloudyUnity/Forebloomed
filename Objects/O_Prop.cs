using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class O_Prop : MonoBehaviour
{
    [SerializeField] float _hitpoints;
    [SerializeField] bool _randomAngle;
    [SerializeField] BoxCollider2D _col;
    [SerializeField] ParticleSystem[] _ps;
    [SerializeField] SpriteRenderer[] _rend;
    [SerializeField] Sprite[] _deadSprite;
    [SerializeField] Sprite[] _whiteSprite;
    bool _dead;

    private void OnEnable()
    {
        if (_dead)
            Destroy(gameObject);
    }

    private void Start()
    {
        if (_randomAngle)
        {
            transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
        }
    }

    void Die()
    {
        _col.enabled = false;

        foreach (var p in _ps)
            p.Play();

        A_EventManager.InvokePlaySFX("Crack");
        if (_rend.Length > 0)
        {
            StartCoroutine(C_Flash());
        }

        Squash(0.2f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Die();
    }

    IEnumerator C_Flash()
    {
        _dead = true;

        for (int i = 0; i < _rend.Length; i++)
        {
            _rend[i].sprite = _whiteSprite[i];
        }

        yield return new WaitForSeconds(0.1f);

        for (int i = 0; i < _rend.Length; i++)
        {
            _rend[i].sprite = _deadSprite[i];
        }
    }

    bool _squashing;
    public void Squash(float dur) => StartCoroutine(C_SqaushStretch(dur));
    IEnumerator C_SqaushStretch(float dur)
    {
        if (_squashing)
            yield break;
        _squashing = true;

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
        _squashing = false;
    }
}
