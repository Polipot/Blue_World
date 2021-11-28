using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class anAI : MonoBehaviour
{
    CaseManager CM;
    TurnManager TM;
    Pathfinding thePathfinding;
    TheGrid theGrid;

    [Header("Cooldown")]
    float CooldownLatence = 1f;
    float CooldownTemps;
    public bool Cooled;
    public bool NeedsToBeCooled;

    [Header("Composants")]
    FightEntity myFightEntity;

    [Header("Général")]

    [Header("BattleMemory")]
    [HideInInspector] public int index;
    [HideInInspector] public Case NextTarget;
    [HideInInspector] public List<Node> MemoAttackReachables = new List<Node>();
    [HideInInspector] public Node MemoGetCloserNode = null;
    [HideInInspector] public bool CoroutineProcessing = false;

    void Awake()
    {
        myFightEntity = GetComponent<FightEntity>();

        CM = CaseManager.Instance;
        TM = TurnManager.Instance;
        thePathfinding = Pathfinding.Instance;
        theGrid = TheGrid.Instance;
    }

    private void Update()
    {
        if (!Cooled && NeedsToBeCooled)
        {
            CooldownTemps += Time.deltaTime;
            if(CooldownTemps >= CooldownLatence)
            {
                CooldownTemps = 0;
                Cooled = true;
                NeedsToBeCooled = false;
                if (NextTarget)
                {
                    Cooled = false;
                    ValidateAttack(NextTarget);
                }
                else if(TM.AreBothSideAlive())
                {
                    StartCoroutine(DecisionProcess());
                    //NextDecision();
                }
            } 
        }
    }

    #region NextDecisionVariables

    // Informations finales
    aCompetence ToUse = null;
    int BestOpportunity = 0;
    Case BestWhereToGo = null;
    Case BestWhereToStrike = null;

    // Informations locales finales
    aCompetence CompToTest = null;

    int myOpportunity = 0;
    Case myWhereToGo = null;
    Case myWhereToStrike = null;

    List<FightEntity> TargetsToStrike = new List<FightEntity>();

    // Informations de pattern
    AttackDirection PatternDirection = AttackDirection.Up;
    AttackDirection Opposite = AttackDirection.Down;
    List<Vector2> Pattern = new List<Vector2>();

    // Informations locales
    Vector2 StrikePoint = Vector2.zero;

    Case LocalWhereToStrike = null;
    Case LocalWhereToGo = null;

    List<Node> AttackReachables = new List<Node>();
    Node ToNextTarget = null;
    List<FightEntity> myTouchedEntities = new List<FightEntity>();
    FightEntity theFE = null;

    int LocalOpportunity = 0;
    int Distance = 100;
    int NombreBlocages = 0;

    Node NextToVerify = null;
    Node Predecessor = null;

    #endregion

    IEnumerator DecisionProcess()
    {
        if (Cooled && TM.myFS == FightSituation.Fight)
        {
            Cooled = false;
            myFightEntity.usableCompetences.Clear();
            for (int c = 0; c < myFightEntity.myCompetences.Count; c++)
            {
                if (myFightEntity.myCompetences[c].EnergyCost <= myFightEntity.Energy)
                    myFightEntity.usableCompetences.Add(myFightEntity.myCompetences[c]);
            }

            ToUse = null;

            BestOpportunity = 0;
            BestWhereToGo = null;
            BestWhereToStrike = null;

            /// Les différents indexs pour simuler la boucle for
            int i = 0; // Index de la liste des compétences utilisables
            int a = 0; // Index des différentes cibles potentielles
            int b = 0; // INdex des différentes cases composant le pattern de l'attaque
            ///

            /// Variables
            CompToTest = null;
            TargetsToStrike = new List<FightEntity>();

            PatternDirection = AttackDirection.Up;
            Opposite = AttackDirection.Down;
            Pattern = new List<Vector2>();
            /// 

            #region Boucle de décision

            myOpportunity = 0;
            myWhereToGo = null;
            myWhereToStrike = null;

            while (i < myFightEntity.usableCompetences.Count)
            {
                if (myFightEntity.usableCompetences[i].myCompetenceType == CompetenceType.Amélioration || myFightEntity.usableCompetences[i].myCompetenceType == CompetenceType.Attaque)
                {
                    if (!CompToTest)
                        CompToTest = myFightEntity.usableCompetences[i];


                    if (TargetsToStrike.Count == 0)
                        TargetsToStrike = TM.GetAlignedEntities(myFightEntity, CompToTest.TargetingAllies, CompToTest.SelectableCases.Contains(new Vector2(0, 0)));

                    if (Pattern.Count == 0)
                        Pattern = CompToTest.PaternCase;


                    StrikePoint = TargetsToStrike[a].OccupiedCase.PointInNode + Pattern[b];
                    if (StrikePoint.x > 0 && StrikePoint.x < theGrid.gridSizeX && StrikePoint.y > 0 && StrikePoint.y < theGrid.gridSizeY)
                    {
                        LocalWhereToStrike = theGrid.grid[(int)StrikePoint.x, (int)StrikePoint.y].myCase;

                        MemoAttackReachables.Clear();
                        CoroutineProcessing = true;
                        StartCoroutine(thePathfinding.LoadAttackReachables_COR(this, LocalWhereToStrike.PointInNode, CompToTest.SelectableCases, CompToTest, false, null, CompToTest.PatternIsDirectionnal, Opposite));
                        yield return new WaitUntil(() => CoroutineProcessing == false);

                        AttackReachables = MemoAttackReachables;

                        /*for (int ai = 0; ai < AttackReachables.Count; ai++)
                            Debug.Log($"{i}{b}: {AttackReachables[ai].gridX}, {AttackReachables[ai].gridY}");*/

                        MemoGetCloserNode = null;
                        CoroutineProcessing = true;
                        StartCoroutine(thePathfinding.GetCloserNode_COR(this, myFightEntity, theGrid.NodeFromWorldPoint(LocalWhereToStrike.transform.position), AttackReachables, CompToTest));
                        yield return new WaitUntil(() => CoroutineProcessing == false);

                        ToNextTarget = MemoGetCloserNode;

                        LocalWhereToGo = null;
                        if (ToNextTarget != null)
                            LocalWhereToGo = ToNextTarget.myCase;

                        myTouchedEntities = thePathfinding.GetEntitiesOnPattern(StrikePoint, CompToTest, Pattern);
                        int TouchedEntities = myTouchedEntities.Count;

                        for(int e = 0; e < myTouchedEntities.Count; e++)
                        {
                            theFE = myTouchedEntities[e];

                            // Mauvaises cibles
                            if (thePathfinding.isAnEnemy(myFightEntity, theFE) == CompToTest.TargetingAllies)
                                TouchedEntities = 0;

                            // has already the state
                            if (CompToTest.myCompetenceType == CompetenceType.Amélioration && CompToTest.AppliedStates.Count >= 1 && CompToTest.AppliedStates.Count <= theFE.ActiveStates.Count)
                            {
                                bool HasntAllStates = false;
                                for (int c = 0; c < CompToTest.AppliedStates.Count; c++)
                                {
                                    bool HastheState = false;

                                    for(int f = 0; f < theFE.ActiveStates.Count; f++)
                                    {
                                        if (theFE.ActiveStates[f].name == CompToTest.AppliedStates[c])
                                        {
                                            HastheState = true;
                                            break;
                                        }
                                    }

                                    if (!HastheState)
                                    {
                                        HasntAllStates = true;
                                        break;
                                    }
                                }

                                if (!HasntAllStates)
                                    TouchedEntities -= 1;
                            }

                            // is not in the weapon filter
                            if (CompToTest.WeaponFilter != WeaponType.Everything)
                            {
                                bool isValid = false;
                                if ((theFE.FirstWeaponStats && CompToTest.WeaponFilter.HasFlag(theFE.FirstWeaponStats.myWeaponType)) || (theFE.SideWeaponStats && CompToTest.WeaponFilter.HasFlag(theFE.SideWeaponStats.myWeaponType)))
                                    isValid = true;

                                if (!isValid)
                                {
                                    TouchedEntities -= 1;
                                }
                            }
                        }

                        int Distance = 100;
                        int NombreBlocages = 0;

                        /*if(ToNextTarget != null)
                            Debug.Log(i + "" + b + ": " + "Veut aller en " + ToNextTarget.gridX + "," + ToNextTarget.gridY);*/

                        bool ToNextTargetContained = thePathfinding.ReachableNodes(myFightEntity.transform, myFightEntity.RemainingMovement, true, true).Contains(ToNextTarget);

                        if (ToNextTargetContained)
                        {
                            Distance = 0;
                            NextToVerify = ToNextTarget;

                            // La limite de C est de 30, elle sert de sécurité pour éviter les boucles infinies
                            for (int c = 0; c < 30 && NextToVerify != null; c++)
                            {
                                Predecessor = NextToVerify.Predecessor;
                                if (Predecessor != null)
                                {
                                    foreach (FightEntity theFE in NextToVerify.myCase.Bloqueurs)
                                    {
                                        if (isAnEnemy(myFightEntity, theFE))
                                        {
                                            NombreBlocages += 1;
                                        }
                                    }

                                    NextToVerify = Predecessor;
                                    Distance += 1;
                                }

                                else
                                    break;
                            }

                            //Debug.Log(Distance + " cases à parcourir, " + NombreBlocages + " blocages à prévoir");
                        }

                        LocalOpportunity = 2 * TouchedEntities * CompToTest.BaseOpportunity - Distance - NombreBlocages;
                        //Debug.Log(i + "" + b + ": " + CompToTest.Name + " :" + LocalOpportunity + " pour toucher " + TouchedEntities + " en parcourant " + Distance + " et en se faisant bloquer " + NombreBlocages);
                        if (LocalOpportunity > myOpportunity && ToNextTargetContained && LocalWhereToGo && LocalWhereToStrike)
                        {
                            myOpportunity = LocalOpportunity;
                            myWhereToGo = LocalWhereToGo;
                            myWhereToStrike = LocalWhereToStrike;
                        }
                    }

                    if (!ToUse || BestOpportunity < myOpportunity && myWhereToGo && myWhereToStrike)
                    {
                        ToUse = CompToTest;
                        BestOpportunity = myOpportunity;
                        BestWhereToGo = myWhereToGo;
                        BestWhereToStrike = myWhereToStrike;
                    }

                    yield return new WaitForEndOfFrame();

                    if (CompToTest.PatternIsDirectionnal && b == CompToTest.PaternCase.Count - 1 && PatternDirection != AttackDirection.Left)
                    {
                        b = 0;
                        PatternDirection += 1;

                        Pattern = PatternInTrueDirection(PatternDirection, CompToTest);
                        Opposite = OppositeOf(PatternDirection);
                    }

                    else
                    {
                        b += 1;
                        if (b >= Pattern.Count)
                        {
                            b = 0;
                            Pattern = new List<Vector2>();

                            a += 1;
                            if (a >= TargetsToStrike.Count)
                            {
                                a = 0;
                                TargetsToStrike.Clear();

                                i += 1;
                                CompToTest = null;
                            }
                        }
                    }

                    myOpportunity = 0;
                    myWhereToGo = null;
                    myWhereToStrike = null;
                }

                else
                    i += 1;
            }

            #endregion

            #region résolution
            if (ToUse && BestWhereToGo && BestWhereToStrike)
            {
                Node ToNextTarget = theGrid.grid[(int)BestWhereToGo.PointInNode.x, (int)BestWhereToGo.PointInNode.y];

                if (ToNextTarget != theGrid.NodeFromWorldPoint(myFightEntity.transform.position) && myFightEntity.RemainingMovement > 0)
                {
                    PathRequestManager.RequestPath(transform.position, BestWhereToGo.transform.position, myFightEntity.OnPathFound);
                }
                else if (ToNextTarget == theGrid.NodeFromWorldPoint(myFightEntity.transform.position) && ToUse.EnergyCost <= myFightEntity.Energy)
                {
                    index = myFightEntity.usableCompetences.IndexOf(ToUse);
                    myFightEntity.LoadAttack(myFightEntity.usableCompetences[index]);
                    NextTarget = BestWhereToStrike;
                    NeedsToBeCooled = true;
                }
                else
                {
                    myFightEntity.EndTurn();
                }
            }
            else
            {
                if (myFightEntity.usableCompetences.Count > 0)
                {
                    Node ToNextTarget = thePathfinding.GetCloserEntities(myFightEntity, 99);
                    List<Node> Neighbours = theGrid.GetNeighbours(ToNextTarget, true);
                    ToNextTarget = null;

                    MemoGetCloserNode = null;
                    CoroutineProcessing = true;
                    StartCoroutine(thePathfinding.GetCloserNode_COR(this, myFightEntity, null, Neighbours));
                    yield return new WaitUntil(() => CoroutineProcessing == false);

                    ToNextTarget = MemoGetCloserNode;

                    if (ToNextTarget != null && myFightEntity.RemainingMovement > 0 && ToNextTarget != theGrid.NodeFromWorldPoint(myFightEntity.transform.position))
                    {
                        PathRequestManager.RequestPath(transform.position, ToNextTarget.myCase.transform.position, myFightEntity.OnPathFound);
                    }
                    else
                    {
                        myFightEntity.EndTurn();
                    }
                }
                else
                {
                    myFightEntity.EndTurn();
                }
            }
            #endregion
        }
        else
        {
            NeedsToBeCooled = true;
        }

        yield return null;
    }

    public void NextDecision()
    {
        if (Cooled && TM.myFS == FightSituation.Fight)
        {
            Cooled = false;
            myFightEntity.usableCompetences.Clear();
            for (int i = 0; i < myFightEntity.myCompetences.Count; i++)
            {
                if(myFightEntity.myCompetences[i].EnergyCost <= myFightEntity.Energy)
                    myFightEntity.usableCompetences.Add(myFightEntity.myCompetences[i]);
            }

            ToUse = null;

            BestOpportunity = 0;
            BestWhereToGo = null;
            BestWhereToStrike = null;

            for (int i = 0; i < myFightEntity.usableCompetences.Count; i++)
            {
                if (myFightEntity.usableCompetences[i].myCompetenceType == CompetenceType.Amélioration || myFightEntity.usableCompetences[i].myCompetenceType == CompetenceType.Attaque)
                {
                    CompToTest = myFightEntity.usableCompetences[i];

                    myOpportunity = 0;
                    myWhereToGo = null;
                    myWhereToStrike = null;

                    TargetsToStrike = TM.GetAlignedEntities(myFightEntity, CompToTest.TargetingAllies, CompToTest.SelectableCases.Contains(new Vector2(0, 0)));
                    for (int a = 0; a < TargetsToStrike.Count; a++)
                    {
                        // Direction
                        PatternDirection = AttackDirection.Up;
                        Opposite = AttackDirection.Down;
                        Pattern = CompToTest.PaternCase;
                        //

                        for (int b = 0; b < Pattern.Count; b++)
                        {
                            StrikePoint = TargetsToStrike[a].OccupiedCase.PointInNode + Pattern[b];
                            if (StrikePoint.x > 0 && StrikePoint.x < theGrid.gridSizeX && StrikePoint.y > 0 && StrikePoint.y < theGrid.gridSizeY)
                            {
                                LocalWhereToStrike = theGrid.grid[(int)StrikePoint.x, (int)StrikePoint.y].myCase;
                                AttackReachables = thePathfinding.LoadAttackReachables(LocalWhereToStrike.PointInNode, CompToTest.SelectableCases, CompToTest, false, null, CompToTest.PatternIsDirectionnal, Opposite); // Critique
                                ToNextTarget = thePathfinding.GetCloserNode(myFightEntity, theGrid.NodeFromWorldPoint(LocalWhereToStrike.transform.position), AttackReachables, CompToTest); // Critique
                                LocalWhereToGo = null;
                                if (ToNextTarget != null)
                                    LocalWhereToGo = ToNextTarget.myCase;

                                myTouchedEntities = thePathfinding.GetEntitiesOnPattern(StrikePoint, CompToTest, Pattern); // Critique
                                int TouchedEntities = myTouchedEntities.Count;

                                // deleting the invalid entities
                                for (int d = 0; d < myTouchedEntities.Count; d ++)
                                {
                                    theFE = myTouchedEntities[d];
                                    // is not of the right alignement
                                    if (thePathfinding.isAnEnemy(myFightEntity, theFE) == CompToTest.TargetingAllies)
                                        TouchedEntities = 0;

                                    // has already the state
                                    if (CompToTest.myCompetenceType == CompetenceType.Amélioration && CompToTest.AppliedStates.Count >= 1 && CompToTest.AppliedStates.Count <= theFE.ActiveStates.Count)
                                    {
                                        bool HasntAllStates = false;
                                        for (int c = 0; c < CompToTest.AppliedStates.Count; c++)
                                        {
                                            bool HastheState = false;

                                            for(int e = 0; e < theFE.ActiveStates.Count; e++)
                                            {
                                                if (theFE.ActiveStates[e].name == CompToTest.AppliedStates[c])
                                                {
                                                    HastheState = true;
                                                    break;
                                                }
                                            }

                                            if (!HastheState)
                                            {
                                                HasntAllStates = true;
                                                break;
                                            }
                                        }

                                        if (!HasntAllStates)
                                            TouchedEntities -= 1;
                                    }

                                    // is not in the weapon filter
                                    if (CompToTest.WeaponFilter != WeaponType.Everything)
                                    {
                                        bool isValid = false;
                                        if ((theFE.FirstWeaponStats && CompToTest.WeaponFilter.HasFlag(theFE.FirstWeaponStats.myWeaponType)) || (theFE.SideWeaponStats && CompToTest.WeaponFilter.HasFlag(theFE.SideWeaponStats.myWeaponType)))
                                            isValid = true;

                                        if (!isValid)
                                        {
                                            TouchedEntities -= 1;
                                        }
                                    }
                                }

                                Distance = 100;
                                NombreBlocages = 0;

                                //Debug.Log(i + "" + b + ": " + "Veut aller en " + ToNextTarget.gridX + "," + ToNextTarget.gridY);
                                if (thePathfinding.ReachableNodes(myFightEntity.transform, myFightEntity.RemainingMovement, true).Contains(ToNextTarget))
                                {
                                    Distance = 0;
                                    NextToVerify = ToNextTarget;

                                    // La limite de C est de 30, elle sert de sécurité pour éviter les boucles infinies
                                    for (int c = 0; c < 30 && NextToVerify != null; c++)
                                    {
                                        Predecessor = NextToVerify.Predecessor;
                                        if (Predecessor != null)
                                        {
                                            for(int e = 0; e < NextToVerify.myCase.Bloqueurs.Count; e++)
                                            {
                                                if (isAnEnemy(myFightEntity, NextToVerify.myCase.Bloqueurs[e]))
                                                    NombreBlocages += 1;
                                            }

                                            NextToVerify = Predecessor;
                                            Distance += 1;
                                        }

                                        else
                                            break;
                                    }

                                    //Debug.Log(Distance + " cases à parcourir, " + NombreBlocages + " blocages à prévoir");
                                }

                                LocalOpportunity = 2 * TouchedEntities * CompToTest.BaseOpportunity - Distance - NombreBlocages;
                                //Debug.Log(i + "" + b + ": " + CompToTest.Name + " :" + LocalOpportunity + " pour toucher " + TouchedEntities + " en parcourant " + Distance + " et en se faisant bloquer " + NombreBlocages);
                                if (LocalOpportunity > myOpportunity && thePathfinding.ReachableNodes(myFightEntity.transform, myFightEntity.RemainingMovement, true).Contains(ToNextTarget) && LocalWhereToGo && LocalWhereToStrike)
                                {
                                    myOpportunity = LocalOpportunity;
                                    myWhereToGo = LocalWhereToGo;
                                    myWhereToStrike = LocalWhereToStrike;
                                }
                            }

                            if (CompToTest.PatternIsDirectionnal && b == CompToTest.PaternCase.Count - 1 && PatternDirection != AttackDirection.Left)
                            {
                                b = 0;
                                PatternDirection += 1;

                                Pattern = PatternInTrueDirection(PatternDirection, CompToTest);
                                Opposite = OppositeOf(PatternDirection);
                            }
                        }
                    }

                    if (!ToUse || BestOpportunity < myOpportunity && myWhereToGo && myWhereToStrike)
                    {
                        ToUse = CompToTest;
                        BestOpportunity = myOpportunity;
                        BestWhereToGo = myWhereToGo;
                        BestWhereToStrike = myWhereToStrike;
                    }
                }
            }

            if (ToUse && BestWhereToGo && BestWhereToStrike)
            {
                Node ToNextTarget = theGrid.grid[(int)BestWhereToGo.PointInNode.x, (int)BestWhereToGo.PointInNode.y];

                if (ToNextTarget != theGrid.NodeFromWorldPoint(myFightEntity.transform.position) && myFightEntity.RemainingMovement > 0)
                {
                    PathRequestManager.RequestPath(transform.position, BestWhereToGo.transform.position, myFightEntity.OnPathFound);
                }
                else if (ToNextTarget == theGrid.NodeFromWorldPoint(myFightEntity.transform.position) && ToUse.EnergyCost <= myFightEntity.Energy)
                {
                    index = myFightEntity.usableCompetences.IndexOf(ToUse); 
                    myFightEntity.LoadAttack(myFightEntity.usableCompetences[index]);
                    NextTarget = BestWhereToStrike;
                    NeedsToBeCooled = true;
                }
                else
                {
                    myFightEntity.EndTurn();
                }
            }
            else
            {
                myFightEntity.EndTurn();
            }
        }
        else
        {
            NeedsToBeCooled = true;
        }
    }

    #region Attack Preparation and Execution

    void ValidateAttack(Case TargetCase)
    {
        if (CM.AttackClickableCases.Contains(TargetCase))
        {
            myFightEntity.BeginAttack(ActualizeAttackPattern(TargetCase), TargetCase);
            NextTarget = null;
        }
    }

    List<Case> ActualizeAttackPattern(Case AttackCenterCase)
    {
        List<Case> AttackPattern = new List<Case>();

        if (AttackCenterCase != null)
        {
            for (int i = 0; i < myFightEntity.UsedCompetence.PaternCase.Count; i++)
            {
                int x = 0;
                int y = 0;

                switch (AttackCenterCase.myDirection)
                {
                    case AttackDirection.Up:
                        x = Mathf.RoundToInt(AttackCenterCase.PointInNode.x + myFightEntity.UsedCompetence.PaternCase[i].x);
                        y = Mathf.RoundToInt(AttackCenterCase.PointInNode.y + myFightEntity.UsedCompetence.PaternCase[i].y);
                        break;
                    case AttackDirection.Down:
                        x = Mathf.RoundToInt(AttackCenterCase.PointInNode.x - myFightEntity.UsedCompetence.PaternCase[i].x);
                        y = Mathf.RoundToInt(AttackCenterCase.PointInNode.y - myFightEntity.UsedCompetence.PaternCase[i].y);
                        break;
                    case AttackDirection.Right:
                        x = Mathf.RoundToInt(AttackCenterCase.PointInNode.x + myFightEntity.UsedCompetence.PaternCase[i].y);
                        y = Mathf.RoundToInt(AttackCenterCase.PointInNode.y - myFightEntity.UsedCompetence.PaternCase[i].x);
                        break;
                    case AttackDirection.Left:
                        x = Mathf.RoundToInt(AttackCenterCase.PointInNode.x - myFightEntity.UsedCompetence.PaternCase[i].y);
                        y = Mathf.RoundToInt(AttackCenterCase.PointInNode.y + myFightEntity.UsedCompetence.PaternCase[i].x);
                        break;
                    default:
                        break;
                }

                if (x >= 0 && x < theGrid.gridWorldSize.x / 2 && y >= 0 && y < theGrid.gridWorldSize.y / 2)
                {
                    Node myNode = theGrid.grid[x, y];

                    if (myNode != null && myNode.myCase != null && myNode.walkable)
                    {
                        Case newCase = myNode.myCase;
                        AttackPattern.Add(newCase);
                    }
                }
            }
        }

        return AttackPattern;
    }

    #endregion

    #region Direction Logic

    AttackDirection OppositeOf(AttackDirection ToOppose)
    {
        AttackDirection toReturn = AttackDirection.Down;

        switch (ToOppose)
        {
            case AttackDirection.Up:
                toReturn = AttackDirection.Down;
                break;
            case AttackDirection.Down:
                toReturn = AttackDirection.Up;
                break;
            case AttackDirection.Right:
                toReturn = AttackDirection.Left;
                break;
            case AttackDirection.Left:
                toReturn = AttackDirection.Right;
                break;
            default:
                break;
        }

        return toReturn;
    }

    List<Vector2> PatternInTrueDirection(AttackDirection theDirection, aCompetence CompToTest)
    {
        List<Vector2> toReturn = new List<Vector2>();

        for(int i = 0; i < CompToTest.PaternCase.Count; i++)
        {
            int x = 0;
            int y = 0;

            switch (theDirection)
            {
                case AttackDirection.Up:
                    x = Mathf.RoundToInt(CompToTest.PaternCase[i].x);
                    y = Mathf.RoundToInt(CompToTest.PaternCase[i].y);
                    break;
                case AttackDirection.Down:
                    x = Mathf.RoundToInt(-CompToTest.PaternCase[i].x);
                    y = Mathf.RoundToInt(-CompToTest.PaternCase[i].y);
                    break;
                case AttackDirection.Right:
                    x = Mathf.RoundToInt(CompToTest.PaternCase[i].y);
                    y = Mathf.RoundToInt(-CompToTest.PaternCase[i].x);
                    break;
                case AttackDirection.Left:
                    x = Mathf.RoundToInt(-CompToTest.PaternCase[i].y);
                    y = Mathf.RoundToInt(+CompToTest.PaternCase[i].x);
                    break;
                default:
                    break;
            }

            toReturn.Add(new Vector2(x, y));
        }

        return toReturn;
    }

    #endregion

    bool isAnEnemy(FightEntity Me, FightEntity Him)
    {
        bool isItHostile = false;
        if ((Me.myAlignement == Alignement.Ennemi && Him.myAlignement != Alignement.Ennemi) || (Me.myAlignement != Alignement.Ennemi && Him.myAlignement == Alignement.Ennemi))
            isItHostile = true;

        return isItHostile;
    }
}
