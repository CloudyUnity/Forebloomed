using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class A_LoopMachineManager : MonoBehaviour
{
    [SerializeField] Vector3 _portalPos;
    [SerializeField] float _portalDistance;
    [SerializeField] AnimationClip _anim;
    [SerializeField] ParticleSystem _dustPS;

    bool _beginSuck = false;

    private void Start()
    {
        A_EventManager.InvokeCameraShake(0.03f, 999f);
        Player.Instance.InCutscene = true;
    }

    bool _dropped = false;
    private void Update()
    {
        if (Player.Instance == null)
            return;

        if (!_dropped)
            StartCoroutine(C_DropPlayerIn());
        _dropped = true;

        float dis = Vector3.Distance(Player.Instance.HitBox, _portalPos);

        if (dis < _portalDistance && !_beginSuck)
        {
            V_HUDManager.Instance.OverPortalLine = true;

            if (Input.GetKeyDown(A_InputManager.Instance.Key("Interact")))
            {
                StartCoroutine(C_SuckInPlayer());
                V_HUDManager.Instance.OverPortalLine = false;
            }
            return;
        }

        V_HUDManager.Instance.OverPortalLine = false;
    }

    IEnumerator C_SuckInPlayer()
    {
        _beginSuck = true;

        while (Player.Instance == null)
            yield return null;

        Player.Instance.InCutscene = true;
        A_EventManager.InvokePlaySFX("Portal");
        A_EventManager.InvokeLowerMusic(3f, false);

        Vector3 scale = Player.Instance.transform.localScale;

        float startMag = Vector3.Distance(Player.Instance.transform.position, _portalPos);
        float angle = 90;

        float elapsed = 0;
        float dur = 3;
        
        while (elapsed < dur)
        {

            float curved = A_Extensions.CosCurve(elapsed / dur);

            Vector3 dir = (Player.Instance.transform.position - _portalPos).normalized;
            Player.Instance.transform.up = dir;

            angle += curved * 20;
            float mag = Mathf.Lerp(startMag, 0.2f, curved);

            Vector3 target = _portalPos + Quaternion.Euler(0, 0, angle) * Vector2.right * mag;
            Player.Instance.transform.position = Vector3.Lerp(Player.Instance.transform.position, target, Time.deltaTime * 10);

            Player.Instance.transform.localScale = Vector3.Lerp(scale, Vector2.zero, curved);

            elapsed += Time.deltaTime;

            yield return null;
        }

        A_LevelManager.Instance.CurrentLevel = 1;

        if (A_LevelManager.Instance.DifficultyModifier == 3)
            A_EventManager.InvokeUnlock("Loop King");

        if (A_LevelManager.Instance.DifficultyModifier == 6)
            A_EventManager.InvokeUnlock("Loop Queen");

        if (ItemManager.Instance.AllItems.Count >= 50 && A_LevelManager.Instance.DifficultyModifier == 1)
            A_EventManager.InvokeUnlock("Collector");

        A_EventManager.InvokeUnlock("Sucked into a bagel");

        A_LevelManager.Instance.DifficultyModifier++;

        A_EventManager.InvokeSaveGame();

        A_EventManager.InvokeLoadWorld(1);
    }

    IEnumerator C_DropPlayerIn()
    {
        Player.Instance.InCutscene = true;
        yield return new WaitForSecondsRealtime(1f);

        float elapsed = 0;
        float dur = 0.6f;
        Vector2 start = new Vector2(0, 8);
        Vector2 end = new Vector2(0, -1);

        while (elapsed < dur)
        {
            float t = (elapsed / dur).Pow(2);
            Player.Instance.transform.position = Vector2.Lerp(start, end, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        Player.Instance.InCutscene = false;
        A_EventManager.InvokeCameraShake(0.02f, 0.2f);
        A_EventManager.InvokePlaySFX("Drop");
        _dustPS.Play();
    }
}
