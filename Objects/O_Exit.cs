using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class O_Exit : MonoBehaviour
{
    [SerializeField] SpriteRenderer _rend;
    [SerializeField] BoxCollider2D _col;
    [SerializeField] ParticleSystem _openPS, _exitPS;
    [SerializeField] Animator _anim;
    [SerializeField] float _activateAfter;
    [SerializeField] V_FocalPoint _focal;

    public static bool Entered;

    bool _opened;
    bool _selected { get { return Player.Selected == gameObject; } }

    private void OnEnable()
    {
        Entered = false;
    }

    private void Start()
    {
        Entered = false;

        if (A_LevelManager.Instance.CurrentLevel.IsEven() || A_LevelManager.Instance.SceneTime > 3) 
            return;

        StartCoroutine(C_FreshAir());
    }

    private void Update()
    {
        if (V_HUDManager.Instance.IsPaused)
            return;

        if (_selected && !Player.Instance.Dead && Input.GetKeyDown(A_InputManager.Instance.Key("Interact")))
        {
            A_EventManager.InvokeNextScene();
            A_EventManager.InvokePlaySFX("Exit Enter");
            if (!A_LevelManager.Instance.CurrentLevel.IsEven())
            {
                if (A_TimeManager.Instance != null && A_TimeManager.Instance.TimeRemaining < 1 && A_TimeManager.Instance.TimeRemaining > -1)
                    A_EventManager.InvokeUnlock("Suspicious");

                StartCoroutine(C_TheBossCanSuckME());
            }
        }

        if (A_TimeManager.Instance == null)
            return;

        if (A_TimeManager.Instance.TimePercent >= _activateAfter && !_opened)
        {
            Open();
        }
    }

    public void Open()
    {
        _rend.enabled = true;
        _col.enabled = true;
        _focal.enabled = true;
        _anim.SetTrigger("Open");
        _opened = true;
        A_EventManager.InvokePlaySFX("Exit Open");
        A_EventManager.InvokeExitOpen(this);
        _openPS.Play();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (Player.Selected != null || collision.gameObject.tag != "Player")
            return;

        Player.Selected = gameObject;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (_selected && collision.gameObject.tag == "Player")
            Player.Selected = null;
    }

    IEnumerator C_FreshAir()
    {
        while (Player.Instance == null)
            yield return null;

        if (!A_LevelManager.Instance.BossLevel)
            Player.Instance.InCutscene = true;
        Player.Instance.Hide(false);
        yield return new WaitForSecondsRealtime(0.8f);
        Player.Instance.Hide(true);
        if (!A_LevelManager.Instance.BossLevel)
            Player.Instance.InCutscene = true;

        float elapsed = 0;
        float dur = 0.5f;
        Vector2 pos = transform.position + (Random.onUnitSphere.normalized * 0.5f);
        Vector2 scale = Player.Instance.CurStats.Size * Vector2.one;
        Vector2 rise = pos;
        rise.y += 0.3f;

        if (_exitPS == null)
            yield break;

        _exitPS.Play(); 
        A_EventManager.InvokeCameraShake(0.02f, 0.2f);
        A_EventManager.InvokePlaySFX("Exit Exit");

        bool closed = false;
        while (elapsed < dur)
        {
            if (elapsed < 0.1f)
            {
                Player.Instance.transform.localScale = Vector2.Lerp(Vector2.zero, scale, elapsed / 0.1f);
            }
            else
                Player.Instance.transform.localScale = scale;

            float humped = 1 - A_Extensions.HumpCurve(elapsed / dur, 0, 1);

            if (elapsed < dur / 2)
                Player.Instance.transform.position = Vector3.Lerp(transform.position, rise, humped);
            else
                Player.Instance.transform.position = Vector3.Lerp(pos, rise, humped);

            if (elapsed > dur * 0.5f && !closed)
            {
                _anim.SetTrigger("Close");
                closed = true;
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        _anim.SetTrigger("Close");

        if (V_HUDManager.Instance.IsBossIntroing)
            yield break;

        Player.Instance.InCutscene = false;
        A_EventManager.InvokeCameraShake(0.01f, 0.1f);
    }


    IEnumerator C_TheBossCanSuckME()
    {
        float elapsed = 0;
        float dur = 0.5f;
        Vector2 pos = Player.Instance.transform.position;
        Vector2 end = transform.position + new Vector3(0, -0.1f);
        Vector2 scale = Player.Instance.transform.localScale;
        Player.Instance.InCutscene = true;
        Entered = true;

        while (elapsed < dur)
        {
            float c = A_Extensions.CosCurve(elapsed / dur);
            Player.Instance.transform.localScale = Vector2.Lerp(scale, Vector2.zero, c);

            Player.Instance.transform.position = Vector2.Lerp(pos, end, c);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        Player.Instance.transform.localScale = Vector2.zero;
        _exitPS.Play();
        A_EventManager.InvokeCameraShake(0.01f, 0.1f);
    }
}
