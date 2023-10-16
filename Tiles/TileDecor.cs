using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Decoration", menuName = "New Decoration")]
public class TileDecor : ScriptableObject
{
    public Sprite Sprite;
    public float Size;
    public float Offset;
    public Vector2 Amount;
}
