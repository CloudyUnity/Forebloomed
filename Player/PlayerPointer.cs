using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPointer : MonoBehaviour
{
    O_Exit _exit;

    public static bool DEBUG_DISABLE = false;

    private void OnEnable()
    {
        A_EventManager.OnExitOpen += TurnOn;
    }

    private void OnDisable()
    {
        A_EventManager.OnExitOpen -= TurnOn;
    }

    private void Start()
    {
        transform.parent = null;
    }

    bool _begunBeepBeep;

    void Update()
    {
        if (DEBUG_DISABLE)
        {
            _rend.enabled = false;
            return;
        }

        if (_exit == null)
        {
            transform.localScale = Vector2.zero;
            transform.position = Player.Instance.transform.position;
            return;
        }

        Vector3 dir = (Player.Instance.transform.position - _exit.transform.position).normalized;
        Vector2 targetPos = Player.Instance.transform.position - dir;
        transform.position = Vector2.Lerp(transform.position, targetPos, Time.deltaTime * 5.5f);
        transform.up = -dir;

        float dis = Vector2.Distance(Player.Instance.transform.position, _exit.transform.position);

        Vector2 targetSize = dis > 1 ? Vector2.one * 0.7f : Vector2.zero;
        transform.localScale = Vector2.Lerp(transform.localScale, targetSize, Time.deltaTime * 5);

        if (A_TimeManager.Instance != null && A_TimeManager.Instance.TimePercent > 0.75f && !_begunBeepBeep) 
        {
            _begunBeepBeep = true;
            StartCoroutine(C_BeepBeep());
        }
    }

    [SerializeField] SpriteRenderer _rend;
    [SerializeField] Sprite _on;
    [SerializeField] Sprite _off;

    IEnumerator C_BeepBeep()
    {
        float elapsed = 0;
        while (true)
        {
            elapsed += Time.deltaTime;
            float cooldown = Mathf.Clamp((1 - A_TimeManager.Instance.TimePercent) * 4, 0.2f, 1);

            if (elapsed > cooldown)
            {
                _rend.sprite = _rend.sprite == _on ? _off : _on;
                elapsed = 0;
            }

            yield return null;
        }
    }

    void TurnOn(O_Exit exit)
    {
        _exit = exit;
    }
}
