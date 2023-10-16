using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class O_PricedHeart : MonoBehaviour
{
    public int Cost;
    public bool Changed;
    [SerializeField] GameObject _reward;
    [SerializeField] TextPair _costTXT;
    [SerializeField] SpriteRenderer _rend;
    [SerializeField] Material _baseMat;
    [SerializeField] Material _glowMat;
    [SerializeField] Color _discountColor;
    bool _setCost;

    bool _selected { get { return Player.Selected == gameObject; } }

    private void Start()
    {
        _sqaushTimer = Random.Range(0, 1.5f);
    }

    public void UpdateCost(int newCost, bool isSale)
    {
        Cost = newCost;
        _costTXT.text = newCost.ToString();

        if (!isSale)
            return;

        _costTXT.color = _discountColor;
    }

    private void Update()
    {
        if (!_setCost)
        {
            Cost = A_LevelManager.ItemCost(3, 4, transform.position.x);

            _costTXT.text = Cost.ToString();

            if (!Changed)
                A_EventManager.InvokePHeartSpawn(this);

            _setCost = true;
        }

        _rend.material = _selected ? _glowMat : _baseMat;
        float targetSize = _selected ? 0.7f : 0.6f;
        transform.localScale = Vector2.Lerp(transform.localScale, Vector2.one * targetSize, Time.deltaTime * 5);

        _sqaushTimer += Time.deltaTime;
        if (_sqaushTimer > 1.5f)
        {
            Squash(0.4f);
            _sqaushTimer = 0;
        }

        if (!_selected)
            return;

        if (!Input.GetKeyDown(A_InputManager.Instance.Key("Interact")))
            return;

        if (Player.Instance.SoftStats.GoldAmount < Cost)
        {
            A_EventManager.InvokePlaySFX("Error");
            if (V_HUDManager.Instance != null) V_HUDManager.Instance.AssignErrorHelpMessage("Not enough gold!");
            return;
        }

        if (Player.Instance.SoftStats.CurHealth == Player.Instance.SoftStats.MaxHealth && !Changed)
        {
            A_EventManager.InvokePlaySFX("Error");
            if (V_HUDManager.Instance != null) V_HUDManager.Instance.AssignErrorHelpMessage("You're at max health silly!");
            return;
        }

        PickUp();
        A_EventManager.InvokePlaySFX("Item Buy");
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

    void PickUp()
    {
        Player.Instance.SoftStats.GoldAmount -= Cost;
        Instantiate(_reward, transform.position, Quaternion.identity);
        A_EventManager.InvokeSpendGold(Cost);
        Player.Selected = null;

        Destroy(gameObject);
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
}
