using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class F_Order : MonoBehaviour
{
    [SerializeField] SpriteRenderer _rend;
    [SerializeField] float _pivot;
    [SerializeField] int _belowLayer = 900;
    [SerializeField] int _aboveLayer = 1100;

    private void Update()
    {
        if (Player.Instance == null)
            return;

        bool above = Player.Instance.transform.position.y - 0.25f > transform.position.y + _pivot;
        _rend.sortingOrder = above ? _aboveLayer : _belowLayer;
    }
}
