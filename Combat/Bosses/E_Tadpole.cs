using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_Tadpole : Entity
{
    [SerializeField] BulletInfo _info;
    [SerializeField] GameObject _pointer;
    bool _died;

    public override void Start()
    {
        base.Start();
        Rend.flipX = transform.position.x < 1;
        HitPoints *= A_LevelManager.Instance.DifficultyModifier;
    }

    public override void Die(GameObject toDie)
    {
        if (_died)
            return;
        _died = true;

        A_EventManager.InvokeSubBreak();
        BurstAttack();
        base.Die(toDie);
    }

    public override void Update()
    {
        base.Update();

        Vector3 dir = (Player.Instance.transform.position -transform.position).normalized;
        _pointer.transform.position = Player.Instance.transform.position - dir;
        _pointer.transform.right = -dir;

    }

    void BurstAttack()
    {
        for (int i = 0; i < 360; i += 45)
        {
            float A = Mathf.Deg2Rad * i;
            Vector2 dir = new Vector2(Mathf.Cos(A), Mathf.Sin(A));

            BulletInfo info = _info;
            info.spread += Confused * 10;
            A_Factory.Instance.MakeEnemyBullet(transform.position, info, dir);
        }
    }
}
