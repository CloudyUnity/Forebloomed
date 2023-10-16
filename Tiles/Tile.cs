using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool Explored;

    [SerializeField] LayerMask AllTileLayers;

    [SerializeField] SpriteRenderer _bRend;
    [SerializeField] SpriteRenderer _tRend;
    [SerializeField] SpriteRenderer _bufferRend;

    [SerializeField] protected GameObject Bottom;
    [SerializeField] protected GameObject Top;

    [SerializeField] protected ParticleSystem GroundBurstPS;
    [SerializeField] protected ParticleSystem LeafDropPS;

    Vector3[] checkPos;

    public Vector3 _myPos = Vector3.zero;

    public virtual void MarkExplored()
    {
        Explored = true;
    }

    protected virtual void Update()
    {
        if (!Explored)
            CheckExplored();
    }

    public void SetMyPos(Vector3 pos)
    {
        _myPos = pos;
        checkPos = new Vector3[]
        {
            Vector3.down + pos,
            Vector3.up + pos,
            Vector3.right + pos,
            Vector3.left + pos,
        };
    }

    void CheckExplored()
    {
        if (_myPos == Vector3.zero || TileMap.Instance == null)
            return;

        foreach (Vector3 pos in checkPos)
        {
            Tile otherTile = TileMap.Instance.GetTileAtPos(pos);

            if (otherTile == null)
                continue;

            if (!otherTile.Explored)
                continue;

            if (otherTile is TileWall)
                continue;

            MarkExplored();
            break;
        }
    }

    public void SetSprites((Sprite, Sprite) sprites)
    {
        _tRend.sprite = sprites.Item1;
        _bRend.sprite = sprites.Item2;

        if (_bufferRend != null)
            _bufferRend.sprite = sprites.Item1;
    }

    public virtual void SetHealthToMaximum() { }
}
