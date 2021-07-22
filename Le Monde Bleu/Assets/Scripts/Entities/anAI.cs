using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum BattleSpirit { Attaquant, Ammocheur, Opportuniste }

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
    public BattleSpirit myBattleSpirit;

    [Header("BattleMemory")]
    public int index;
    public Case NextTarget;

    void Awake()
    {
        myFightEntity = GetComponent<FightEntity>();

        CM = CaseManager.Instance;
        TM = TurnManager.Instance;
        thePathfinding = Pathfinding.Instance;
        theGrid = TheGrid.Instance;

        myBattleSpirit = (BattleSpirit)Random.Range(0, 3);
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
                    NextDecision();
                }
            } 
        }
    }

    // Choix de la cible ( dépend du BattleSpirit ) OK
    // Choix de l'attaque suivante ( Pour l'instant, la première aggressive )
    // Choix de la case de déplacement ( génération de l'attaque sur la cible pour choper les cases potentielles, et détermination de la case la plus proche, qui deviendra la cible )
    // Déplacement
    // Attaque

    public void NextDecision()
    {
        if (Cooled && TM.myFS == FightSituation.Fight)
        {
            Cooled = false;

            FightEntity myNextTarget = null;
            List<FightEntity> AvailableTargets = new List<FightEntity>();
            for (int i = 0; i < TM.activeFighters.Count; i++)
            {
                if (isAnEnemy(myFightEntity, TM.activeFighters[i]))
                    AvailableTargets.Add(TM.activeFighters[i]);
            }

            if (myBattleSpirit == BattleSpirit.Opportuniste)
            {
                AvailableTargets = AvailableTargets.OrderBy(o => o.Hp + o.Armor).ToList();
                myNextTarget = AvailableTargets[0];
            }
            else if (myBattleSpirit == BattleSpirit.Attaquant)
            {
                Node NodeNextTarget = thePathfinding.GetCloserEntities(myFightEntity, 90);
                myNextTarget = NodeNextTarget.myCase.EntityOnTop;
            }
            else if (myBattleSpirit == BattleSpirit.Ammocheur)
            {
                AvailableTargets = AvailableTargets.OrderByDescending(o => o.Hp + o.Armor).ToList();
                myNextTarget = AvailableTargets[0];
            }

            myFightEntity.usableCompetences = myFightEntity.ActualizedUsableComps();
            bool AbleToAttack = false;

            for (int i = 0; i < myFightEntity.usableCompetences.Count; i++)
            {
                if (myFightEntity.usableCompetences[i].myCompetenceType == CompetenceType.Attaque)
                {
                    index = i;
                    AbleToAttack = true;
                    break;
                }
            }

            if (AbleToAttack)
            {
                List<Node> AttackReachables = thePathfinding.LoadAttackReachables(myNextTarget.OccupiedCase.PointInNode, myFightEntity.usableCompetences[index].SelectableCases, myFightEntity.usableCompetences[index], false);
                Node ToNextTarget = thePathfinding.GetCloserNode(myFightEntity, theGrid.NodeFromWorldPoint(myNextTarget.OccupiedCase.transform.position), AttackReachables, myFightEntity.usableCompetences[index]);

                if (ToNextTarget != theGrid.NodeFromWorldPoint(myFightEntity.transform.position) && myFightEntity.RemainingMovement > 0)
                {
                    PathRequestManager.RequestPath(transform.position, ToNextTarget.myCase.transform.position, myFightEntity.OnPathFound);
                }
                else if (ToNextTarget == theGrid.NodeFromWorldPoint(myFightEntity.transform.position) && myFightEntity.usableCompetences[index].EnergyCost <= myFightEntity.Energy)
                {
                    myFightEntity.LoadAttack(myFightEntity.usableCompetences[index]);
                    NextTarget = myNextTarget.OccupiedCase;
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

    bool isAnEnemy(FightEntity Me, FightEntity Him)
    {
        bool isItHostile = false;
        if ((Me.myAlignement == Alignement.Ennemi && Him.myAlignement != Alignement.Ennemi) || (Me.myAlignement != Alignement.Ennemi && Him.myAlignement == Alignement.Ennemi))
            isItHostile = true;

        return isItHostile;
    }

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

        if (AttackPattern.Count > 0)
        {
            for (int i = 0; i < AttackPattern.Count; i++)
            {
                AttackPattern[i].HighlightAttackCase(false);
            }
        }

        if (AttackCenterCase != null)
        {
            AttackPattern = new List<Case>();

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
}
