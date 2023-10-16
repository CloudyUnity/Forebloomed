using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_SpiderPart : Entity
{
    [SerializeField] E_Spider _body;

    public override void Start()
    {
        NormalSize = transform.localScale;
    }

    public override void Update()
    {

    }

    public override void TakeDamage(float dmg) => _body.TakeDamage(dmg);

    public override void InflictStatus() => _body.InflictStatus();
}
