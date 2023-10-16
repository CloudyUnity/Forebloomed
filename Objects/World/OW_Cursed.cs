using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OW_Cursed : ObjectWorld
{
    [SerializeField] TextPair _nameTXT;
    [SerializeField] TextPair _descTXT;
    [SerializeField] TextPair _quipTXT;
    [SerializeField] GameObject _infoCard;
    [SerializeField] SpriteRenderer _rend;
    [SerializeField] Material _baseMat;
    [SerializeField] Material _glowMat;
    [SerializeField] SpriteRenderer _infoCardIcon;
    [SerializeField] SinWave _infoWave;
    [SerializeField] SpriteRenderer _iconBG;

    bool _selected { get { return Player.Selected == gameObject && !_taken; } }
    ItemData data;

    bool _taken;
    [SerializeField] SpriteRenderer _rootRend;
    [SerializeField] Sprite _root2;
    [SerializeField] ParticleSystem _purplePS, _sandPS;

    protected override void Start()
    {
        data = ItemHolder.Instance.GetRandomItem(transform.position.x * A_LevelManager.QuickSeed, ItemPool.Chest, out _);
        _rend.sprite = data.Icon;
        _infoCardIcon.sprite = data.Icon;
        _nameTXT.text = data.Name;
        _descTXT.text = data.GetDescription();
        _quipTXT.text = data.Quip;
        _iconBG.color = data.Colors[0];
        _iconBG.SetAlpha(0.8f);
    }

    private void Update()
    {
        float targetSize = _selected ? 0.7f : 0.6f;
        transform.localScale = Vector2.Lerp(transform.localScale, Vector2.one * targetSize, Time.deltaTime * 5);

        if (_taken)
            return;

        _rend.material = _selected ? _glowMat : _baseMat;

        Vector2 cardSize = _selected ? Vector2.one * 3 : Vector2.zero;
        _infoCard.transform.localScale = Vector2.Lerp(_infoCard.transform.localScale, cardSize, Time.deltaTime * 7);
        _infoCard.transform.localPosition = new Vector2(0, A_Extensions.GetSinWave(_infoWave, Time.time));

        if (!_selected)
            return;

        if (!Input.GetKeyDown(A_InputManager.Instance.Key("Interact")))
            return;

        PickUpItem();
        A_TimeManager.Instance.TimeMult++;
        A_EventManager.InvokePlaySFX("Hourglass");
    }


    static bool _pizzaTrig, _deathTrig;
    void PickUpItem()
    {
        _taken = true;

        GameObject go = A_Factory.Instance.TurnToItem(new Vector2(400, 400), data, 0);
        go.GetComponent<Item>().OnPickup();

        _infoCard.SetActive(false);
        _rend.sprite = null;
        _rootRend.sprite = _root2;
        Selectable = false;
        Player.Selected = null;

        Instantiate(_purplePS, transform.position - new Vector3(0.1f, 0), _purplePS.transform.rotation);
        Instantiate(_purplePS, transform.position + new Vector3(0.1f, 0), _purplePS.transform.rotation);
        Instantiate(_sandPS, transform.position, Quaternion.identity);

        if (!_pizzaTrig)
            A_EventManager.InvokeUnlock("Its Pizza Time");
        _pizzaTrig = true;

        if (_deathTrig || A_TimeManager.Instance.TimeMult < 5)
            return;

        A_EventManager.InvokeUnlock("The death that I deservioli");
        _deathTrig = true;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (Player.Selected != null || collision.gameObject.tag != "Player" || _taken)
            return;

        Player.Selected = gameObject;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (_selected && collision.gameObject.tag == "Player")
            Player.Selected = null;
    }
}
