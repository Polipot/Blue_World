using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;

public enum AttackDirection { Up, Down, Right, Left};

public class Pathfinding : Singleton<Pathfinding>
{
    TurnManager TM;
    PathRequestManager requestManager;
    TheGrid grid;


    public int DistanceCalculated;

    void Awake()
    {
        if(Instance != this)
        {
            Destroy(this);
        }
        requestManager = GetComponent<PathRequestManager>();
        grid = GetComponent<TheGrid>();
        TM = TurnManager.Instance;
    }

    public void StartFindPath(Vector3 startPos, Vector3 targetPos)
    {
        StartCoroutine(FindPath(startPos, targetPos));
    }

    IEnumerator FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();

        Vector3[] waypoints = new Vector3[0];
        bool pathSucess = false;

        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        if(startNode.walkable && targetNode.walkable && targetNode.myCase.EntityOnTop == null)
        {
            Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
            HashSet<Node> closedSet = new HashSet<Node>();
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                Node node = openSet.RemoveFirst();

                closedSet.Add(node);

                if (node == targetNode)
                {
                    sw.Stop();
                    pathSucess = true;
                    break;
                }

                foreach (Node neighbour in grid.GetNeighbours(node))
                {
                    if (!neighbour.walkable || neighbour.myCase.EntityOnTop != null || closedSet.Contains(neighbour))
                    {
                        continue;
                    }

                    int newCostToNeighbour = node.gCost + GetDistance(node, neighbour);
                    if (newCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        neighbour.gCost = newCostToNeighbour;
                        neighbour.hCost = GetDistance(neighbour, targetNode);
                        neighbour.parent = node;

                        if (!openSet.Contains(neighbour))
                            openSet.Add(neighbour);
                        else
                            openSet.UpdateItem(neighbour);
                    }
                }
            }
        }       

        yield return null;

        if (pathSucess)
        {
            waypoints = RetracePath(startNode, targetNode);

            if (waypoints.Length == 0)
            {
                pathSucess = false;
                waypoints = new Vector3[0];
            }
        }

        requestManager.FinishedProcessingPath(waypoints, pathSucess);
    }

    Vector3[] RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        DistanceCalculated = path.Count;
        Vector3[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);
        return waypoints;
    }

    Vector3[] SimplifyPath(List<Node> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;

        for (int i = 1; i < path.Count; i++)
        {
            Vector2 directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);
            if(directionNew != directionOld)
            {
                waypoints.Add(path[i-1].worldPosition);
            }
            directionOld = directionNew;
        }

        if(waypoints.Count > 0)
        {
            waypoints.Add(path[path.Count - 1].worldPosition);
        }

        if (path.Count == 1)
        {
            waypoints.Add(path[0].worldPosition);
        }

        return waypoints.ToArray();
    }

    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

    public List<Node> ReachableNodes(Transform BasePosition, int MaxDistance)
    {
        if(MaxDistance > 0)
        {
            Node StartPos = grid.NodeFromWorldPoint(BasePosition.position);
            List<Node> VerifiedNodes = new List<Node>();

            List<Node> StartNeighbours = grid.GetNeighbours(StartPos, true);

            for (int i = 0; i < StartNeighbours.Count; i++)
            {
                if (!VerifiedNodes.Contains(StartNeighbours[i]))
                {
                    VerifiedNodes.Add(StartNeighbours[i]);
                }
            }

            for (int DistanceIndex = 2; DistanceIndex <= MaxDistance; DistanceIndex++)
            {
                List<Node> NewNeighbours = new List<Node>();

                for (int i = 0; i < VerifiedNodes.Count; i++)
                {
                    List<Node> theNewNeighbours = grid.GetNeighbours(VerifiedNodes[i], true);
                    for (int a = 0; a < theNewNeighbours.Count; a++)
                    {
                        if (!NewNeighbours.Contains(theNewNeighbours[a]))
                        {
                            NewNeighbours.Add(theNewNeighbours[a]);
                        }
                    }
                }

                for (int i = 0; i < NewNeighbours.Count; i++)
                {
                    if (!VerifiedNodes.Contains(NewNeighbours[i]))
                    {
                        VerifiedNodes.Add(NewNeighbours[i]);
                    }
                }
            }

            return VerifiedNodes;
        }
        else
        {
            List<Node> theNodes = new List<Node>();
            return theNodes;
        }
    }

    public List<Node> LoadAttackReachables(Vector2 BasePosition, List<Vector2> AttackReachablePattern, aCompetence Comp, bool includeEntites = true, Node Except = null)
    {
        AttackDirection myDirection = AttackDirection.Up;

        int myEnumMemberCount = AttackDirection.GetNames(typeof(AttackDirection)).Length;

        Node StartNode = grid.grid[Mathf.RoundToInt(BasePosition.x), Mathf.RoundToInt(BasePosition.y)];

        List<Node> AttackNodes = new List<Node>();

        for (int a = 0; a < myEnumMemberCount; a++)
        {
            for (int i = 0; i < AttackReachablePattern.Count; i++)
            {
                int x = 0;
                int y = 0;

                switch (myDirection)
                {
                    case AttackDirection.Up:
                        x = Mathf.RoundToInt(BasePosition.x + AttackReachablePattern[i].x);
                        y = Mathf.RoundToInt(BasePosition.y + AttackReachablePattern[i].y);
                        break;
                    case AttackDirection.Down:
                        x = Mathf.RoundToInt(BasePosition.x - AttackReachablePattern[i].x);
                        y = Mathf.RoundToInt(BasePosition.y - AttackReachablePattern[i].y);
                        break;
                    case AttackDirection.Right:
                        x = Mathf.RoundToInt(BasePosition.x + AttackReachablePattern[i].y);
                        y = Mathf.RoundToInt(BasePosition.y - AttackReachablePattern[i].x);
                        break;
                    case AttackDirection.Left:
                        x = Mathf.RoundToInt(BasePosition.x - AttackReachablePattern[i].y);
                        y = Mathf.RoundToInt(BasePosition.y + AttackReachablePattern[i].x);
                        break;
                    default:
                        break;
                }

                if (x >= 0 && x < grid.gridWorldSize.x / 2 && y >= 0 && y < grid.gridWorldSize.y / 2)
                {
                    Node newNode = grid.grid[x, y];
                    if (newNode.myCase != null)
                    {
                        AttackNodes.Add(newNode);
                        newNode.myCase.myDirection = myDirection;
                    }
                } 
            }

            myDirection = (AttackDirection)myDirection + 1;
        }

        if (Comp.Vision_Affected)
        {
            List<Node> AllInvolvedNodes = GetAllInvolvedNodes(AttackNodes);
            AllInvolvedNodes = TriVision(StartNode, AllInvolvedNodes, Comp, includeEntites, Except);

            List<Node> FinalNodes = new List<Node>();
            for (int i = 0; i < AttackNodes.Count; i++)
            {
                if (AllInvolvedNodes.Contains(AttackNodes[i]))
                    FinalNodes.Add(AttackNodes[i]);
            }

            AttackNodes = FinalNodes;
        }

        if (Comp.Anti_Perso)
        {
            List<Node> NewAttackNodes = new List<Node>();

            for (int i = 0; i < AttackNodes.Count; i++)
            {
                if(AttackNodes[i].myCase.EntityOnTop == null && AttackNodes[i].myCase.Walkable)
                {
                    NewAttackNodes.Add(AttackNodes[i]);
                }
            }

            AttackNodes = NewAttackNodes;
        }

        if (Comp.Anti_Ground)
        {
            List<Node> NewAttackNodes = new List<Node>();

            for (int i = 0; i < AttackNodes.Count; i++)
            {
                if (AttackNodes[i].myCase.EntityOnTop != null && AttackNodes[i].myCase.Walkable)
                {
                    NewAttackNodes.Add(AttackNodes[i]);
                }
            }

            AttackNodes = NewAttackNodes;
        }

        return AttackNodes;
    }

    #region Try Vision

    public List<Node> TriVision(Node BasePosition, List<Node> AVerifier, aCompetence Comp, bool includeEntites, Node Except = null)
    {
        // mise en place de la liste des cases à vérifier et acquisition des obstacles
        List<Node> Obstacles = new List<Node>();

        for (int i = 0; i < AVerifier.Count; i++)
        {
            AVerifier[i].RemovedAsVerifiable = false;

            if ((AVerifier[i].walkable == false || AVerifier[i].myCase.EntityOnTop != null) && AVerifier[i] != Except)
            {
                Obstacles.Add(AVerifier[i]);
                AVerifier[i].RemovedAsObstacle = false;
            }
        }

        for (int i = 0; i < Obstacles.Count; i++)
        {
            if (!Obstacles[i].RemovedAsObstacle)
            {
                for (int a = 0; a < AVerifier.Count; a++)
                {
                    if (!AVerifier[a].RemovedAsVerifiable && (!Obstacles[i].myCase.EntityOnTop || (Obstacles[i].myCase.EntityOnTop && Obstacles[i] != AVerifier[a])))
                    {
                        float X = AVerifier[a].gridX; // Case à vérifier.x
                        float Y = AVerifier[a].gridY; // Case à vérifier.y
                        float X0 = BasePosition.gridX; // Position du joueur.x
                        float Y0 = BasePosition.gridY; // Position du joueur.y
                        float XN = Obstacles[i].gridX; // Position de l'obstacle.x
                        float YN = Obstacles[i].gridY; // Position de l'obstacle.y

                        float Line1 = Y1(X, X0, Y0, XN, YN); // Y1(Case à vérifier.x)
                        float Line2 = Y2(X, X0, Y0, XN, YN); // Y2(Case à vérifier.y)

                        if (Mathf.Abs(X - X0) >= Mathf.Abs(XN - X0)
                            && (X - X0) * (XN - X0) >= 0
                            && Mathf.Abs(Y - Y0) >= Mathf.Abs(YN - Y0)
                            && (Y - Y0) * (YN - Y0) >= 0
                            && AntiVisionLC(X, Y, X0, Y0, XN, YN, Line1, Line2))
                        {
                            AVerifier[a].RemovedAsVerifiable = true;
                            if (AVerifier[a].walkable == false || AVerifier[a].myCase.EntityOnTop != null)
                            {
                                Obstacles[i].RemovedAsObstacle = true;
                            }
                        }
                    }
                }
            }
        }

        List<Node> FinalNodes = new List<Node>();
        for (int i = 0; i < AVerifier.Count; i++)
        {
            if(AVerifier[i].RemovedAsVerifiable == false)
            {
                if (includeEntites)
                    FinalNodes.Add(AVerifier[i]);
                else if(!includeEntites && (AVerifier[i].myCase.EntityOnTop == null || AVerifier[i].myCase.EntityOnTop == TM.activeFighters[TM.TurnIndex]))
                    FinalNodes.Add(AVerifier[i]);
            }
        }

        return FinalNodes;
    }

    float Yspe (float X, float X0, float Y0, float XN, float YN)
    {
        float Resultat = 0;
        if(XN != X0)
        {
            Resultat = Y0 + ((YN - Y0) / (XN - X0)) * (X - X0);
        }

        return Resultat;
    }

    float Y1(float X , float X0, float Y0, float XN, float YN)
    {
        float Resultat = 0;

        if (XN <= X0 && YN < Y0)
            Resultat = Yspe(X, XN - 0.5f, YN + 0.5f, X0, Y0);

        else if (XN <= X0 && YN == Y0)
            Resultat = Yspe(X, XN + 0.5f, YN + 0.5f, X0, Y0);

        else if (XN > X0 && YN < Y0)
            Resultat = Yspe(X, X0, Y0, XN + 0.5f, YN + 0.5f);

        else if (XN < X0 && YN > Y0)
            Resultat = Yspe(X, XN + 0.5f, YN + 0.5f, X0, Y0);

        else if (XN == X0 && YN > Y0)
            Resultat = Yspe(X, X0, Y0, XN + 0.5f, YN - 0.5f);

        else if (XN > X0 && YN >= Y0)
            Resultat = Yspe(X, X0, Y0, XN - 0.5f, YN + 0.5f);

        return Resultat;
    }

    float Y2(float X, float X0, float Y0, float XN, float YN)
    {
        float Resultat = 0;

        if (XN < X0 && YN <= Y0)
            Resultat = Yspe(X, XN + 0.5f, YN - 0.5f, X0, Y0);

        else if (XN == X0 && YN <= Y0)
            Resultat = Yspe(X, X0, Y0, XN + 0.5f, YN + 0.5f);

        else if (XN > X0 && YN < Y0)
            Resultat = Yspe(X, X0, Y0, XN - 0.5f, YN - 0.5f);

        else if (XN <= X0 && YN > Y0)
            Resultat = Yspe(X, XN - 0.5f, YN - 0.5f, X0, Y0);

        else if (XN > X0 && YN > Y0)
            Resultat = Yspe(X, X0, Y0, XN + 0.5f, YN - 0.5f);

        else if (XN > X0 && YN == Y0)
            Resultat = Yspe(X, X0, Y0, XN - 0.5f, YN - 0.5f);

        return Resultat;
    }

    bool AntiVisionLC(float X, float Y, float X0, float Y0, float XN, float YN, float Line1, float Line2)
    {
        bool Blocked = false;

        if(XN != X0 && Line2 < Y && Y < Line1)
        {
            Blocked = true;
        }
        else if(XN == X0 && YN > Y0 && (X == X0 || Y > Mathf.Max(Line1, Line2)))
        {
            Blocked = true;
        }
        else if (XN == X0 && YN < Y0 && (X == X0 || Y < Mathf.Min(Line1, Line2)))
        {
            Blocked = true;
        }

        return Blocked;
    }

    List<Node> GetAllInvolvedNodes(List<Node> ToVerify)
    {
        List<Node> AllInvolvedNodes = new List<Node>();

        Vector2 CoordinatesX = new Vector2(0, 0);
        Vector2 CoordinatesY = new Vector2(0, 0);
        for (int i = 0; i < ToVerify.Count; i++)
        {
            if (CoordinatesX.x > ToVerify[i].gridX)
                CoordinatesX.x = ToVerify[i].gridX;
            else if (CoordinatesX.y < ToVerify[i].gridX)
                CoordinatesX.y = ToVerify[i].gridX;
            if (CoordinatesY.x > ToVerify[i].gridY)
                CoordinatesY.x = ToVerify[i].gridY;
            else if (CoordinatesY.y < ToVerify[i].gridY)
                CoordinatesY.y = ToVerify[i].gridY;
        }

        for (int x = (int)CoordinatesX.x; x < (int)CoordinatesX.y + 1; x++)
        {
            for (int y = (int)CoordinatesY.x; y < (int)CoordinatesY.y + 1; y++)
            {
                if (x >= 0 && x < grid.gridWorldSize.x / 2 && y >= 0 && y < grid.gridWorldSize.y / 2)
                {
                    Node newNode = grid.grid[x, y];
                    if (newNode.myCase != null)
                    {
                        AllInvolvedNodes.Add(newNode);
                    }
                }
            }
        }

        return AllInvolvedNodes;
    }

    #endregion

    #region IA

    public Node GetCloserEntities(FightEntity Me, int MaxDistance)
    {
        Node toReturn = null;
        Transform BasePosition = Me.transform;

        if (MaxDistance > 0)
        {
            Node StartPos = grid.NodeFromWorldPoint(BasePosition.position);
            List<Node> VerifiedNodes = new List<Node>();

            List<Node> StartNeighbours = grid.GetNeighbours(StartPos, false);

            for (int i = 0; i < StartNeighbours.Count; i++)
            {
                if (!VerifiedNodes.Contains(StartNeighbours[i]))
                {
                    VerifiedNodes.Add(StartNeighbours[i]);
                    if(StartNeighbours[i].myCase.EntityOnTop != null && isAnEnemy(Me, StartNeighbours[i].myCase.EntityOnTop))
                    {
                        toReturn = StartNeighbours[i];
                        break;
                    }
                }
            }

            if (toReturn == null)
            {
                for (int DistanceIndex = 2; DistanceIndex <= MaxDistance; DistanceIndex++)
                {
                    List<Node> NewNeighbours = new List<Node>();

                    for (int i = 0; i < VerifiedNodes.Count; i++)
                    {
                        List<Node> theNewNeighbours = grid.GetNeighbours(VerifiedNodes[i], false);
                        for (int a = 0; a < theNewNeighbours.Count; a++)
                        {
                            if (!NewNeighbours.Contains(theNewNeighbours[a]))
                            {
                                NewNeighbours.Add(theNewNeighbours[a]);
                            }
                        }
                    }

                    for (int i = 0; i < NewNeighbours.Count; i++)
                    {
                        if (!VerifiedNodes.Contains(NewNeighbours[i]))
                        {
                            VerifiedNodes.Add(NewNeighbours[i]);
                            if (NewNeighbours[i].myCase.EntityOnTop != null && isAnEnemy(Me, NewNeighbours[i].myCase.EntityOnTop))
                            {
                                toReturn = NewNeighbours[i];
                                break;
                            }
                        }
                    }

                    if (toReturn != null)
                        break;
                }
            }

            return toReturn;
        }
        else
        {
            return toReturn;
        }
    }

    bool isAnEnemy(FightEntity Me, FightEntity Him)
    {
        bool isItHostile = false;
        if ((Me.myAlignement == Alignement.Ennemi && Him.myAlignement != Alignement.Ennemi) || (Me.myAlignement != Alignement.Ennemi && Him.myAlignement == Alignement.Ennemi))
            isItHostile = true;

        return isItHostile;
    }

    public Node GetCloserNode(FightEntity Me, Node UltimeTargetNode, List<Node> TheNodes, aCompetence Comp)
    {
        if (Comp.Vision_Affected)
        {
            TheNodes = TriVision(UltimeTargetNode, TheNodes, Comp, false);
        }

        Node toReturn = null;
        Transform BasePosition = Me.transform;

        int MaxDistance = 90;
        Node MyNode = grid.NodeFromWorldPoint(Me.transform.position);

        if (TheNodes.Contains(grid.NodeFromWorldPoint(Me.transform.position)))
            toReturn = grid.NodeFromWorldPoint(Me.transform.position);

        if (MaxDistance > 0 && toReturn == null)
        {
            Node StartPos = grid.NodeFromWorldPoint(BasePosition.position);
            List<Node> VerifiedNodes = new List<Node>();

            List<Node> StartNeighbours = grid.GetNeighbours(StartPos ,true);

            for (int i = 0; i < StartNeighbours.Count; i++)
            {
                if (!VerifiedNodes.Contains(StartNeighbours[i]))
                {
                    VerifiedNodes.Add(StartNeighbours[i]);
                    if (TheNodes.Contains(StartNeighbours[i]))
                    {
                        toReturn = StartNeighbours[i];
                        break;
                    }
                }
            }

            if (toReturn == null)
            {
                for (int DistanceIndex = 2; DistanceIndex <= MaxDistance; DistanceIndex++)
                {
                    List<Node> NewNeighbours = new List<Node>();

                    for (int i = 0; i < VerifiedNodes.Count; i++)
                    {
                        List<Node> theNewNeighbours = grid.GetNeighbours(VerifiedNodes[i], true);
                        for (int a = 0; a < theNewNeighbours.Count; a++)
                        {
                            if (!NewNeighbours.Contains(theNewNeighbours[a]))
                            {
                                NewNeighbours.Add(theNewNeighbours[a]);
                            }
                        }
                    }

                    for (int i = 0; i < NewNeighbours.Count; i++)
                    {
                        if (!VerifiedNodes.Contains(NewNeighbours[i]))
                        {
                            VerifiedNodes.Add(NewNeighbours[i]);
                            if (TheNodes.Contains(NewNeighbours[i]))
                            {
                                toReturn = NewNeighbours[i];
                                break;
                            }
                        }
                    }

                    if (toReturn != null)
                        break;
                }
            }

            return toReturn;
        }
        else
        {
            return toReturn;
        }
    }

    #endregion
}