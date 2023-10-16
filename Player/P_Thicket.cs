using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P_Thicket : Player
{
    [SerializeField] float _cooldown;
    [SerializeField] LayerMask _bulletLayer;
    [SerializeField] GameObject _blankPS;
    float _timer;

    public bool AbilityButtonDown() => Input.GetKeyDown(A_InputManager.Instance.Key("Ability")) || A_InputManager.Instance.GamepadAbility();

    public override void Update()
    {
        base.Update();

        if (Stopped)
            return;

        _timer += Time.deltaTime;
        float percent = Mathf.Clamp01(_timer / _cooldown);
        AbilityPercent = percent;

        if (!AbilityButtonDown())
            return;

        if (percent >= 0.95f)
        {
            A_EventManager.InvokeUseAbility();
            Blank();
            _timer = 0;
            return;
        }

        if (_timer < 0.2f)
            return;

        A_EventManager.InvokePlaySFX("Error");
        if (V_HUDManager.Instance != null) 
            V_HUDManager.Instance.AssignErrorHelpMessage("Ability doesn't look ready!");
    }

    void Blank()
    {
        GameObject go = Instantiate(_blankPS, HitBox, Quaternion.identity);
        go.transform.localScale *= (CurStats.MagnetRange + AbilityUpgrades) * 5f;

        Squash(0.1f);
        A_EventManager.InvokePlaySFX("Blank");

        A_EventManager.InvokeCameraShake(0.03f, 0.2f);

        var hits = Physics2D.OverlapCircleAll(HitBox, (CurStats.MagnetRange + AbilityUpgrades) * 5f, _bulletLayer);

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out BulletEnemy be))
            {
                be.Dissipate();
            }
        }
    }
}
