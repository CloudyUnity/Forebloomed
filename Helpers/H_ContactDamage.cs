using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class H_ContactDamage : MonoBehaviour
{
    [SerializeField] float _dmg;
    [SerializeField] float _freq;
    [SerializeField] bool _notFlying;
    [SerializeField] bool _damageMultiplier;
    float _timer;

    private void Start()
    {
        _timer = _freq;

        if (!_damageMultiplier || Player.Instance == null)
            return;

        _dmg += (Player.Instance.CurStats.Damage / 10);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        _timer += Time.deltaTime;
        if (_timer > _freq)
        {
            _timer = 0;

            if (collision.tag == "Enemy")
            {
                Entity entity = collision.GetComponent<Entity>();
                if (entity.Flying && _notFlying)
                    return;

                entity.TakeDamage(_dmg);
                return;
            }        
            if (collision.tag == "ITakeDamage")
            {
                collision.GetComponent<ITakeDamage>().TakeDamage(_dmg);
                return;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        _timer = _freq;
    }
}
