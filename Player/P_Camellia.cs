using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P_Camellia : Player
{
    KeyCode _abilityKey;
    int uses;

    public bool AbilityButtonDown() => Input.GetKeyDown(_abilityKey) || A_InputManager.Instance.GamepadAbility();
    public override void Start()
    {
        base.Start();
        _abilityKey = A_InputManager.Instance.Key("Ability");
    }

    public override void Update()
    {
        base.Update();

        if (A_LevelManager.Instance.CurrentLevel.IsEven() || A_LevelManager.Instance.BossLevel)
        {
            AbilityPercent = 0;
            return;
        }

        AbilityPercent = 1 - ((float)uses / (AbilityUpgrades + 1));

        if (Stopped)
            return;

        if (AbilityButtonDown())
        {
            if (Selected == null || Selected.tag != "Item" || AbilityPercent == 0)
            {
                A_EventManager.InvokePlaySFX("Error");
                if (V_HUDManager.Instance != null) 
                    V_HUDManager.Instance.AssignErrorHelpMessage("Nice try!");
                return;
            }

            if (Selected.TryGetComponent(out Item item))
            {
                if (item.Data.Name == "Soul of the Tree")
                {
                    A_EventManager.InvokePlaySFX("Error");
                    if (V_HUDManager.Instance != null) 
                        V_HUDManager.Instance.AssignErrorHelpMessage("There can only be one!");
                    return;
                }

                GameObject go = A_Factory.Instance.TurnToItem(item.transform.position, item.Data, 1);
                go.GetComponent<Item>().DuplicatedItem = true;
                item.DuplicatedItem = true;
                uses++;
                A_EventManager.InvokeUseAbility();
                A_EventManager.InvokeUnlock("Magician");
                A_EventManager.InvokePlaySFX("Duplicate");
                Squash(0.1f);
            }
        }
    }
}
