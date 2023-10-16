using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EBoss : Entity
{
    public float StartHitPoints;
    public float HealthPercent => HitPoints / StartHitPoints;
    public bool HasDied;

    [SerializeField] bool _isEnemy;
    [SerializeField] GameObject _flamePrefab;
    [SerializeField] Vector3 _flameOffset;
    [SerializeField] float _flameRandOffset;

    public static readonly float BOSS_HP_SCALING = 2.5f;

    public override void Start()
    {
        base.Start();

        if (_isEnemy)
            return;

        if (A_LevelManager.Instance.DifficultyModifier > 1)
            HitPoints *= (A_LevelManager.Instance.DifficultyModifier - 1) * BOSS_HP_SCALING;
        StartHitPoints = HitPoints;
        A_EventManager.InvokeBossSpawn(this);
    }

    public override void TakeDamage(float dmg)
    {
        base.TakeDamage(dmg);

        if (Player.Instance.CharacterIndex == 5 && Random.Range(0f, 100f) < 10)
        {
            Instantiate(_flamePrefab, transform.position + _flameOffset 
                + new Vector3(Random.Range(-_flameRandOffset, _flameRandOffset), Random.Range(-_flameRandOffset, _flameRandOffset)), Quaternion.identity);
        }
    }

    public override void Die(GameObject toDie)
    {
        if (_isEnemy)
        {
            base.Die(toDie);
            return;
        }

        if (HasDied)
            return;

        HasDied = true;
        A_EventManager.InvokeBossDie(this);
    }

    public virtual void ActuallyDie()
    {
        MakeCorpse();
        A_Factory.Instance.MakeBasic(transform.position, RandomDrop());
        A_EventManager.InvokePlaySFX("EnemyDie");
        Destroy(gameObject);
    }

    public virtual void ActuallyDie(GameObject toDie)
    {
        MakeCorpse();
        A_Factory.Instance.MakeBasic(transform.position, RandomDrop());
        A_EventManager.InvokePlaySFX("EnemyDie");
        Destroy(toDie);
    }
}
