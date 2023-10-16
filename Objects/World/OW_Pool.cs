using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OW_Pool : ObjectWorld
{
    [SerializeField] Animator _anim;
    [SerializeField] SpriteRenderer _rend;
    [SerializeField] Material _noGlow;
    [SerializeField] Material _yesGlow;
    [SerializeField] CircleCollider2D _col;
    [SerializeField] ParticleSystem _burst, _spatter;
    bool _drained;

    static bool _hurtTrig, _offerTrig;
    private void Update()
    {
        bool enlarge = Player.Selected == gameObject && !_drained;

        Vector2 targetSize = enlarge ? 1.05f * Vector2.one : Vector2.one;
        transform.localScale = Vector2.Lerp(transform.localScale, targetSize, Time.deltaTime * 5);

        _rend.material = enlarge ? _yesGlow : _noGlow;

        if (Interact && !_drained)
        {
            if (Player.Instance.CharacterIndex != 7 && Player.Instance.SoftStats.MaxHealth < 1)
            {
                A_EventManager.InvokePlaySFX("Error");
                if (V_HUDManager.Instance != null) 
                    V_HUDManager.Instance.AssignErrorHelpMessage("No health to sacrifice...");
                return;
            }

            if (Player.Instance.CharacterIndex != 7 && Player.Instance.SoftStats.MaxHealth == 1 && Player.Instance.SoftStats.BonusHealth == 0 && !_offerTrig)
            {
                A_EventManager.InvokeUnlock("Offering");
                _offerTrig = true;
            }

            if (Player.Instance.CharacterIndex == 7)
                Player.Instance.TakeDamage(1, transform.position, "Blood Pool");
            else
                Player.Instance.SoftStats.MaxHealth--;

            _anim.SetTrigger("Drain");
            StartCoroutine(C_Drain());
            A_EventManager.InvokePlaySFX("Blood1");
            A_EventManager.InvokePlaySFX("Blood2");
            if (!_hurtTrig)
                A_EventManager.InvokeUnlock("That Hurt!");
            _hurtTrig = true;

            _col.enabled = false;
            _drained = true;
            _burst.Play();
            var module = _spatter.emission;
            module.rateOverTime = 0;

            Player.Instance.CheckHealth();
        }
    }

    IEnumerator C_Drain()
    {
        yield return new WaitForSeconds(1.2f);
        A_Factory.Instance.MakeItem(transform.position.x * A_LevelManager.QuickSeed, transform.position, ItemPool.Blood);
    }
}
