using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OW_Portal : ObjectWorld
{
    OW_Portal _otherPortal;
    [SerializeField] LayerMask _tileLayer;
    [SerializeField] int _ringAmount;
    [SerializeField] float _offset;
    [SerializeField] GameObject _ringPrefab;
    [SerializeField] SinWave _ringWave;
    [SerializeField] float _dur;
    [SerializeField] float _cooldown;
    [SerializeField] Animator _anim;
    [SerializeField] GameObject _lightning;
    [SerializeField] Material _spriteMat;
    [SerializeField] Material _glowMat;
    [SerializeField] SpriteRenderer _rend;
    float _timeSinceBirth;
    float _timeSinceUse;

    public bool CountedSelfForAch;

    private void OnEnable() => A_EventManager.OnFindPortal += Connect;

    private void OnDisable() => A_EventManager.OnFindPortal -= Connect;

    private void Awake()
    {
        A_EventManager.InvokeStumpSpawn(this);
    }

    public void SetOther(OW_Portal other)
    {
        _otherPortal = other;
        _lightning.SetActive(true);
        _anim.SetTrigger("Active");
        A_EventManager.OnFindPortal -= Connect;
    }

    public void Connect(OW_Portal other)
    {
        if (other == this || _otherPortal != null || _timeSinceBirth < 1)
            return;

        SetOther(other);
        _otherPortal.SetOther(this);
    }

    private void Update()
    {
        _timeSinceBirth += Time.deltaTime;
        _timeSinceUse += Time.deltaTime;

        if (_rend == null)
            return;

        _rend.material = Player.Selected == gameObject ? _glowMat : _spriteMat;

        if (Player.Instance == null || Player.Instance.Stopped)
            return;

        if (_otherPortal == null)
        {
            A_EventManager.InvokeFindPortal(this);

            if (Interact)
            {
                A_EventManager.InvokePlaySFX("Error");
                if (V_HUDManager.Instance != null) V_HUDManager.Instance.AssignErrorHelpMessage("No connection!");
            }

            return;
        }

        if (Player.Selected != gameObject)
            return;

        if (Interact)
        {
            if (_timeSinceUse < _cooldown)
            {
                A_EventManager.InvokePlaySFX("Error");
                return;
            }

            StartCoroutine(C_ZipPlayer());

            _timeSinceUse = 0;
            _otherPortal._timeSinceUse = 0;

            if (CountedSelfForAch)
                return;

            A_LevelManager.Instance.TelestumpsUsed++;
            CountedSelfForAch = true;
            _otherPortal.CountedSelfForAch = true;

            if (A_LevelManager.Instance.TelestumpsUsed >= 2)
                A_EventManager.InvokeUnlock("Mad Botanist");
        }
    }

    IEnumerator C_ZipPlayer()
    {
        Player.Instance.Teleporting = true;

        bool playerGone = false;
        for (int i = 0; i < _ringAmount; i++)
        {
            if (i > _ringAmount / 2 && !playerGone)
            {
                StartCoroutine(C_Zip(Player.Instance.gameObject, false));
                A_EventManager.InvokePlaySFX("Teleport");
                playerGone = true;
            }

            GameObject go = Instantiate(_ringPrefab, transform.position, Quaternion.identity);
            StartCoroutine(C_Zip(go, i == _ringAmount - 1));
            go.transform.up = (_otherPortal.transform.position - transform.position).normalized;
            go.GetComponent<SpriteRenderer>().color = Player.Instance.PlayerColors.RandomItem();
            yield return new WaitForSeconds(_offset);
        }
    }


    IEnumerator C_Zip(GameObject go, bool last)
    {
        Vector2 startPos = transform.position;
        Vector2 endPos = _otherPortal.transform.position;
        float elapsed = 0;

        while (elapsed < _dur)
        {
            float curved = A_Extensions.CosCurve(elapsed / _dur);
            go.transform.position = Vector2.Lerp(startPos, endPos, curved);

            if (go != Player.Instance.gameObject)
            {
                go.transform.localScale = Vector2.one * A_Extensions.GetSinWave(_ringWave, curved);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (last)
        {
            Player.Instance.transform.position = _otherPortal.transform.position;
            Player.Instance.Teleporting = false;
            Player.Selected = null;
            ForceExplored(_otherPortal.transform.position);
        }

        if (go != Player.Instance.gameObject)
            Destroy(go);
    }

    void ForceExplored(Vector2 pos)
    {
        var hit = Physics2D.Raycast(pos, Vector3.back, 1, _tileLayer);
        if (hit.collider == null)
            return;

        hit.collider.GetComponent<Tile>().MarkExplored();
    }
}

[System.Serializable]
public struct SinWave
{
    public float Midline;
    public float Amplitude;
    public float Frequency;
}
