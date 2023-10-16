using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class B_Slash : MonoBehaviour
{
    [SerializeField] LayerMask _hitMask;
    [SerializeField] float _radius;
    [SerializeField] SpriteRenderer _rend;

    bool _colDisabled;

    public B_Slash NextInList = null;

    public float Dmg;

    public void Slice(float dmg, Vector2 dir)
    {
        StartCoroutine(C_Dissapate());

        _colDisabled = false;
        _hitSomething = false;

        transform.up = dir;

        if (dmg > 0)
            dmg = Mathf.Clamp(dmg, 0.05f, float.MaxValue);

        Dmg = dmg;
        A_EventManager.InvokePlayerSlashSpawn(this);

        if (ItemManager.Instance.AllColors.Count > 0)
        {
            _rend.color = ItemManager.Instance.AllColors.RandomItem();
            _rend.SetAlpha(1);
        }        
    }

    bool _hitSomething;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_colDisabled)
            return;

        A_EventManager.InvokePlayerSliceCollide(this, collision.gameObject);
        _hitSomething = true;

        if (collision.tag == "Tile")
        {
            Tile tile = collision.GetComponent<Tile>();
            if (tile is TileWall wall)
            {
                wall.TakeDamage(Dmg);
                wall.InflictStatus();
            }
            return;
        }

        if (collision.tag == "Enemy")
        {
            Entity entity = collision.GetComponent<Entity>();
            entity.TakeDamage(Dmg);
            entity.InflictStatus();
            entity.KnockBack(transform.up);

            MakeInfo? info = ItemManager.Instance.RandomDrop();
            if (info != null && !(entity is E_Dummy))
            {
                A_Factory.Instance.MakeBasic(transform.position, (MakeInfo)info);
            }
            return;
        }

        if (collision.tag == "EnemyBullet")
        {
            BulletEnemy be = collision.GetComponent<BulletEnemy>();
            be.ReverseDir();
            be.ChangeToPlayer(Dmg * 0.25f);
            return;
        }

        if (collision.tag == "ITakeDamage")
        {
            ITakeDamage takeD = collision.GetComponent<ITakeDamage>();
            takeD.TakeDamage(Dmg);

            MakeInfo? info = ItemManager.Instance.RandomDrop();
            if (info != null)
            {
                A_Factory.Instance.MakeBasic(transform.position, (MakeInfo)info);
            }
        }
    }

    IEnumerator C_Dissapate()
    {
        yield return new WaitForSeconds(0.1f);
        _colDisabled = true;

        if (_hitSomething)
            A_EventManager.InvokePlaySFX("Slice");
        else
            A_EventManager.InvokePlaySFX("Slice NoHit");

        yield return new WaitForSeconds(1.7f);
        PlayerSword.Push(this);
        gameObject.SetActive(false);
    }
}
