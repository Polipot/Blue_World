using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TheGrid : Singleton<TheGrid>
{
    CaseManager CM;

    [Header("Map Traduction")]
    public Sprite MapToTraduce;
    public Sprite MapToTraduce_Deploy;

    [Header("Grid Values")]
    public bool DisplayGridGizmos;
    public LayerMask unwalkableMask;
    [HideInInspector] public Vector2 gridWorldSize;
    public float nodeRadius;
    public Node[,] grid;

    float nodeDiameter;
    [HideInInspector] public int gridSizeX, gridSizeY;

    void Awake()
    {
        if(Instance != this)
        {
            Destroy(this);
        }

        nodeDiameter = nodeRadius * 2;
        CM = CaseManager.Instance;
        CreateGrid();
    }

    public int MaxSize
    {
        get
        {
            return gridSizeX * gridSizeY;
        }
    }

    void CreateGrid()
    {
        GameObject theCase = Resources.Load<GameObject>("Plateau/Case");
        CaseManager CM = CaseManager.Instance;

        float textureWidth = MapToTraduce.bounds.size.x * MapToTraduce.pixelsPerUnit;
        float textureHeight = MapToTraduce.bounds.size.y * MapToTraduce.pixelsPerUnit;
        //Texture2D texture = MapToTraduce.texture;
        grid = new Node[(int)textureWidth, (int)textureHeight];
        gridWorldSize = new Vector2(textureWidth * 2, textureHeight * 2);
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.up * gridWorldSize.y / 2;

        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.up * (y * nodeDiameter + nodeRadius);

                CaseProperties myProperties = Resources.Load<CaseProperties>("CaseProperty/" + RoundToNearestMile(MapToTraduce.texture.GetPixel(x, y).r) + "_" + 
                    RoundToNearestMile(MapToTraduce.texture.GetPixel(x, y).g) + "_" + 
                    RoundToNearestMile(MapToTraduce.texture.GetPixel(x, y).b));
                if (myProperties)
                {
                    Case newCase = null;
                    GameObject theCaseObject = Instantiate(theCase, worldPoint, theCase.transform.rotation, CM.transform);
                    newCase = theCaseObject.GetComponent<Case>();
                    newCase.Traduction(myProperties);
                    CM.GetAlignDeploy(new Vector3(RoundToNearestMile(MapToTraduce_Deploy.texture.GetPixel(x, y).r), RoundToNearestMile(MapToTraduce_Deploy.texture.GetPixel(x, y).g), 
                        RoundToNearestMile(MapToTraduce_Deploy.texture.GetPixel(x, y).b)), newCase);

                    grid[x, y] = new Node(myProperties.isWalkable, worldPoint, x, y, newCase);
                }
                else
                {
                    Debug.LogError("Missing Case Property: " + RoundToNearestMile(MapToTraduce.texture.GetPixel(x, y).r) + "_" +
                        RoundToNearestMile(MapToTraduce.texture.GetPixel(x, y).g) + "_" +
                        RoundToNearestMile(MapToTraduce.texture.GetPixel(x, y).b));
                }
            }
        }
    }

    List<Node> neighbours = new List<Node>();
    Node theNode = null;
    int checkX = 0;
    int checkY = 0;

    public List<Node> GetNeighbours(Node node, bool NeedToBeUnocupied = false)
    {
        neighbours.Clear();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x != 0 && y != 0)
                    continue;

                checkX = node.gridX + x;
                checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    theNode = grid[checkX, checkY];
                    if(theNode.walkable && (!NeedToBeUnocupied || theNode.myCase.EntityOnTop == null))
                        neighbours.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }

    public List<Node> GetNeighboursExcept(Node node, Node Exception ,bool NeedToBeUnocupied = false)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x != 0 && y != 0)
                    continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    Node theNode = grid[checkX, checkY];
                    if (theNode.walkable && (!NeedToBeUnocupied || theNode.myCase.EntityOnTop == null || Exception == theNode))
                        neighbours.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }

    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.y + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        if ((gridSizeX - 1) * percentX < 0.9f)
            x = 0;
        else if((gridSizeX - 1) * percentX > (gridSizeX - 2))
        {
            x = gridSizeX - 1;
        }

        if ((gridSizeY - 1) * percentY < 0.9f)
            y = 0;
        else if ((gridSizeY - 1) * percentY > (gridSizeY - 2))
        {
            y = gridSizeY - 1;
        }

        return grid[x, y];
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, gridWorldSize.y, 1));
            if (grid != null && DisplayGridGizmos)
            {
                foreach (Node n in grid)
                {
                    Gizmos.color = (n.walkable) ? Color.white : Color.red;
                    Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - .1f));
                }
            }
    }

    float RoundToNearestMile(float toRound)
    {
        toRound = (int)(toRound * 255);
        toRound = Mathf.Round(toRound * 1000);
        toRound /= 1000;
        return toRound;
    }
}