using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OW_Lockbox : ObjectWorld
{
    [SerializeField] GameObject _flasher;
    [SerializeField] Sprite _openedSprite;
    [SerializeField] SpriteRenderer _rend;
    [SerializeField] ParticleSystem _lockBoxPS;

    private void Update()
    {
        if (!Selectable)
            return;

        if (Interact)
        {
            if (Player.Instance.SoftStats.Keys <= 0)
            {
                A_EventManager.InvokePlaySFX("Error");
                if (V_HUDManager.Instance != null) 
                    V_HUDManager.Instance.AssignErrorHelpMessage("Needs a key!");
                return;
            }

            StartCoroutine(C_Shake());
        }
    }

    static bool _unlatchTrig;
    void Open()
    {        
        GameObject item = A_Factory.Instance.MakeItem(0, transform.position + new Vector3(0, 0.35f, 0), ItemPool.LockBox);
        StartCoroutine(C_DropItem(item));

        _lockBoxPS.Play();
        
        A_EventManager.InvokePlaySFX("Chest Open");
        if (!_unlatchTrig)
            A_EventManager.InvokeUnlock("Unlatched");
        _unlatchTrig = true;

        _rend.sprite = _openedSprite;
        StartCoroutine(C_Shrink());
    }

    IEnumerator C_DropItem(GameObject item)
    {
        float radius = Random.Range(0.75f, 0.9f);

        float elapsed = 0;
        float dur = 0.3f;
        Vector3 rise = transform.position + new Vector3(0, 1f, 0);
        Vector3 end = transform.position + Random.onUnitSphere * radius;

        while (elapsed < dur)
        {
            if (Vector3.Distance(item.transform.position, Player.Instance.transform.position) > 50)
                yield break;

            float humped = 1 - A_Extensions.HumpCurve(elapsed / dur, 0, 1);

            if (elapsed < dur / 2)
                item.transform.position = Vector3.Lerp(transform.position, rise, humped);
            else
                item.transform.position = Vector3.Lerp(end, rise, humped);

            elapsed += Time.deltaTime;
            yield return null;
        }
        item.transform.position = end;
    }

    IEnumerator C_Shake()
    {
        Player.Instance.UseKey();
        Selectable = false;
        Player.Selected = null;

        Destroy(_flasher);
        A_EventManager.InvokeCameraShake(0.01f, 0.3f);
        A_EventManager.InvokePlaySFX("ChestShake");

        A_LevelManager.Instance.Sleep(5);

        float dur = 0.3f;
        Vector2 pos = transform.position;
        float elapsed = 0;

        while (elapsed < dur)
        {
            transform.position = pos + new Vector2(Random.value * 0.1f, Random.value * 0.1f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        A_EventManager.InvokeCameraShake(0.03f, 0.05f);
        A_LevelManager.Instance.Sleep(30);

        transform.position = pos;

        Open();
    }

    IEnumerator C_Shrink()
    {
        yield return new WaitForSeconds(2);

        float elapsed = 0;
        float dur = 1f;
        Vector2 scale = transform.localScale;

        while (elapsed < dur)
        {
            float c = A_Extensions.CosCurve(elapsed / dur);
            transform.localScale = Vector2.Lerp(scale, Vector2.zero, c);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}
