using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public float gCost;
    public float hCost;
    public float fCost;

    public Node lastNode;

    public Vector2 pos;

    public Node(Vector2 pos)
    {
        this.pos = pos;
    }

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }
}
