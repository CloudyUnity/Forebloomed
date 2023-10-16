using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class H_Blocker : MonoBehaviour, ITakeDamage
{
    [SerializeField] float HitPoints;
    [SerializeField] Sprite _mainSprite;
    [SerializeField] Sprite _whiteSprite;
    [SerializeField] SpriteRenderer _rend;
    [SerializeField] ParticleSystem _ps;
    [SerializeField] bool _randSize;
    [SerializeField] Vector2 _scaleRange;
    bool _flashing;

    void Start()
    {
        if (_randSize)
            transform.localScale = Vector2.one * _scaleRange.AsRange();
    }

    public void TakeDamage(float dmg)
    {
        HitPoints -= dmg;
        Squash(0.1f);
        A_EventManager.InvokePlaySFX("Crack");
        StartCoroutine(C_Flash());

        if (HitPoints <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Instantiate(_ps, transform.position, Quaternion.identity);
        
        Destroy(gameObject);
    }


    IEnumerator C_Flash()
    {
        if (_flashing)
            yield break;

        _flashing = true;

        _rend.sprite = _whiteSprite;
        yield return new WaitForSeconds(0.3f);
        _rend.sprite = _mainSprite;

        _flashing = false;
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
