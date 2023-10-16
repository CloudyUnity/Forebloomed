using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class V_DebugPTManager : MonoBehaviour
{
    [SerializeField] TMP_InputField _txt;
    [SerializeField] MakeInfo _hearts;
    [SerializeField] MakeInfo _keys;
    [SerializeField] MakeInfo _gKey;

    public static readonly bool DISABLE_IN_BUILD = true;

    private void Start()
    {
#if !UNITY_EDITOR
        if (DISABLE_IN_BUILD)
            Destroy(gameObject);
#endif
    }

    public void Click()
    {
        string s = _txt.text.Trim().Replace("\n", "").Replace("\r", "");

        if (int.TryParse(s, out int i))
        {
            A_Factory.Instance.TurnToItem(Player.Instance.transform.position, ItemHolder.Instance.Get(i), 0);
            return;
        }

        if (s.StartsWith("w("))
        {
            s = s.Replace("w(", "").Replace(")", "");
            if (int.TryParse(s, out int j))
            {
                A_LevelManager.Instance.CurrentLevel = j;
                A_EventManager.InvokeSaveGame();
                A_EventManager.InvokeLoadWorld(j);
                return;
            }
            
            SceneManager.LoadScene(s);
            return;
        }

        if (s.StartsWith("save"))
        {
            A_EventManager.InvokeSaveGame();
            return;
        }

        if (s.StartsWith("hp"))
        {
            A_Factory.Instance.MakeBasic(Player.Instance.transform.position, _hearts);
            return;
        }

        if (s.StartsWith("gold"))
        {
            Player.Instance.SoftStats.GoldAmount += 10000;
            return;
        }

        if (s.StartsWith("key"))
        {
            Player.Instance.SoftStats.Keys += 1;
            A_Factory.Instance.MakeBasic(Player.Instance.transform.position, _keys);
            return;
        }

        if (s.StartsWith("gKey"))
        {
            Player.Instance.SoftStats.GoldKeys += 1;
            A_Factory.Instance.MakeBasic(Player.Instance.transform.position, _gKey);
            return;
        }

        if (s.StartsWith("r"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }

        if (s.StartsWith("stop"))
        {
            A_TimeManager.Instance.TimeMult = 0;
            return;
        }

        if (s.StartsWith("start"))
        {
            A_TimeManager.Instance.TimeMult = 1;
            return;
        }

        if (s.StartsWith("evening"))
        {
            A_TimeManager.Instance.SetTime(110);
            return;
        }

        if (s.StartsWith("night"))
        {
            A_TimeManager.Instance.SetTime(120);
            return;
        }

        if (s.StartsWith("sharks"))
        {
            A_TimeManager.Instance.TotalSharks = 999999;
            return;
        }

        if (s.StartsWith("hideAbility"))
        {
            V_HUDManager.Instance.FlipAbilityIcon(true);
            return;
        }

        if (s.StartsWith("showAbility"))
        {
            V_HUDManager.Instance.FlipAbilityIcon(false);
            return;
        }

        if (s.StartsWith("hidePoint"))
        {
            PlayerPointer.DEBUG_DISABLE = true;
            return;
        }

        if (s.StartsWith("showPoint"))
        {
            PlayerPointer.DEBUG_DISABLE = false;
            return;
        }

        if (s.StartsWith("hideBoss"))
        {
            A_BossManager.HideUIDebug = true;
            return;
        }

        if (s.StartsWith("showBoss"))
        {
            A_BossManager.HideUIDebug = false;
            return;
        }

        if (s.StartsWith("hide"))
        {
            Player.Instance.Hide(false);
            return;
        }

        if (s.StartsWith("show"))
        {
            Player.Instance.Hide(true);
            return;
        }

        if (s.StartsWith("seed"))
        {
            A_LevelManager.Instance.Seed = Random.Range(0, 10000f);
            A_EventManager.InvokeSaveGame();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }        

        foreach (var item in ItemHolder.Instance.EveryItemByIndex)
        {
            if (item.Item.Name == s)
            {
                A_Factory.Instance.TurnToItem(Player.Instance.transform.position, item.Item, 0);
                return;
            }
        }

        Debug.Log("Invalid instruction! " + s);

        A_EventManager.InvokePlaySFX("Error");
    }
}
