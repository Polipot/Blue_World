using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FightSituation { Deployement, Reinforcement ,Fight, Dialogue, EndFight }

public class TurnManager : Singleton<TurnManager>
{
    [Header("References")]
    FightNature FN;
    CaseManager CM;
    UIStartEnd UISE;
    DialogueManager DM;
    InitiativeDisplayer ID;

    [Header("Global")]
    public FightSituation myFS;
    public List<FightEntity> activeFighters;
    [Space]
    public int TurnIndex;
    public bool NeedsNewTurn;

    [Header("AutoDeploying")]
    float TempsBeforeAD;

    [Header("Reinforcements")]
    public List<FightEntity> Reinforcements;
    int InitiativeTime;
    public int InitiativeLatency;
    bool HasReinforcement => Reinforcements.Count > 0;
    float ReinforcementTime;
    public float ReinforcementLatency;

    [Header("Narrative")]
    public aDialogue StartingDialogue;
    public aDialogue ReinforcementDialogue;
    public aDialogue EndingDialogue;
    public List<aDialogue> OthersDialogues;

    // Start is called before the first frame update
    void Awake()
    {
        if(Instance != this)
        {
            Destroy(this);
        }

        ID = InitiativeDisplayer.Instance;
        FN = FightNature.Instance;
        CM = CaseManager.Instance;
        UISE = UIStartEnd.Instance;
        DM = DialogueManager.Instance;

        for (int i = 0; i < OthersDialogues.Count; i++)
            OthersDialogues[i].myConditionTranslated();
    }

    void FixedUpdate()
    {
        if (NeedsNewTurn && myFS == FightSituation.Fight)
        {
            bool Found = false;
            int maxSurplus = 0;
            int actualSurplus = 0;
            int Index = 0;

            bool RDeploy = false;
            if (HasReinforcement)
            {
                InitiativeTime += 20;
                if (InitiativeTime >= InitiativeLatency)
                {
                    myFS = FightSituation.Reinforcement;
                    RDeploy = true;
                }
            }

            if (!RDeploy)
            {
                for (int i = 0; i < activeFighters.Count; i++)
                {
                    actualSurplus = activeFighters[i].ActualInitiative - activeFighters[i].MaxInitiative;
                    if (actualSurplus >= 0)
                    {
                        if (!Found)
                        {
                            Found = true;
                            maxSurplus = actualSurplus;
                            Index = i;
                        }
                        else if (Found && actualSurplus > maxSurplus)
                        {
                            maxSurplus = actualSurplus;
                            Index = i;
                        }
                    }
                }

                actualSurplus = FN.ActualInitiative - 1000;
                if (actualSurplus >= 0)
                {
                    if (!Found)
                    {
                        Found = true;
                        maxSurplus = actualSurplus;
                        Index = activeFighters.Count;
                    }
                    else if (Found && actualSurplus > maxSurplus)
                    {
                        maxSurplus = actualSurplus;
                        Index = activeFighters.Count;
                    }
                }

                if (Found)
                {
                    if (Index != activeFighters.Count)
                    {
                        TurnIndex = Index;
                        activeFighters[TurnIndex].ActualInitiative = maxSurplus;
                        NewTurn();
                    }
                    else
                    {
                        TurnIndex = Index;
                        FN.ActualInitiative = maxSurplus;
                        NewTurn();
                    }
                }
                else
                {
                    for (int i = 0; i < activeFighters.Count; i++)
                    {
                        activeFighters[i].ActualInitiative += activeFighters[i].InitiativeSpeed;
                    }
                    FN.ActualInitiative += FN.InitiativeSpeed;
                }

                ID.UpdateCadres();
            }
        }

        else if(myFS == FightSituation.Deployement && TempsBeforeAD < 0.5f)
        {
            TempsBeforeAD += Time.deltaTime;
            if (TempsBeforeAD >= 0.5f)
                AutoDeploy();
        }

        else if(myFS == FightSituation.Reinforcement)
        {
            ReinforcementTime += Time.fixedDeltaTime;
            if(ReinforcementTime >= ReinforcementLatency)
            {
                ReinforcementTime = 0;
                if (Reinforcements.Count > 0)
                    Reinforcements[Reinforcements.Count - 1].ReinforcementDeploy();
                else
                {
                    if (ReinforcementDialogue)
                    {
                        myFS = FightSituation.Dialogue;
                        DM.StartDialogue(ReinforcementDialogue);
                    }
                    else
                        myFS = FightSituation.Fight;
                }  
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && myFS == FightSituation.Deployement)
        {
            CM.UnmarkDeployment();
            if (!StartingDialogue)
            {
                myFS = FightSituation.Fight;
                ID.Appear();
            }
            else if(!DM.DialogueActive)
                DM.StartDialogue(StartingDialogue, DialogueType.Start);
        }
    }

    void AutoDeploy()
    {
        for (int i = 0; i < activeFighters.Count; i++)
        {
            List<Case> RandomWalkable = new List<Case>();
            List<Case> NewRandomWalkable = new List<Case>();

            Alignement toGet = activeFighters[i].myAlignement;

            if (toGet == Alignement.Allié)
                RandomWalkable = CM.CasesDAllied;
            else if (toGet == Alignement.Membre)
                RandomWalkable = CM.CasesDMember;
            else if (toGet == Alignement.Ennemi)
                RandomWalkable = CM.CasesDEnemy;
            for (int a = 0; a < RandomWalkable.Count; a++)
            {
                if (!RandomWalkable[a].EntityOnTop)
                    NewRandomWalkable.Add(RandomWalkable[a]);
            }

            int rnd = Random.Range(0, NewRandomWalkable.Count);
            activeFighters[i].LeaveCase();
            NewRandomWalkable[rnd].DeployHere(activeFighters[i]);
        }

        ID.UpdateGridConstraint();
    }

    public void ForceReinforcements() => myFS = FightSituation.Reinforcement;

    public void NewTurn()
    {
        NeedsNewTurn = false;
        for (int i = 0; i < activeFighters.Count; i++)
        {
            activeFighters[i].GenerateBlockZone(false);
        }
        if (TurnIndex < activeFighters.Count)
        {
            for (int i = 0; i < activeFighters.Count; i++)
            {
                if (i != TurnIndex)
                {
                    activeFighters[i].GenerateBlockZone(true);
                }
            }
            activeFighters[TurnIndex].BeforeTurn();
        }

        else
        {
            FN.TurnStart();
        }       
    }

    public void EndTurn()
    {
        for (int i = 0; i < activeFighters.Count; i++)
        {
            activeFighters[i].GenerateBlockZone(false);
        }
        NeedsNewTurn = true;
    }

    public void RemoveAnEntity(FightEntity FE)
    {
        int theIndex = 0;

        for (int i = 0; i < activeFighters.Count; i++)
        {
            if(activeFighters[i] == FE)
            {
                activeFighters.RemoveAt(i);
                theIndex = i;
                break;
            }
        }

        if(theIndex < TurnIndex)
            TurnIndex -= 1;

        else if(theIndex == TurnIndex)
        {
            if (AreBothSideAlive(FE))
            {
                TurnIndex -= 1;
                EndTurn();
            }
            else if (HasReinforcement)
                ForceReinforcements();
            else
                EndFightSituation();
        }

    }

    public bool AreBothSideAlive(FightEntity Except = null)
    {
        bool AlliedPresent = false;
        bool EnnemiesPresent = false;

        for(int i = 0; i < activeFighters.Count; i++)
        {
            if (activeFighters[i].myAlignement == Alignement.Ennemi && !EnnemiesPresent && Except != activeFighters[i])
                EnnemiesPresent = true;
            else if (activeFighters[i].myAlignement != Alignement.Ennemi && !AlliedPresent && Except != activeFighters[i])
                AlliedPresent = true;

            if (AlliedPresent && EnnemiesPresent)
                break;
        }

        bool toReturn = AlliedPresent && EnnemiesPresent;
        return toReturn;
    }

    public bool WonOrLost()
    {
        bool AlliedPresent = false;
        for (int i = 0; i < activeFighters.Count; i++)
        {
            if (activeFighters[i].myAlignement != Alignement.Ennemi)
            {
                AlliedPresent = true;
                break;
            }
        }

        return AlliedPresent;
    }

    public void ActualizeBlocage()
    {
        for (int i = 0; i < activeFighters.Count; i++)
        {
            activeFighters[i].GenerateBlockZone(false);
        }
        for (int i = 0; i < activeFighters.Count; i++)
        {
            if (i != TurnIndex)
            {
                activeFighters[i].GenerateBlockZone(true);
            }
        }
    }

    public void EndFightSituation()
    {
        if (!EndingDialogue)
            EndCombat();
        else
            DialogueManager.Instance.StartDialogue(EndingDialogue, DialogueType.End);
    }

    public void EndCombat()
    {
        myFS = FightSituation.EndFight;
        UISE.TriggerEnd(WonOrLost());
    }

    public bool VerifyEventDialogues()
    {
        bool toReturn = false;

        if(OthersDialogues.Count > 0)
        {
            for(int i = 0; i < OthersDialogues.Count; i++)
            {
                if (OthersDialogues[i].isVerified())
                {
                    myFS = FightSituation.Dialogue;
                    DM.StartDialogue(OthersDialogues[i]);
                    OthersDialogues.Remove(OthersDialogues[i]);
                    toReturn = true;
                    break;
                }
            }
        }

        return toReturn;
    }
}
