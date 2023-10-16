using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OW_Crystal : ObjectWorld, ITakeDamage
{
    [SerializeField] MakeInfo _info;
    [SerializeField] float HP;

    [SerializeField] Sprite _main, _hurt, _dead;
    [SerializeField] SpriteRenderer _rend;
    [SerializeField] ParticleSystem _ps;

    [SerializeField] BoxCollider2D _col;

    Vector2 NormalSize;

    protected override void Start()
    {
        base.Start();

        NormalSize = transform.localScale;
        HP *= A_LevelManager.Instance.DifficultyModifier;
    }

    public void TakeDamage(float dmg)
    {
        if (dmg <= 0 || HP <= 0)
            return;

        HP -= dmg;

        if (HP <= 0)
        {
            Die();
            Squash(0.3f);
            return;
        }

        Squash(0.1f);
        StartCoroutine(C_Flash());
        A_EventManager.InvokePlaySFX("CrystalHit");
    }

    static bool _carmineTrig;
    void Die()
    {
        A_Factory.Instance.MakeBasic(transform.position, _info);
        _rend.sprite = _dead;
        _col.enabled = false;
        Instantiate(_ps, transform.position, Quaternion.identity);
        A_EventManager.InvokePlaySFX("CrystalBreak");
        if (_carmineTrig)
            return;
        A_EventManager.InvokeUnlock("Carmine");
        _carmineTrig = true;
    }

    IEnumerator C_Flash()
    {
        _rend.sprite = _hurt;
        yield return new WaitForSeconds(0.1f);

        if (HP <= 0)
            yield break;

        _rend.sprite = _main;
    }

    bool _squashing;
    public void Squash(float dur) => StartCoroutine(C_SqaushStretch(dur));
    IEnumerator C_SqaushStretch(float dur)
    {
        if (_squashing || NormalSize == Vector2.zero)
            yield break;

        _squashing = true;

        float elapsed = 0;

        while (elapsed < dur)
        {
            float humped = 1 - A_Extensions.HumpCurve(elapsed / dur, 0, 1);
            transform.localScale = NormalSize + new Vector2(0.1f * -humped, 0.1f * humped);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = NormalSize;
        _squashing = false;
    }
}
