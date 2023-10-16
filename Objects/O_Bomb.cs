using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class O_Bomb : MonoBehaviour
{
    [SerializeField] float _cooldown;
    [SerializeField] float _radius;
    [SerializeField] SpriteRenderer _flasher;
    [SerializeField] Animator anim;
    [SerializeField] LayerMask _hits;
    [SerializeField] GameObject _shadow;
    [SerializeField] GameObject _crater;
    [SerializeField] int _playerDamage;
    [SerializeField] string _deathMessage = "Bomb";
    [SerializeField] SpriteRenderer _rend;

    float _flashSegment;
    float _flashTimer;
    bool _exploded;

    private void Start()
    {
        _flashSegment = (_cooldown - _timer) / 7;

        StartCoroutine(C_Grow());
    }

    float _timer;

    IEnumerator C_Grow()
    {
        float elapsed = 0;
        float dur = 0.15f;
        Vector2 scale = transform.localScale;

        while (elapsed < dur)
        {
            transform.localScale = Vector2.Lerp(Vector2.zero, scale, elapsed / dur);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = scale;
    }
    private void Update()
    {
        _timer += Time.deltaTime;
        _flashTimer += Time.deltaTime;

        if (_timer > _cooldown + 1)
            Destroy(gameObject);

        if (_exploded)
            return;

        if (_timer >= _cooldown && !_exploded)
        {
            Explode();
            _exploded = true;
            return;
        }

        if (_timer > _cooldown - 0.5f)
        {
            Color newAlpha = _flasher.color;
            newAlpha.a = 0.5f;
            _flasher.color = newAlpha;
            return;
        }

        if (_flashTimer > _flashSegment)
        {
            _flashSegment = (_cooldown - _timer) / 7;
            _flashTimer = 0;
            A_EventManager.InvokePlaySFX("Bomb Beep");
            StartCoroutine(Flash(_flashSegment));
        }
    }

    void Explode()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, _radius, Vector3.back, _hits);
        for (int i = 0; i < hits.Length && i < 30; i++)
        {
            RaycastHit2D hit = hits[i];
            if (hit.collider.tag == "Tile")
            {
                Tile tile = hit.collider.gameObject.GetComponent<Tile>();
                if (tile is TileWall wall)
                    wall.TakeDamage(10);
            }

            if (hit.collider.tag == "Player" && _playerDamage > 0)
            {
                A_EventManager.InvokeDealDamage(_playerDamage, transform.position, _deathMessage);
            }

            if (hit.collider.tag == "Enemy")
            {
                Entity entity = hit.collider.gameObject.GetComponent<Entity>();
                entity.TakeDamage(transform.localScale.x * 3);

                if (entity is E_Dummy dummy)
                    dummy.Die(dummy.gameObject);
            }

            if (hit.collider.tag.Is("ITakeDamage", "Blocker"))
            {
                hit.collider.GetComponent<ITakeDamage>().TakeDamage(5);
            }
        }
        
        _flasher.enabled = false;
        _shadow.SetActive(false);
        _rend.sortingOrder = 2000;
        if (anim != null)
            anim.SetTrigger("Explode");
        A_EventManager.InvokeCameraShake(0.02f, 0.3f);
        Instantiate(_crater, transform.position, Quaternion.identity);
    }

    IEnumerator Flash(float dur)
    {
        float elapsed = 0;

        while (elapsed < dur)
        {
            Color newAlpha = _flasher.color;
            newAlpha.a = 0.25f - 0.25f * Mathf.Cos(2 * Mathf.PI / dur * elapsed);
            _flasher.color = newAlpha;
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}
