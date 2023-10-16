using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P_Springleaf : Player
{
    [SerializeField] float _cooldown;
    [SerializeField] float _overDur;
    float _timer;
    public static bool _overclocked;
    float _overTimer;

    public bool AbilityButtonDown() => Input.GetKeyDown(A_InputManager.Instance.Key("Ability")) || A_InputManager.Instance.GamepadAbility();
    public override void Update()
    {
        base.Update();

        if (Stopped)
            return;

        DefaultStats.FireRate = _overclocked ? 30 : 70;

        if (_overclocked)
        {
            Active();
            return;
        }

        InActive();
    }

    void InActive()
    {
        _timer += Time.deltaTime;
        float percent = Mathf.Clamp01(_timer / _cooldown);
        AbilityPercent = percent;

        if (!AbilityButtonDown())
            return;

        if (percent >= 0.95f)
        {
            _overclocked = true;
            _timer = 0;
            _overTimer = 0;
            A_EventManager.InvokeUseAbility();
            A_EventManager.InvokePlaySFX("Overclock");
            Squash(0.2f);
            return;
        }

        A_EventManager.InvokePlaySFX("Error");
        if (V_HUDManager.Instance != null) V_HUDManager.Instance.AssignErrorHelpMessage("You don't want to break them!");
    }

    void Active()
    {
        _overTimer += Time.deltaTime;
        float percent = Mathf.Clamp01(_overTimer / (_overDur + AbilityUpgrades * 2));
        AbilityPercent = 1 - percent;

        if (percent >= 1)
        {
            _overclocked = false;
            _timer = 0;
            _overTimer = 0;
            return;
        }
    }
}
