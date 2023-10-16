using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileTrigger : MonoBehaviour
{
    [SerializeField] TileWall _tile;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        _tile.ActivateFakeFloor();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        _tile.TargetAlpha = 0.8f;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        _tile.TargetAlpha = 1;
    }
}
