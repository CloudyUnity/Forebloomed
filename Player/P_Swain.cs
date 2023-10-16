using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P_Swain : Player
{
    public float Percent;
    [SerializeField] MakeInfo _gold;

    public bool AbilityButtonDown() => Input.GetKeyDown(A_InputManager.Instance.Key("Ability")) || A_InputManager.Instance.GamepadAbility();
    public override void Update()
    {
        base.Update();

        int hp = SoftStats.CurHealth + SoftStats.BonusHealth;
        Percent = Mathf.Lerp(0f, 75f, hp / 15f);

        AbilityPercent = hp > 1 ? 1 : 0;

        if (hp > 1 && AbilityButtonDown())
        {
            _invincibiltyFramesTimer = -4 - AbilityUpgrades * 2;

            if (SoftStats.CurHealth > 1)
                SoftStats.CurHealth--;
            else
                SoftStats.BonusHealth--;

            A_EventManager.InvokeUseAbility();
            A_EventManager.InvokePlaySFX("SwainSac");
            Squash(0.1f);
        }
    }

    public override void TakeDamage(int amount, Vector2 from, string name)
    {
        if (amount <= 0 || Stopped || HasInvincibilty)
            return;

        if (Random.Range(0, 100f) < Percent * 1.5f)
        {
            A_EventManager.InvokePlaySFX("SwainBlock");
            _invincibiltyFramesTimer = -1;
            return;
        }

        if (SoftStats.CurHealth + SoftStats.BonusHealth <= amount)
        {
            //DIE
            base.TakeDamage(99, from, name);
            return;
        }

        ResetHealth();
        A_EventManager.InvokePlaySFX("Hurt");
        _invincibiltyFramesTimer = 0;

    }

    void ResetHealth()
    {
        SoftStats.MaxHealth = Mathf.Clamp(SoftStats.MaxHealth, 1, 6);
        SoftStats.CurHealth = 1;
        SoftStats.BonusHealth = 0;
    }
}
