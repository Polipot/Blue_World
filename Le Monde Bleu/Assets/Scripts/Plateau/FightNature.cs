using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightNature : Singleton<FightNature>
{
    InitiativeDisplayer ID;
    CaseManager CM;
    PlayerManager PM;
    TurnManager TM;
    TheGrid theGrid;
    FightCamera FC;

    [Header("Global")]
    [HideInInspector] public bool isPlaying;

    [Header("Initiative")]
    [HideInInspector] public int ActualInitiative;
    public int InitiativeSpeed;

    [Header("ToUpdate")]
    int Index = 0;
    [HideInInspector] public List<CaseState> ToUpdate;

    [Header("CoolDown")]
    float CoolTime;
    bool Cooled;
    bool Ready;

    private void Awake()
    {
        if (Instance != this)
        {
            Destroy(this);
        }
        PM = PlayerManager.Instance;
        CM = CaseManager.Instance;
        TM = TurnManager.Instance;
        theGrid = TheGrid.Instance;
        FC = FightCamera.Instance;
        ID = InitiativeDisplayer.Instance;
        ID.GetACadre(this);
    }
    bool FirstTurn = true; // A retirer quand un pouvoir pourra propager du feu;
    public void TurnStart()
    {
        isPlaying = true;
        if (FirstTurn)
        {
            FirstTurn = false;
            CM.GetAllCases();
            theGrid.NodeFromWorldPoint(new Vector3(1, 1, 0)).myCase.ApplyNewCaseState("Burning(Case)");
        }
        else
        {
            for(int i = 0; i < CM.AllCases.Count; i++)
            {
                if(CM.AllCases[i].myEffects.Count > 0)
                {
                    foreach(CaseState myCase in CM.AllCases[i].myEffects)
                    {
                        if (myCase.BaseTime > 0)
                        {
                            ToUpdate.Add(myCase);
                        }
                    }
                }
            }
        }

        Cooled = false;
    }

    private void Update()
    {
        if (isPlaying && !Cooled)
        {
            CoolTime += Time.deltaTime;
            if (CoolTime >= 0.4f)
            {
                CoolTime = 0;
                Cooled = true;
                Ready = true;
            }
        }

        else if (isPlaying && Cooled && Ready)
        {
            Ready = false;
            if (Index >= ToUpdate.Count)
                EndTurn();
            else
            {
                NextTerrainEffect();
            }
        }
    }

    void NextTerrainEffect()
    {
        bool Changed = false;
        if(Index < ToUpdate.Count)
        {
            if (ToUpdate[Index].myBehavior == NatureBehavior.Spread)
            {
                List<Node> Neighbours = theGrid.GetNeighbours(theGrid.grid[(int)ToUpdate[Index].myCase.PointInNode.x, (int)ToUpdate[Index].myCase.PointInNode.y]);

                for (int i = 0; i < Neighbours.Count; i++)
                {
                    if (Neighbours[i].myCase.myTypes.HasFlag(ToUpdate[Index].CaseTypesAllowed) && !Neighbours[i].myCase.DoesEffectsContain(ToUpdate[Index].name))
                    {
                        int rnd = Random.Range(0, 100);
                        if (rnd <= ToUpdate[Index].MyPPGTChance)
                        {
                            Neighbours[i].myCase.ApplyNewCaseState(ToUpdate[Index].name, ToUpdate[Index].Heritage);
                            Changed = true;
                        }
                    }
                }
            }

            Transform toShow = ToUpdate[Index].myCase.transform;
            ToUpdate[Index].RemainingTime -= 1;
            if (ToUpdate[Index].RemainingTime <= 0)
                ToUpdate[Index].myCase.RemoveState(true, ToUpdate[Index].name);

            if (Changed)
            {
                FC.target = toShow;
                Cooled = false;
                Index += 1;
            }
            else
            {
                Index += 1;

                if (Index > ToUpdate.Count)
                {
                    EndTurn();
                }
                else
                {
                    NextTerrainEffect();
                }
            }
        }
        else
        {
            EndTurn();
        }
    }

    public void EndTurn()
    {
        ToUpdate = new List<CaseState>();
        isPlaying = false;
        Ready = false;
        Index = 0;
        PM.PlayingApercu.ActualizeShowed(null);
        TM.EndTurn();
    }
}
