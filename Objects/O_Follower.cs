using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class O_Follower : MonoBehaviour
{
    public float Distance;
    public float MinDistance;
    public Transform Target;
    public float _failChance;
    [SerializeField] bool _turnOffRotation;
    [SerializeField] bool _flipXDir;
    [SerializeField] SpriteRenderer _rend;
    [SerializeField] SpriteRenderer _rend2;
    [SerializeField] float _setYRot;
    [SerializeField] float _moveSpeed = 30;

    [SerializeField] List<Vector2> _history = new List<Vector2>();

    Vector2 _targetPos;

    [SerializeField] ParticleSystem _destroyPS;
    [SerializeField] bool _dontStartAtPlayer;
    [SerializeField] bool _disableWhenCutscene;
    [SerializeField] bool _randomizePosition;

    private void Start()
    {
        if (_randomizePosition)
            transform.position += new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f));

        _targetPos = transform.position;

        if (Player.Instance == null || _dontStartAtPlayer)
            return;

        _targetPos = Player.Instance.transform.position;
    }

    private void FixedUpdate()
    {
        if (_disableWhenCutscene && Player.Instance.InCutscene)
            return;

        if (Target == null || Vector2.Distance(transform.position, Target.transform.position) > MinDistance)
            transform.position = Vector2.Lerp(transform.position, _targetPos, Time.deltaTime * _moveSpeed);

        if (_flipXDir)
        {
            _rend.flipX = _targetPos.x < transform.position.x;
            if (_rend2 != null)
                _rend2.flipX = _rend.flipX;
        }

        if (Target == null)
        {
            return;
        }

        if (_history.Count == 0 || _history[_history.Count - 1] != (Vector2)Target.position)
            _history.Add(Target.position);

        if (_history[_history.Count - 1] == (Vector2)Target.position && Random.Range(0, 100) < _failChance)
            _history.Add(Target.position);

        if (_history.Count > 50)
            _history.RemoveAt(0);

        if (_history.Count < Distance)
            return;

        if (_history.Count > 1 && _history[1] != _history[0] && !_turnOffRotation)
            transform.right = (_history[1] - _history[0]).normalized;

        _targetPos = _history[0];
        _history.RemoveAt(0);
    }

    private void OnDestroy()
    {
        Debug.Log("Key Destroyed");

        if (_destroyPS == null || !gameObject.scene.isLoaded)
            return;

        Instantiate(_destroyPS, transform.position, Quaternion.identity);
        A_EventManager.InvokePlaySFX("KeyUse");        
    }
}
