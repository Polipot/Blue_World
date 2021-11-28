using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerManager : Singleton<PlayerManager>
{
    EventSystem ES;
    TurnManager TM;
    TheGrid grid;

    [Header("References")]
    CaseManager CM;

    [Header("UI")]
    public anApercu PlayingApercu;
    public anApercu SelectedApercu;

    [Header("Raycast pour cases")]
    public LayerMask CaseLayer;
    Case MovementHighLightedCase;
    public Case AttackCenterCase;
    public List<Case> AttackPattern;

    [Header("Tracé du mouvement")]
    public List<Case> ManualMovement;

    [HideInInspector]
    public FightEntity actualEntity;
    public FightEntity selectedEntity;

    public bool OverUI => ES.IsPointerOverGameObject();

    // Start is called before the first frame update
    void Awake()
    {
        if(Instance != this)
        {
            Destroy(this);
        }

        ES = EventSystem.current;
        grid = TheGrid.Instance;
        CM = CaseManager.Instance;
        TM = TurnManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (actualEntity != null)
        {
            if (!OverUI)
            {
                if(actualEntity.mySituation == Situation.ChooseMove)
                {
                    MovementClick();
                    if (!Input.GetMouseButton(1))
                    {
                        MovementFlyOnCases();
                    }
                }

                else if (actualEntity.mySituation == Situation.ChooseAttack)
                {
                    AttackClick();
                    if (!Input.GetMouseButton(1))
                    {
                        AttackFlyOnCases();
                    }
                }
            }
        }
        else
        {
            if (!OverUI)
            {
                Fly();
            }
        }
    }

    #region Deploy

    void Fly()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 clickPos = new Vector2(mousePos.x, mousePos.y);
        RaycastHit2D hit = Physics2D.CircleCast(mousePos, 0.1f, Vector2.zero, CaseLayer);
        if (hit.collider != null && hit.collider.GetComponent<Case>() != null && TM.myFS == FightSituation.Deployement && hit.collider.GetComponent<Case>().Walkable)
        {
            Case newCase = hit.collider.GetComponent<Case>();

            if (newCase != null && MovementHighLightedCase == null)
            {
                MovementHighLightedCase = newCase;
                MovementHighLightedCase.HighlightCase(true);
            }
            else if (newCase != null && MovementHighLightedCase != null && MovementHighLightedCase != newCase)
            {
                MovementHighLightedCase.HighlightCase(false);
                MovementHighLightedCase = newCase;
                MovementHighLightedCase.HighlightCase(true);
            }
            if (newCase)
            {
                SelectSecondary(newCase);
                SelectDeploy(newCase);
            }
        }
        else if (MovementHighLightedCase != null)
        {
            MovementHighLightedCase.HighlightCase(false);
            MovementHighLightedCase = null;
        }
    }

    void SelectDeploy(Case NewCase)
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (!NewCase.EntityOnTop && selectedEntity && NewCase.Deployable && selectedEntity.myAlignement == NewCase.DeployAlignement)
            {
                selectedEntity.LeaveCase();
                NewCase.DeployHere(selectedEntity);
            }
        }
    }

    #endregion

    #region Movement

    void MovementClick()
    {
        if (Input.GetMouseButton(1))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 clickPos = new Vector2(mousePos.x, mousePos.y);
            RaycastHit2D hit = Physics2D.CircleCast(mousePos, 0.01f, Vector2.zero, CaseLayer);
            if (hit.collider != null && hit.collider.GetComponent<Case>() != null && actualEntity.mySituation == Situation.ChooseMove && hit.collider.GetComponent<Case>().Reachable && hit.collider.GetComponent<Case>().Walkable && actualEntity.RemainingMovement > ManualMovement.Count)
            {
                Case verifyCase = hit.collider.GetComponent<Case>();

                if (ManualMovement.Count == 0 && AreCasesNeighbours(verifyCase, actualEntity.OccupiedCase))
                {
                    ManualMovement.Add(verifyCase);
                    verifyCase.ManualMovementHighlight(true);
                }
                else if(ManualMovement.Count > 0 && !ManualMovement.Contains(verifyCase) && AreCasesNeighbours(verifyCase, ManualMovement[ManualMovement.Count - 1]))
                {
                    ManualMovement.Add(verifyCase);
                    verifyCase.ManualMovementHighlight(true);
                }
            }
        }

        if (Input.GetMouseButtonUp(1))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 clickPos = new Vector2(mousePos.x, mousePos.y);
            RaycastHit2D hit = Physics2D.CircleCast(mousePos, 0.01f, Vector2.zero, CaseLayer);
            if (hit.collider != null && hit.collider.GetComponent<Case>() != null && actualEntity.mySituation == Situation.ChooseMove && hit.collider.GetComponent<Case>().Reachable && hit.collider.GetComponent<Case>().Walkable)
            {
                if(ManualMovement.Count == 0)
                {
                    PathRequestManager.RequestPath(actualEntity.transform.position, hit.collider.transform.position, actualEntity.OnPathFound);
                }
                else if(ManualMovement.Count > 0 && ManualMovement[ManualMovement.Count - 1] == hit.collider.GetComponent<Case>())
                {
                    Vector3[] CasesPositions = new Vector3[ManualMovement.Count];
                    for (int i = 0; i < ManualMovement.Count; i++)
                    {
                        CasesPositions[i] = ManualMovement[i].transform.position;
                    }

                    actualEntity.OnPathFound(CasesPositions, true, ManualMovement.Count);
                    for (int i = 0; i < ManualMovement.Count; i++)
                    {
                        ManualMovement[i].ManualMovementHighlight(false);
                    }
                    ManualMovement = new List<Case>();
                }
                else
                {
                    for (int i = 0; i < ManualMovement.Count; i++)
                    {
                        ManualMovement[i].ManualMovementHighlight(false);
                    }
                    ManualMovement = new List<Case>();
                }
            }
            else
            {
                for (int i = 0; i < ManualMovement.Count; i++)
                {
                    ManualMovement[i].ManualMovementHighlight(false);
                }
                ManualMovement = new List<Case>();
            }
        }
    }

    void MovementFlyOnCases()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 clickPos = new Vector2(mousePos.x, mousePos.y);
        RaycastHit2D hit = Physics2D.CircleCast(mousePos, 0.1f, Vector2.zero, CaseLayer);
        if (hit.collider != null && hit.collider.GetComponent<Case>() != null && actualEntity.mySituation == Situation.ChooseMove && hit.collider.GetComponent<Case>().Walkable)
        {
            Case newCase = hit.collider.GetComponent<Case>();

            if (newCase != null && MovementHighLightedCase == null)
            {
                MovementHighLightedCase = newCase;
                MovementHighLightedCase.HighlightCase(true);
            }
            else if(newCase != null && MovementHighLightedCase != null && MovementHighLightedCase != newCase)
            {
                MovementHighLightedCase.HighlightCase(false);
                MovementHighLightedCase = newCase;
                MovementHighLightedCase.HighlightCase(true);
            }
            if(newCase)
                SelectSecondary(newCase);
        }
        else if(MovementHighLightedCase != null)
        {
            MovementHighLightedCase.HighlightCase(false);
            MovementHighLightedCase = null;
        }
    }

    #endregion

    #region Attack

    void AttackClick()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 clickPos = new Vector2(mousePos.x, mousePos.y);
            RaycastHit2D hit = Physics2D.CircleCast(mousePos, 0.01f, Vector2.zero, CaseLayer);

            if (hit.collider != null && hit.collider.GetComponent<Case>() != null && actualEntity.mySituation == Situation.ChooseAttack && hit.collider.GetComponent<Case>().Walkable && hit.collider.GetComponent<Case>() == AttackCenterCase)
            {
                if (AttackPattern.Count > 0)
                {
                    for (int i = 0; i < AttackPattern.Count; i++)
                    {
                        AttackPattern[i].HighlightAttackCase(false, true);
                    }
                }
                actualEntity.BeginAttack(AttackPattern, AttackCenterCase);
                AttackPattern = new List<Case>();
                AttackCenterCase = null;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            CM.ResetCases();
            actualEntity.EndAttack();
        }
    }

    void AttackFlyOnCases()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 clickPos = new Vector2(mousePos.x, mousePos.y);
        RaycastHit2D hit = Physics2D.CircleCast(mousePos, 0.1f, Vector2.zero, CaseLayer);
        if (hit.collider != null && hit.collider.GetComponent<Case>() != null && actualEntity.mySituation == Situation.ChooseAttack && hit.collider.GetComponent<Case>().Walkable)
        {
            Case newCase = hit.collider.GetComponent<Case>();

            if (newCase != null && AttackCenterCase == null && newCase.Attackable)
            {
                AttackCenterCase = newCase;
                AttackCenterCase.HighlightAttackCase(true, true);
                ActualizeAttackPattern();
            }
            else if (newCase != null && AttackCenterCase != null && AttackCenterCase != newCase && newCase.Attackable)
            {
                AttackCenterCase.HighlightAttackCase(false, true);
                AttackCenterCase = newCase;
                AttackCenterCase.HighlightAttackCase(true, true);
                ActualizeAttackPattern();
            }
            else if (newCase != null && AttackCenterCase != null && !newCase.Attackable)
            {
                AttackCenterCase.HighlightAttackCase(false, true);
                AttackCenterCase = null;
                ActualizeAttackPattern();
            }
        }
        else if (AttackCenterCase != null)
        {
            AttackCenterCase.HighlightAttackCase(false, true);
            AttackCenterCase = null;
            ActualizeAttackPattern();
        }
    }

    void ActualizeAttackPattern()
    {
        if(AttackPattern.Count > 0)
        {
            for (int i = 0; i < AttackPattern.Count; i++)
            {
                AttackPattern[i].HighlightAttackCase(false, true);
            }
        }

        if(AttackCenterCase != null)
        {
            AttackPattern = new List<Case>();

            for (int i = 0; i < actualEntity.UsedCompetence.PaternCase.Count; i++)
            {
                int x = 0;
                int y = 0;

                switch (AttackCenterCase.myDirection)
                {
                    case AttackDirection.Up:
                        x = Mathf.RoundToInt(AttackCenterCase.PointInNode.x + actualEntity.UsedCompetence.PaternCase[i].x);
                        y = Mathf.RoundToInt(AttackCenterCase.PointInNode.y + actualEntity.UsedCompetence.PaternCase[i].y);
                        break;
                    case AttackDirection.Down:
                        x = Mathf.RoundToInt(AttackCenterCase.PointInNode.x - actualEntity.UsedCompetence.PaternCase[i].x);
                        y = Mathf.RoundToInt(AttackCenterCase.PointInNode.y - actualEntity.UsedCompetence.PaternCase[i].y);
                        break;
                    case AttackDirection.Right:
                        x = Mathf.RoundToInt(AttackCenterCase.PointInNode.x + actualEntity.UsedCompetence.PaternCase[i].y);
                        y = Mathf.RoundToInt(AttackCenterCase.PointInNode.y - actualEntity.UsedCompetence.PaternCase[i].x);
                        break;
                    case AttackDirection.Left:
                        x = Mathf.RoundToInt(AttackCenterCase.PointInNode.x - actualEntity.UsedCompetence.PaternCase[i].y);
                        y = Mathf.RoundToInt(AttackCenterCase.PointInNode.y + actualEntity.UsedCompetence.PaternCase[i].x);
                        break;
                    default:
                        break;
                }

                if (x >= 0 && x < grid.gridWorldSize.x / 2 && y >= 0 && y < grid.gridWorldSize.y / 2)
                {
                    Node myNode = grid.grid[x, y];

                    if (myNode != null && myNode.myCase != null && myNode.walkable)
                    {
                        Case newCase = myNode.myCase;
                        AttackPattern.Add(newCase);
                        newCase.HighlightAttackCase(true, true);
                    }
                }
            }
        }

        else
        {
            AttackPattern = new List<Case>();
        }
    }

    #endregion

    void SelectSecondary(Case myCase)
    {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            if (myCase.EntityOnTop && (TM.myFS == FightSituation.Fight || myCase.EntityOnTop.myAlignement == Alignement.Membre))
                selectedEntity = myCase.EntityOnTop;
            else
            {
                if (selectedEntity)
                {
                    FightEntity toClose = selectedEntity;
                    selectedEntity = null;
                    toClose.CloseFollowingBar();
                }
                selectedEntity = null;
            }
        }
    }

    public void ForceSelectSecondary(FightEntity newFightEntity)
    {
        selectedEntity = newFightEntity;
        selectedEntity.OpenFollowingBar();
    }

    public bool AreCasesNeighbours(Case Case1, Case Case2)
    {
        if(Case1 == Case2)
        {
            return false;
        }

        else if((Case1.PointInNode.x == Case2.PointInNode.x && Mathf.Abs(Case1.PointInNode.y - Case2.PointInNode.y) == 1) 
            || (Case1.PointInNode.y == Case2.PointInNode.y && Mathf.Abs(Case1.PointInNode.x - Case2.PointInNode.x) == 1))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}