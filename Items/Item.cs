using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Item : MonoBehaviour
{
    public ItemData Data;

    [SerializeField] GameObject _colorBall;
    [SerializeField] SpriteRenderer _rend;
    [SerializeField] Material _baseMat;
    [SerializeField] Material _glowMat;
    [SerializeField] TextPair _nameTXT;
    [SerializeField] TextPair _descTXT;
    [SerializeField] TextPair _quipTXT;
    [SerializeField] GameObject _infoCard;
    [SerializeField] SpriteRenderer _infoCardIcon;
    [SerializeField] SinWave _infoWave;
    [SerializeField] SpriteRenderer _iconBG;
    [SerializeField] GameObject _ps;

    public bool PickedUp;
    public bool DuplicatedItem;

    bool _selected { get { return Player.Selected == gameObject; } }

    private void ApplyItemData()
    {
        if (Data == null)
        {
            Data = ItemHolder.Instance.GetRandomItem(0, out _);
        }
        _rend.sprite = Data.Icon;
        _nameTXT.text = Data.Name;
        _descTXT.text = Data.GetDescription();
        _quipTXT.text = Data.Quip;
        _infoCardIcon.sprite = Data.Icon;
        _iconBG.color = Data.Colors[0];
        _iconBG.SetAlpha(0.8f);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (Player.Selected != null || collision.gameObject.tag != "Player" || PickedUp)
            return;

        Player.Selected = gameObject;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (Player.Selected == gameObject)
                Player.Selected = null;
        }
    }

    bool beenSelected;
    bool _appliedData;
    public virtual void Update()
    {
        if (_selected)
            beenSelected = true;

        if (!_appliedData)
        {
            ApplyItemData();
            _appliedData = true;
        }

        _sqaushTimer += Time.deltaTime;
        if (_sqaushTimer > 1.5f)
        {
            Squash(0.4f);
            _sqaushTimer = 0;
        }

        if (!beenSelected)
            return;

        _rend.material = _selected ? _glowMat : _baseMat;
        float targetSize = _selected ? 0.7f : 0.6f;
        transform.localScale = Vector2.Lerp(transform.localScale, Vector2.one * targetSize, Time.deltaTime * 5);

        Vector2 cardSize = _selected ? Vector2.one * 3 : Vector2.zero;
        _infoCard.transform.localScale = Vector2.Lerp(_infoCard.transform.localScale, cardSize, Time.deltaTime * 15);        

        if (!_selected || V_HUDManager.Instance.IsPaused)
            return;

        _infoCard.transform.localPosition = new Vector2(0, A_Extensions.GetSinWave(_infoWave, Time.time));

        if (Input.GetKeyDown(A_InputManager.Instance.Key("Interact")))
        {
            _rend.sortingOrder = 2000;
            OnPickup();
            A_EventManager.InvokePlaySFX("Item Get");
        }
    }

    float _sqaushTimer;
    public void Squash(float dur) => StartCoroutine(C_SqaushStretch(dur));
    IEnumerator C_SqaushStretch(float dur)
    {
        float elapsed = 0;
        Vector2 start = _rend.transform.localScale;

        while (elapsed < dur)
        {
            float humped = 1 - A_Extensions.HumpCurve(elapsed / dur, 0, 1);
            _rend.transform.localScale = start + new Vector2(0.1f * -humped, 0.1f * humped);
            elapsed += Time.deltaTime;
            yield return null;
        }
        _rend.transform.localScale = start;
    }

    public virtual void OnPickup()
    {
        PickedUp = true;

        Player.Selected = null;

        _sqaushTimer = -999;

        if (Data.AddSoftStatsEnabled)
            Player.Instance.AddSoftStats(Data.AddPlayerSoftStats);

        ItemManager.Instance.AllItems.Add(Data);
        Data.OnPickup();

        if (Data.Name == "Magic Mushroom")
            A_EventManager.InvokeUnlock("Shrooms");

        _infoCard.SetActive(false);
        if (_ps != null)
            _ps.SetActive(false);

        A_EventManager.InvokeCollectItem(this);
        StartCoroutine(C_Dissapate());
    }

    IEnumerator C_Dissapate()
    {
        float elapsed = 0;
        float dur = 0.5f;

        Quaternion startRot = Quaternion.Euler(0, 0, 90);

        Vector2 baseSize = transform.localScale;

        while (elapsed < dur)
        {
            Vector2 pos = Player.Instance.transform.position;
            pos.y += 1;
            transform.position = pos;

            transform.rotation = Quaternion.Lerp(startRot, Quaternion.identity, elapsed / dur);
            transform.localScale = 2f * Vector2.one * Mathf.Sin(Mathf.PI * elapsed / dur) + baseSize;

            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);
        ExplodeColors();
        transform.position = new Vector2(500, 500);
    }

    void ExplodeColors()
    {
        for (float x = 0; x < 1; x += 0.2f)
            for (float y = 0; y < 1; y += 0.2f)
            {
                Vector2 point = (Vector2)transform.position - new Vector2(0.5f, 0.5f) + new Vector2(x, y);
                GameObject go = Instantiate(_colorBall, point, Quaternion.identity);

                if (Data.Colors.Length == 0)
                    continue;

                go.GetComponent<SpriteRenderer>().color = Data.Colors[Random.Range(0, Data.Colors.Length)];
            }
    }
}
