using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P_Cholla : Player
{
    [SerializeField] float _fr = 150;
    [SerializeField] float _frA = 100;
    [SerializeField] float _spd = 100;
    [SerializeField] float _spdA = 100;
    [SerializeField] Material _baseMat;
    [SerializeField] Material _glowMat;
    [SerializeField] SpriteRenderer _rend;

    [SerializeField] float _cooldown;
    [SerializeField] float _invDur;
    float _timer;
    public bool _invincible;
    float _invTimer;

    public bool AbilityButtonDown() => Input.GetKeyDown(A_InputManager.Instance.Key("Ability")) || A_InputManager.Instance.GamepadAbility();

    public override void Update()
    {
        base.Update();

        _rend.material = _invincible ? _glowMat : _baseMat;
        DefaultStats.FireRate = _invincible ? _frA : _fr;
        DefaultStats.Speed = _invincible ? _spdA : _spd;

        if (Stopped)
            return;

        if (_invincible)
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
            _invincible = true;
            _timer = 0;
            _invTimer = 0;
            A_EventManager.InvokeUseAbility();
            A_EventManager.InvokePlaySFX("Shield");
            Squash(0.3f);
            return;
        }

        A_EventManager.InvokePlaySFX("Error");
        if (V_HUDManager.Instance != null) V_HUDManager.Instance.AssignErrorHelpMessage("Not angry enough!");
    }

    void Active()
    {
        _invTimer += Time.deltaTime;
        float percent = Mathf.Clamp01(_invTimer / (_invDur + AbilityUpgrades * 2));
        AbilityPercent = 1 - percent;

        if (percent >= 1)
        {
            _invincible = false;
            _timer = 0;
            _invTimer = 0;
            return;
        }
    }
}
