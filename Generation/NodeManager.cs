using System.Collections.Generic;
using UnityEngine;

public class NodeManager : MonoBehaviour
{
    public static NodeManager Instance;

    readonly static List<Vector2> NEIGHBOUR_OFFSETS = new List<Vector2>()
    {
        new Vector2(1, 0),
        new Vector2(0, 1),
        new Vector2(0.5f, 0.5f),
        new Vector2(-0.5f, 0.5f),
    };

    LayerMask CollisionLayers = 1 << 11 | 1 << 14 | 1 << 29;

    Dictionary<Vector2, Node> allNodes = new Dictionary<Vector2, Node>();

    public static readonly float DENSITY = 1f;

    float _neighDictClearTimer;
    static readonly float NEIGHDICT_CLEAR_COOLDOWN = 5;

    private void Awake() => Instance = this;

    private void Update()
    {
        _neighDictClearTimer += Time.deltaTime;
        if (_neighDictClearTimer > NEIGHDICT_CLEAR_COOLDOWN)
        {
            neighDict.Clear();
        }
    }

    public List<Node> MakeNodes(Vector2 offset, float size)
    {
        List<Node> nodes = new List<Node>();

        Vector2 totalOffset = (offset - new Vector2(size * 0.5f, size * 0.5f)).RoundToNearest(DENSITY);

        for (float x = 0; x < size; x += DENSITY)
            for (float y = 0; y < size; y += DENSITY)
            {
                Vector2 point = totalOffset + new Vector2(x, y);

                if (Physics2D.Raycast(point, Vector3.back, 1, CollisionLayers).collider == null)
                    nodes.Add(GetNode(point));
            }

        return nodes;
    }

    Node GetNode(Vector2 pos)
    {
        if (allNodes.ContainsKey(pos))
            return allNodes[pos];

        Node n = new Node(pos);
        allNodes.Add(pos, n);
        return n;
    }

    List<Node> OpenList;
    HashSet<Node> ClosedList;

    public List<Node> FindPath(Vector2 start, Vector2 end, float nodeGridSize)
    {
        List<Node> localNodes = MakeNodes(start, nodeGridSize);

        if (localNodes.Count == 0)
            return null;

        Node startNode = ClosestNode(start, localNodes);
        Node endNode = ClosestNode(end, localNodes);

        OpenList = new List<Node>() { startNode };
        ClosedList = new HashSet<Node>();

        foreach (Node node in localNodes)
        {
            node.gCost = 0;
            node.CalculateFCost();
            node.lastNode = null;
        }

        startNode.gCost = 0;
        startNode.hCost = CalculateDistance(startNode, endNode);
        startNode.CalculateFCost();

        while(OpenList.Count > 0)
        {
            Node curNode = LowestFCostNode(OpenList);
            if (curNode == endNode)
            {
                return FinalPath(endNode);
            }

            OpenList.Remove(curNode);
            ClosedList.Add(curNode);

            List<Node> neighbourList = NeighbourList(curNode, localNodes);

            foreach (Node neighbour in neighbourList)
            {
                if (ClosedList.Contains(neighbour))
                    continue;

                float tentativeGCost = curNode.gCost + CalculateDistance(curNode, neighbour);
                if (tentativeGCost > neighbour.gCost)
                {
                    neighbour.lastNode = curNode;
                    neighbour.gCost = tentativeGCost;
                    neighbour.hCost = CalculateDistance(neighbour, endNode);
                    neighbour.CalculateFCost();

                    if (!OpenList.Contains(neighbour))
                    {
                        OpenList.Add(neighbour);
                    }
                }
            }
        }
        return null;
    }

    List<Node> FinalPath(Node endNode)
    {
        List<Node> path = new List<Node>();
        path.Add(endNode);
        Node curNode = endNode;
        while (curNode.lastNode != null)
        {
            path.Add(curNode.lastNode);
            curNode = curNode.lastNode;
        }
        path.Reverse();
        return path;
    }

    Dictionary<Node, List<Node>> neighDict = new Dictionary<Node, List<Node>>();
    List<Node> NeighbourList(Node node, List<Node> localNodes)
    {
        if (neighDict.ContainsKey(node))
            return neighDict[node];

        List<Node> results = new List<Node>();
        foreach (Node otherNode in localNodes)
        {
            foreach (Vector2 neighOffset in NEIGHBOUR_OFFSETS)
            {
                if (node.pos + neighOffset == otherNode.pos || node.pos - neighOffset == otherNode.pos)
                {
                    if (!Connected(node, otherNode))
                        continue;

                    results.Add(otherNode);
                }
            }
        }
        neighDict.Add(node, results);
        return results;
    }

    bool Connected(Node a, Node b)
    {
        Vector2 dir = (b.pos - a.pos).normalized;
        RaycastHit2D hit = Physics2D.Raycast(a.pos, dir, CalculateDistance(a, b), CollisionLayers);
#if UNITY_EDITOR
        Color c = hit.collider != null ? Color.red : Color.white;
        float duration = hit.collider != null ? 1 : 99f;
        Debug.DrawLine(a.pos, b.pos, c, duration, false);
#endif
        return hit.collider == null;
    }

    Node ClosestNode(Vector2 pos, List<Node> nodes)
    {
        float closestDistance = float.MaxValue;
        Node closestNode = nodes[0];
        foreach (Node node in nodes)
        {
            float newDis = Vector2.Distance(pos, node.pos);
            if (newDis < closestDistance)
            {
                closestNode = node;
                closestDistance = newDis;
            }
        }
        return closestNode;
    }

    Node LowestFCostNode(List<Node> list)
    {
        Node lowest = list[0];
        foreach (Node node in list)
            if (node.fCost < lowest.fCost)
                lowest = node;
        return lowest;
    }

    float CalculateDistance(Node a, Node b) => Vector2.Distance(a.pos, b.pos);

    public void OnDrawGizmos()
    {
        foreach (var node in allNodes)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(node.Key, 0.05f);
        }
    }
}
