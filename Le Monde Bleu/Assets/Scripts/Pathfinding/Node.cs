using UnityEngine;
using System.Collections;

public class Node : IHeapItem<Node>
{

    public bool walkable;
    public bool dangerous => myCase != null && myCase.Bloqueurs.Count > 0;
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;

    public int gCost;
    public int hCost;
    public Node parent;
    int heapIndex;

    [Header("Système de Vision"), HideInInspector]
    public bool RemovedAsObstacle;
    [HideInInspector]
    public bool RemovedAsVerifiable;

    public Case myCase;

    [Header("IA memory")]
    public Node Predecessor;

    public Node(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY, Case _Case)
    {
        walkable = _walkable;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
        myCase = _Case;
        myCase.PointInNode = new Vector2(gridX, gridY);
        if (walkable)
        {
            
        }
    }

    public int fCost
    {
        get
        {
            return gCost + hCost;
        }
    }

    public int HeapIndex
    {
        get
        {
            return heapIndex;
        }
        set
        {
            heapIndex = value;
        }
    }

    public int CompareTo(Node nodeToCompare)
    {
        int compare = fCost.CompareTo(nodeToCompare.fCost);
        if (compare == 0)
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }
        return -compare;
    }
}