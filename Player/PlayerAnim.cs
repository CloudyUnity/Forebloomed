using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnim : MonoBehaviour
{
    [SerializeField] Animator anim;
    [SerializeField] SpriteRenderer rend;
    [SerializeField] SpriteRenderer[] _weaponRends;

    [SerializeField] string _deadName;
    [SerializeField] string _hurtName;
    [SerializeField] string _walkName;
    [SerializeField] string _idleName;
    string curState;

    float timeSinceDamage = 99;
    [SerializeField] float HitDur;
    [SerializeField] float FlashFreq;

    [SerializeField] GameObject _hatManager;
    [SerializeField] bool _disableWeaponDuringInv = true;

    private void Update()
    {
        timeSinceDamage += Time.deltaTime;

        if (Player.Instance.HasInvincibilty)
        {
            rend.enabled = Mathf.Repeat(Time.time, FlashFreq) < FlashFreq / 2;
        }
        else
            rend.enabled = true;

        if (_disableWeaponDuringInv)
        {
            foreach (var wRend in _weaponRends)
            {
                wRend.enabled = rend.enabled;
            }
        }        

        _hatManager.SetActive(rend.enabled && rend.gameObject.activeSelf);

        if (Player.Instance.Dead)
        {
            ChangeAnim(_deadName);
            return;
        }

        if (timeSinceDamage < HitDur)
        {
            ChangeAnim(_hurtName);
            return;
        }

        Vector2 input = Player.Instance.PlayerInput;

        bool facingLeft = GetDir().x < 0;
        rend.flipX = facingLeft;
        _hatManager.transform.eulerAngles = new Vector3(0, facingLeft ? 180 : 0, 0);

        if (input.x != 0 || input.y != 0)
        {
            ChangeAnim(_walkName);
            return;
        }

        ChangeAnim(_idleName);
    }

    public void TookDamage()
    {
        timeSinceDamage = 0;
    }

    void ChangeAnim(string name)
    {
        if (curState == name)
            return;

        anim.Play(name);
        curState = name;
    }

    Vector2 GetDir()
    {
        if (A_InputManager.GamepadMode)
            return (V_GamepadCursor.CursorPos - (Vector2)Player.Instance.transform.position).normalized;

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.nearClipPlane;
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(mousePos);
        return (worldPosition - (Vector2)Player.Instance.transform.position).normalized;
    }
}
