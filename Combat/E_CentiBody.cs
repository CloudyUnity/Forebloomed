using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_CentiBody : Entity
{
    [SerializeField] E_Centipede _head;
    [SerializeField] EB_Scolopendra _bossHead;

    public override void Start()
    {
        HitPoints *= A_LevelManager.Instance.DifficultyModifier;

        NormalSize = transform.localScale;
    }

    public override void Update()
    {
        
    }

    public void Init(E_Centipede head) => _head = head;
    public void Init(EB_Scolopendra head) => _bossHead = head;

    public override void TakeDamage(float dmg)
    {
        AnimDamage();
        if (_head == null)
        {
            _bossHead.FakeTakeDamage(dmg);
            Squash(0.1f);            
            return;
        }

        _head.FakeTakeDamage(dmg);
    }

    public override void InflictStatus()
    {
        if (_head == null)
        {
            _bossHead.InflictStatus();
            return;
        }

        _head.InflictStatus();
    }
}
