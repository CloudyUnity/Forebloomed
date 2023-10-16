using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P_Alocasia : Player
{
    [SerializeField] float _dur;
    [SerializeField] ParticleSystem _heartPS;
    float _timer;

    public bool AbilityButtonDown() => Input.GetKey(A_InputManager.Instance.Key("Ability")) || A_InputManager.Instance.GamepadAbility();
    public override void TakeDamage(int amount, Vector2 from, string name)
    {
        base.TakeDamage(amount, from, name);
        _timer = 0;
    }

    public override void Update()
    {
        base.Update();

        if (Stopped || A_LevelManager.Instance.CurrentLevel.IsEven())
        {
            _timer = 0;
            return;
        }

        if (AbilityButtonDown())
        {
            _timer += Time.deltaTime * (AbilityUpgrades + 1);
        }
        else
            _timer = 0;

        if (Input.GetKey(A_InputManager.Instance.Key("Shoot")) || A_InputManager.Instance.GamepadShoot())
            _timer = 0;

        UsingAbility = _timer > 0;

        float percent = Mathf.Clamp01(_timer / _dur);
        AbilityPercent = percent;

        var module = _heartPS.emission;
        module.rateOverTime = Mathf.Lerp(0, 9, percent);

        if (percent >= 1)
        {
            SoftStats.CurHealth += 1;
            _timer = 0;
            A_EventManager.InvokePlaySFX("Heal");
            A_EventManager.InvokeUseAbility();
            Squash(0.2f);
        }
    }
}
