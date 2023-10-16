using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectWorld : MonoBehaviour
{
    [SerializeField] float _seperationRadius;
    [SerializeField] float _size;
    public bool Selectable;
    [SerializeField] LayerMask _wallTileLayer;
    [SerializeField] bool _debugMode;
    [SerializeField] bool _destroyNotDelete;
    [SerializeField] GameObject[] _splatters;

    public bool Interact => Player.Selected == gameObject && Input.GetKeyDown(A_InputManager.Instance.Key("Interact"));

    protected virtual void Start()
    {
        if (_debugMode)
            return;

        if (Mathf.Abs(transform.position.x) + Mathf.Abs(transform.position.y) < 4)
        {
            Destroy(gameObject);
            return;
        }

        foreach (ObjectWorld obj in A_LevelManager.Instance.AllWorldObjects)
        {
            if (obj == null)
                continue;

            if (Vector2.Distance(obj.transform.position, transform.position) < _seperationRadius)
            {
                Destroy(gameObject);
                return;
            }
        }

        A_LevelManager.Instance.AllWorldObjects.Add(this);

        var hits = Physics2D.OverlapCircleAll(transform.position, _size, _wallTileLayer);
        for (int i = hits.Length - 1; i >= 0; i--)
        {
            Collider2D hit = hits[i];
            var wall = hit.GetComponent<TileWall>();
            if (_destroyNotDelete)
            {
                wall.TakeDamage(9999999999);
                continue;
            }
            wall.Convert(false);
        }

        for (int i = 0; i < _splatters.Length; i++)
        {
            GameObject splat = _splatters[i];
            if (A_Extensions.RandomChance(0.3f, A_LevelManager.QuickSeed + i))
                splat.SetActive(true);
        }
    }

    public virtual void OnTouch() { }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            OnTouch();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (Player.Selected != null || collision.gameObject.tag != "Player" || !Selectable)
            return;

        Player.Selected = gameObject;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (Player.Selected == gameObject)
                Player.Selected = null;
        }
    }
}
