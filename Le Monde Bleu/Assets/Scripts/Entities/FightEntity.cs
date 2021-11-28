using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.IO;
public enum Alignement { Membre, Allié, Ennemi }
public enum Situation { None, BeforeTurn , ChooseMove, Move, ChooseAttack, Attack, Blocked }
public enum Classe { Hero, Soldier }

public class FightEntity : MonoBehaviour
{
    [Header("Extern References")]
    PlayerManager PM;
    TheGrid theGrid;
    TurnManager TM;
    FightCamera FC;
    Pathfinding thePathfinding;
    CaseManager CM;
    InitiativeDisplayer ID;
    CompétencesUI CUI;
    DialogueManager DM;

    [HideInInspector]public anAI myAI;
    [HideInInspector]
    public WeaponFX myWeaponFX;

    [Header("Pathfinding")]
    int speed = 5;
    Vector3[] path;
    int targetIndex;

    [Header("Position on Grid")]
    public Case OccupiedCase;

    [Header("Global")]
    public string Nom;
    [Tooltip("Only for non-unique characters!")]
    public int SaveIndex;
    [Tooltip("Only for non-unique heroes!")]
    public string HeroType;
    [TextArea(10,30)]
    public string Description;
    public Classe myClasse;
    public Alignement myAlignement;
    public Situation mySituation;
    public bool IsPlaying;

    [Header("Armes")]
    public Transform FirstWeaponTransform;
    public Transform SideWeaponTransform;
    [Space]
    public string FirstWeaponName;   
    public string SideWeaponName;
    [Space, HideInInspector]
    public aWeapon FirstWeaponStats, SideWeaponStats;

    [Header("Armure")]
    public string ArmorName;
    [Space, HideInInspector]
    public anArmor myArmor;

    [Header("Blocage")]
    [HideInInspector]
    public bool InBlocage, AlreadyBlocked;
    [HideInInspector]
    public int BlocageIndex;
    aWeapon BlockWeapon;
    [HideInInspector]
    public List<Case> MyBlockedCases;
    FightEntity BlockedEntity;

    [Header("UI elements")]
    public Sprite Portrait;
    public Sprite PortraitZoom;
    [HideInInspector]
    public InitiativeCadre myInitiativeCadre;
    [HideInInspector]
    public FollowingBar myFollowingBar;

    [Header("Statistiques")]
    public int Hp;
    public int MaxHp;
    [Space]
    public int Armor;
    public int maxArmor;
    [Space]
    public int Resistance;
    public int Parade;
    public int Esquive;
    [Space]
    public int Energy;
    public int EnergyGain;
    public int MaxEnergy;
    [Space]
    public int Tranchant;
    public int Perforant;
    public int Magique;
    public int Choc;
    public int FrappeHeroique;
    [Space]
    public int Speed;
    public int RemainingMovement;
    [Space]
    public int InitiativeSpeed = 0;
    [HideInInspector]
    public int ActualInitiative;
    [HideInInspector]
    public int MaxInitiative = 1000;
    [HideInInspector]
    public int BaseHp, BaseArmor, BaseResistance, BaseParade, BaseEsquive, BaseEnergyGain, BaseTranchant, BasePerforant, BaseMagique, BaseChoc, BaseFrappeHeroique, BaseSpeed, BaseInitiativeSpeed;

    [Header("Competences")]
    [HideInInspector] public int ClasseDiviser = 1;
    public List<aCompetence> myCompetences;
    [HideInInspector]
    public List<aCompetence> usableCompetences;
    GameObject BoutonCompPath;

    [Header("States")]
    int BeforeTurnStatesIndex;
    bool Interrupted;
    [HideInInspector]
    public float TempsCoolBeforeTurn;
    public List<aState> ActiveStates;

    [Header("Animations"), HideInInspector]
    public Animator myAnimator;

    [Header("Pattern Attaqué")]
    public aCompetence UsedCompetence;
    public List<Case> AttackedPattern;

    public void Activation(string NameCode)
    {
        #region LoadCharacter
        AFighterSave mySave = LoadSavedCharacter(NameCode);

        Nom = mySave.Nom;
        HeroType = mySave.HeroType;
        Description = mySave.Description;
        myClasse = mySave.myClasse;
        myAlignement = mySave.myAlignement;

        // Current Weapons

        FirstWeaponName = mySave.FirstWeaponName;
        SideWeaponName = mySave.SideWeaponName;


        // Stats & Comps

        Hp = mySave.Hp;
        MaxHp = mySave.MaxHp;
        maxArmor = mySave.maxArmor;

        Resistance = mySave.Resistance;
        Parade = mySave.Parade;
        Esquive = mySave.Esquive;

        Energy = 5;
        MaxEnergy = mySave.MaxEnergy;
        EnergyGain = mySave.EnergyGain;

        Tranchant = mySave.Tranchant;
        Perforant = mySave.Perforant;
        Magique = mySave.Magique;
        Choc = mySave.Choc;
        FrappeHeroique = mySave.FrappeHeroique;

        Speed = mySave.Speed;
        InitiativeSpeed = mySave.InitiativeSpeed;

        myCompetences.Clear();
        for(int i = 0; i < mySave.myNativeCompetences.Count; i++)
        {
            myCompetences.Add(Resources.Load<aCompetence>("Competences/" + mySave.myNativeCompetences[i]));
        }

        Portrait = Resources.Load<Sprite>(mySave.Path + "/Portrait");
        PortraitZoom = Resources.Load<Sprite>(mySave.Path + "/Portrait_Zoom");

        LoadSkin(mySave);

        #endregion

        PM = PlayerManager.Instance;
        theGrid = TheGrid.Instance;
        TM = TurnManager.Instance;
        FC = FightCamera.Instance;
        thePathfinding = Pathfinding.Instance;
        CM = CaseManager.Instance;
        CUI = CompétencesUI.Instance;
        DM = DialogueManager.Instance;
        ID = InitiativeDisplayer.Instance;
        if (!TM.Reinforcements.Contains(this))
        {
            ID.GetACadre(this);
            GetAFollowingBar();
        }

        myWeaponFX = GetComponentInChildren<WeaponFX>();

        myAnimator = GetComponentInChildren<Animator>();

        BoutonCompPath = Resources.Load<GameObject>("UI/Button_aCompetence");

        switch (myClasse)
        {
            case Classe.Hero:
                ClasseDiviser = 1;
                break;
            case Classe.Soldier:
                ClasseDiviser = 4;
                break;
            default:
                break;
        }

        LoadWeapons();

        if(myAlignement != Alignement.Membre)
        {
            myAI = gameObject.AddComponent<anAI>();
        }

        #region Attribution des bases
        BaseHp = Hp;
        BaseArmor = Armor;
        BaseResistance = Resistance;
        BaseParade = Parade;
        BaseEsquive = Esquive;
        BaseEnergyGain = EnergyGain;
        BaseTranchant = Tranchant;
        BasePerforant = Perforant;
        BaseMagique = Magique;
        BaseChoc = Choc;
        BaseFrappeHeroique = FrappeHeroique;
        BaseSpeed = Speed;
        BaseInitiativeSpeed = InitiativeSpeed;
        #endregion

        DM.IdentitiesName.Add(Nom);
        DM.IdentitiesAlignement.Add(myAlignement);
        DM.IdentitiesPortrait.Add(Portrait);
    }

    #region Loads

    void LoadSkin(AFighterSave mySave)
    {
        SpriteRenderer Corps = transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>(); Corps.sprite = Resources.Load<Sprite>(mySave.Path + "/Corps");
        SpriteRenderer Tete = Corps.transform.GetChild(0).GetComponent<SpriteRenderer>(); Tete.sprite = Resources.Load<Sprite>(mySave.Path + "/Tête");
        SpriteRenderer Epaule1 = Corps.transform.GetChild(1).GetComponent<SpriteRenderer>(); Epaule1.sprite = Resources.Load<Sprite>(mySave.Path + "/Epaule");
        SpriteRenderer Bras1 = Epaule1.transform.GetChild(0).GetComponent<SpriteRenderer>(); Bras1.sprite = Resources.Load<Sprite>(mySave.Path + "/Bras");
        SpriteRenderer Main1 = Bras1.transform.GetChild(0).GetComponent<SpriteRenderer>(); Main1.sprite = Resources.Load<Sprite>(mySave.Path + "/Main");
        SpriteRenderer Epaule2 = Corps.transform.GetChild(2).GetComponent<SpriteRenderer>(); Epaule2.sprite = Resources.Load<Sprite>(mySave.Path + "/Epaule");
        SpriteRenderer Bras2 = Epaule2.transform.GetChild(0).GetComponent<SpriteRenderer>(); Bras2.sprite = Resources.Load<Sprite>(mySave.Path + "/Bras");
        SpriteRenderer Main2 = Bras2.transform.GetChild(0).GetComponent<SpriteRenderer>(); Main2.sprite = Resources.Load<Sprite>(mySave.Path + "/Main");
    }
    void LoadWeapons()
    {
        FirstWeaponStats = Resources.Load<aWeapon>("Weapons/WeaponLoaders/" + FirstWeaponName);

        ApplyWeaponBonusMalus(FirstWeaponStats);
        if (FirstWeaponStats.myAnimator != null)
        {
            myAnimator.runtimeAnimatorController = FirstWeaponStats.myAnimator;
        }

        GameObject myNewWeaponRight = Instantiate(FirstWeaponStats.myGameObject, FirstWeaponTransform);
        GetComponentInChildren<WeaponFX>().TR = myNewWeaponRight.transform.GetChild(0).GetComponent<TrailRenderer>();
        GetComponentInChildren<WeaponFX>().PS = myNewWeaponRight.transform.GetChild(0).GetComponent<ParticleSystem>();
        BlockWeapon = FirstWeaponStats;

        List <aCompetence> newComps = myCompetences.Union(FirstWeaponStats.LinkedComps).ToList();
        
        myCompetences = newComps;

        if (SideWeaponName != "")
        {
            SideWeaponStats = Resources.Load<aWeapon>("Weapons/WeaponLoaders/" + SideWeaponName);

            ApplyWeaponBonusMalus(SideWeaponStats);
            myAnimator.runtimeAnimatorController = SideWeaponStats.myAnimator;

            GameObject myNewWeaponLeft = Instantiate(SideWeaponStats.myGameObject, SideWeaponTransform);

            List<aCompetence> newComps2 = myCompetences.Union(SideWeaponStats.LinkedComps).ToList();
            myCompetences = newComps2;
        }

        for (int i = 0; i < myCompetences.Count; i++)
        {
            aCompetence myNewComp = CloneOfComp(myCompetences[i]);
            if (FirstWeaponStats && FirstWeaponStats.LinkedComps.Contains(myCompetences[i]))
                myNewComp.LinkedWeapon = FirstWeaponStats;
            else if (SideWeaponStats && SideWeaponStats.LinkedComps.Contains(myCompetences[i]))
                myNewComp.LinkedWeapon = SideWeaponStats;
            myCompetences[i] = myNewComp;
        }

        if(ArmorName != "")
        {
            myArmor = Resources.Load<anArmor>("Armors/" + ArmorName);
            ApplyArmorBonusMalus(myArmor);
        }

        for (int i = 0; i < myCompetences.Count; i++)
            myCompetences[i].myFighter = this;
    }

    void ApplyWeaponBonusMalus(aWeapon toGetFrom)
    {
        Parade = Mathf.Clamp(Parade + toGetFrom.BonusCounter, 0, 300);
        Tranchant = Mathf.Clamp(Tranchant + toGetFrom.BonusTranchant, 0, 300);
        Perforant = Mathf.Clamp(Perforant + toGetFrom.BonusPerforant, 0, 300);
        Magique = Mathf.Clamp(Magique + toGetFrom.BonusMagique, 0, 300);
        Choc = Mathf.Clamp(Choc + toGetFrom.BonusChoc, 0, 300);
        FrappeHeroique = Mathf.Clamp(FrappeHeroique + toGetFrom.BonusFrappeHeroique, 0, 300);
        EnergyGain = Mathf.Clamp(EnergyGain + toGetFrom.EnergyGain, 0, 300);
    }

    void ApplyArmorBonusMalus(anArmor toGetFrom)
    {
        maxArmor += toGetFrom.ArmorValue;
        Armor = maxArmor;
        Resistance = Mathf.Clamp(Resistance + toGetFrom.ResistanceChange, 0, 300);
        Parade = Mathf.Clamp(Parade + toGetFrom.CounterChange, 0, 300);
        Esquive = Mathf.Clamp(Esquive + toGetFrom.DodgeChange, 0, 300);
        Speed += toGetFrom.MovementChange;
    }

    #endregion

    private void Start()
    {
        if (!TM.Reinforcements.Contains(this))
        {
            TM.activeFighters.Add(this);
            OccupyCase();
        }           
        else
            gameObject.SetActive(false);
        
    }

    public void ReinforcementDeploy()
    {
        gameObject.SetActive(true);
        ID.GetACadre(this);
        ID.UpdateGridConstraint();
        GetAFollowingBar();
        TM.activeFighters.Add(this);

        List<Case> myDeployementCases = new List<Case>();
        if (myAlignement == Alignement.Ennemi)
            myDeployementCases = CM.CasesDEnemy_Reinforcement;
        else
            myDeployementCases = CM.CasesDAllied_Reinforcement;

        for(int i = myDeployementCases.Count - 1; i >= 0; i--)
        {
            if (myDeployementCases[i].EntityOnTop || !myDeployementCases[i].Walkable)
                myDeployementCases.RemoveAt(i);
        }

        if(myDeployementCases.Count > 0)
        {
            int rnd = Random.Range(0, myDeployementCases.Count);
            transform.position = myDeployementCases[rnd].transform.position;

            OccupyCase();

            FC.target = transform;
        }

        TM.Reinforcements.Remove(this);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && IsPlaying)
        {
            EndTurn();
        }

        if (mySituation == Situation.BeforeTurn && Interrupted && TempsCoolBeforeTurn < 1)
        {
            TempsCoolBeforeTurn += Time.deltaTime;
            if(TempsCoolBeforeTurn >= 1)
            {
                TempsCoolBeforeTurn = 0;
                BeforeTurn();
            }
        }
    }

    public void LookMovement(Vector3 PositionToWatch)
    {
        Vector3 relative = transform.InverseTransformPoint(PositionToWatch);
        float angle = Mathf.Atan2(relative.x, relative.y) * Mathf.Rad2Deg;
        transform.Rotate(0, 0, -angle);
    }

    #region Pathfinding

    public void OnPathFound(Vector3[] newPath, bool pathSuccessful, int Distance)
    {
        if (pathSuccessful)
        {
            path = newPath;
            targetIndex = 0;
            CM.ResetCases();
            //RemainingMovement -= Distance;
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }

    IEnumerator FollowPath()
    {
        if (mySituation != Situation.Move)
        {
            mySituation = Situation.Move;
            myAnimator.SetTrigger("Move");
        }
        Vector3 currentWaypoint = path[0];
        Case CurrentCase = OccupiedCase;
        LeaveCase();
        LookMovement(currentWaypoint);

        if (theGrid.NodeFromWorldPoint(transform.position).myCase.BlockedByEnemy(this) && !AlreadyBlocked)
        {
            OccupiedCase = theGrid.NodeFromWorldPoint(transform.position).myCase;
            OccupiedCase.Occupied(this);
            PausePath();
            BlocageLoop(theGrid.NodeFromWorldPoint(transform.position).myCase);
        }
        else if (AlreadyBlocked)
        {
            AlreadyBlocked = false;
        }

        while (true)
        {
            if(Vector3.Distance(transform.position, theGrid.NodeFromWorldPoint(transform.position).myCase.transform.position) <= 0.2f && theGrid.NodeFromWorldPoint(transform.position).myCase != CurrentCase)
            {
                CurrentCase = theGrid.NodeFromWorldPoint(transform.position).myCase;
                RemainingMovement -= 1;

                if (Vector3.Distance(transform.position, currentWaypoint) <= 0.2f)
                {
                    targetIndex++;

                    if(targetIndex < path.Length)
                    {
                        currentWaypoint = path[targetIndex];
                        LookMovement(currentWaypoint);
                    }
                }
               
                if (targetIndex < path.Length)
                {

                    if (theGrid.NodeFromWorldPoint(transform.position).myCase.BlockedByEnemy(this) && !AlreadyBlocked)
                    {
                        OccupiedCase = theGrid.NodeFromWorldPoint(transform.position).myCase;
                        OccupiedCase.Occupied(this);
                        PausePath();
                        BlocageLoop(theGrid.NodeFromWorldPoint(transform.position).myCase);
                        StopCoroutine("FollowPath");
                        break;
                    }

                    else 
                    {
                        if (AlreadyBlocked)
                            AlreadyBlocked = false;
                        theGrid.NodeFromWorldPoint(transform.position).myCase.TakeEffects(this, true);
                    }
                }

                if (targetIndex >= path.Length || RemainingMovement <= 0)
                {
                    targetIndex = 0;
                    path = new Vector3[0];

                    InteruptPath();
                    OccupyCase();
                    mySituation = Situation.ChooseMove;
                    if (myAI)
                        myAI.NeedsToBeCooled = true;
                    yield break;
                }
            }

            transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, speed * Time.deltaTime);
            yield return null;
        }
    }

    public void InteruptPath()
    {
        path = new Vector3[0];
        targetIndex = 0;
        StopCoroutine("FollowPath");

        myAnimator.SetTrigger("Idle");
    }

    public void PausePath()
    {
        Vector3[] newPath = new Vector3[path.Length - targetIndex];
        for (int i = 0; i < path.Length - targetIndex; i++)
        {
            newPath[i] = path[i + targetIndex];
        }

        path = newPath;
        StopCoroutine("FollowPath");
    }

    // Section Poussée

    public void CalculatePush(Case CenterCase, int Portée)
    {
        Vector2 Direction = (OccupiedCase.PointInNode - CenterCase.PointInNode).normalized;
        List<Node> CasesToReach = new List<Node>();
        for (int i = 0; i < Portée; i++)
        {
            Vector2 NextCase = OccupiedCase.PointInNode + (Direction * (i + 1));
            if ((NextCase.x >= 0 && NextCase.x < theGrid.gridWorldSize.x / 2) && (NextCase.y >= 0 && NextCase.y < theGrid.gridWorldSize.y / 2))
            {
                Node theNode = theGrid.grid[(int)NextCase.x, (int)NextCase.y];
                if(theNode.walkable && !theNode.myCase.EntityOnTop)
                {
                    CasesToReach.Add(theNode);
                }
                else
                {
                    break;
                }
            }
        }

        if(CasesToReach.Count > 0)
        {
            LeaveCase();
            StartCoroutine(FollowPush(CasesToReach));
        }
    }

    IEnumerator FollowPush(List<Node> NodesToReach)
    {
        int PushIndex = 0;

        Vector3 currentWaypoint = NodesToReach[PushIndex].worldPosition;

        while (true)
        {
            if (Vector3.Distance(transform.position, currentWaypoint) <= 0.1f)
            {
                PushIndex++;
                if (PushIndex >= NodesToReach.Count)
                {
                    InteruptPath();
                    OccupyCase();
                    yield break;
                }

                currentWaypoint = NodesToReach[PushIndex].worldPosition;
            }
            transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, speed * 5 * Time.deltaTime);
            yield return null;
        }
    }

    #endregion

    #region Case

    public void OccupyCase()
    {
        OccupiedCase = theGrid.NodeFromWorldPoint(transform.position).myCase;
        for (int i = 0; i < ActiveStates.Count; i++)
        {
            if (ActiveStates[i].isDependantOfACaseState)
            {
                bool NeedsToGo = true;

                for(int a = 0; a < OccupiedCase.myEffects.Count; a++)
                {
                    if(OccupiedCase.myEffects[a].name == ActiveStates[i].Dependance)
                    {
                        NeedsToGo = false;
                        break;
                    }
                }

                if (NeedsToGo)
                {
                    ActiveStates[i].ATurnLess(this, true);
                }
            }
        }
        OccupiedCase.Occupied(this);
        transform.position = OccupiedCase.transform.position;
        if(TM.myFS == FightSituation.Fight)
            ActualizeMovement();
    }

    public void LeaveCase()
    {
        OccupiedCase.StopOccupation();
        OccupiedCase = null;
    }

    #endregion

    #region Turn

    public void BeforeTurn()
    {
        if (TM.AreBothSideAlive())
        {
            if (mySituation != Situation.BeforeTurn)
            {
                if (myAlignement == Alignement.Membre)
                {
                    PM.actualEntity = this;
                    FC.QuickFollow(transform);
                }
                else
                {
                    FC.NewFollow(transform);
                }
                mySituation = Situation.BeforeTurn;
            }

            Interrupted = false;

            for (int i = BeforeTurnStatesIndex; i < ActiveStates.Count; i++)
            {
                if (ActiveStates[i].myStateType == StateType.StartTurnDamageGiver)
                {
                    StateHitTaken(ActiveStates[i]);
                    BeforeTurnStatesIndex = i + 1;
                    Interrupted = true;
                    break;
                }
            }

            if (!Interrupted)
            {
                for (int i = 0; i < ActiveStates.Count; i++)
                {
                    ActiveStates[i].ATurnLess(this);
                }
                mySituation = Situation.None;
                BeforeTurnStatesIndex = 0;
                StartTurn();
            }
        }
        else
        {
            TM.EndFightSituation();
        }
    }

    public void StartTurn()
    {
        BeforeTurnStatesIndex = 0;
        RemainingMovement = Speed;

        Energy += EnergyGain;
        Energy = Mathf.Clamp(Energy, 0, MaxEnergy);

        ActualizeMovement();

        IsPlaying = true;

        PM.PlayingApercu.ActualizeShowed(this);

        if(myAlignement == Alignement.Membre)
        {
            foreach (Transform child in CUI.transform)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < myCompetences.Count; i++)
            {
                GameObject newButton = Instantiate(BoutonCompPath, CUI.transform);
                newButton.GetComponent<CompetenceButton>().Activate(myCompetences[i]);
                if (myCompetences[i].EnergyCost <= Energy)
                    newButton.GetComponent<Button>().interactable = true;
                else
                    newButton.GetComponent<Button>().interactable = false;
            }
        }

        if (myAI)
            myAI.NeedsToBeCooled = true;
    }

    public void EndTurn()
    {
        CM.ResetCases();
        IsPlaying = false;
        if(myAlignement == Alignement.Membre)
        {
            PM.actualEntity = null;
        }

        foreach (Transform child in CUI.transform)
        {
            Destroy(child.gameObject);
        }
        PM.PlayingApercu.ActualizeShowed(null);
        TM.EndTurn();
    }

    public void ActualizeMovement()
    {
        if (TM.AreBothSideAlive())
        {
            CM.MovementClickableCases.Clear();

            bool ThereIsDialogue = TM.VerifyEventDialogues();
            if (!ThereIsDialogue)
            {
                mySituation = Situation.ChooseMove;

                List<Node> myNodes = thePathfinding.ReachableNodes(transform, RemainingMovement);

                for (int i = 0; i < myNodes.Count; i++)
                {
                    CM.MovementClickableCases.Add(myNodes[i].myCase);
                    if (myAlignement == Alignement.Membre)
                        myNodes[i].myCase.MakeWalkable(true);
                }

                if (myAlignement == Alignement.Membre)
                {
                    foreach (Transform child in CUI.transform)
                    {
                        Destroy(child.gameObject);
                    }

                    usableCompetences = ActualizedUsableComps();

                    for (int i = 0; i < myCompetences.Count; i++)
                    {
                        GameObject newButton = Instantiate(BoutonCompPath, CUI.transform);
                        newButton.GetComponent<CompetenceButton>().Activate(myCompetences[i]);
                        if (myCompetences[i].EnergyCost <= Energy)
                            newButton.GetComponent<Button>().interactable = true;
                        else
                            newButton.GetComponent<Button>().interactable = false;
                    }
                }

                if (PM.actualEntity == this)
                {
                    PM.PlayingApercu.ActualizeShowed(this);
                }


                TM.ActualizeBlocage();
            }
        }
        else
        {
            TM.EndFightSituation();
        }
    }

    #endregion

    #region FollowingBar

    public void OpenFollowingBar()
    {
        PM.SelectedApercu.ActualizeShowed(this);
    }

    public void CloseFollowingBar()
    {
        PM.SelectedApercu.ActualizeShowed(PM.selectedEntity);
    }

    #endregion

    #region Attack

    public void LoadAttack(aCompetence LoadedComp)
    {
        CM.ResetCases();
        mySituation = Situation.ChooseAttack;

        List<Node> myNodes = thePathfinding.LoadAttackReachables(OccupiedCase.PointInNode, LoadedComp.SelectableCases, LoadedComp);

        for (int i = 0; i < myNodes.Count; i++)
        {
            CM.AttackClickableCases.Add(myNodes[i].myCase);
            if(!myAI)
                myNodes[i].myCase.MakeSelectableAttack(true);
        }

        UsedCompetence = LoadedComp;

        if (myAI)
            myAI.NeedsToBeCooled = true;
    }

    public void BeginAttack(List<Case> AttackPattern, Case SelectedCase)
    {
        AttackedPattern = AttackPattern;
        mySituation = Situation.Attack;
        CM.ResetCases();
        myAnimator.SetTrigger(UsedCompetence.TriggerName);

        for (int i = 0; i < AttackPattern.Count; i++)
        {
            AttackPattern[i].HighlightAttackCase(false);
        }

        Vector2 ToLook = new Vector2(0, 1);

        switch (SelectedCase.myDirection)
        {
            case AttackDirection.Up:
                ToLook = new Vector2(0, 1);
                break;
            case AttackDirection.Down:
                ToLook = new Vector2(0, -1);
                break;
            case AttackDirection.Right:
                ToLook = new Vector2(1, 0);
                break;
            case AttackDirection.Left:
                ToLook = new Vector2(-1, 0);
                break;
            default:
                break;
        }

        transform.rotation = Quaternion.LookRotation(Vector3.forward, ToLook);

        Energy -= UsedCompetence.EnergyCost;
        SpawnChange("Negative/" + "-" + UsedCompetence.EnergyCost.ToString() + " <sprite=11>");

        FC.NonLivingTarget = FC.GetMiddlePoint(OccupiedCase.transform.position, SelectedCase.transform.position);
    }

    public void AttackHit()
    {
        if (UsedCompetence.myCompetenceType == CompetenceType.Attaque)
        {
            for (int i = 0; i < AttackedPattern.Count; i++)
            {
                if (AttackedPattern[i].EntityOnTop != null)
                {
                    if (InBlocage)
                        BlockedEntity = AttackedPattern[i].EntityOnTop;
                    AttackedPattern[i].EntityOnTop.HitTaken(this);
                }
            }
        }

        else if(UsedCompetence.myCompetenceType == CompetenceType.Téléportation)
        {
            LeaveCase();
            transform.position = AttackedPattern[0].transform.position;
            OccupyCase();
        }

        else if((UsedCompetence.myCompetenceType == CompetenceType.Poussée))
        {
            for (int i = 0; i < AttackedPattern.Count; i++)
            {
                if (AttackedPattern[i].EntityOnTop != null)
                {
                    if (InBlocage)
                        BlockedEntity = AttackedPattern[i].EntityOnTop;
                    AttackedPattern[i].EntityOnTop.PushTaken(this);
                }
            }
        }

        else if ((UsedCompetence.myCompetenceType == CompetenceType.Amélioration))
        {
            for (int i = 0; i < AttackedPattern.Count; i++)
            {
                if (AttackedPattern[i].EntityOnTop != null)
                {
                    if (InBlocage)
                        BlockedEntity = AttackedPattern[i].EntityOnTop;
                    AttackedPattern[i].EntityOnTop.BonusTaken(this);
                }
            }
        }
    }

    public void EndAttack()
    {
        AttackedPattern = new List<Case>();
        UsedCompetence = null;
        CM.ResetCases();
        if (!InBlocage)
        {
            ActualizeMovement();
            mySituation = Situation.ChooseMove;

            PM.PlayingApercu.ActualizeShowed(this);
            if (myAI)
            {
                if (myClasse == Classe.Hero)
                    myAI.NeedsToBeCooled = true;
                else
                    EndTurn();
            }
        }
        else
        {
            InBlocage = false;
            if(BlockedEntity && BlockedEntity == TM.activeFighters[TM.TurnIndex])
                TM.activeFighters[TM.TurnIndex].BlocageLoop(theGrid.NodeFromWorldPoint(TM.activeFighters[TM.TurnIndex].transform.position).myCase);
        }

        BlockedEntity = null;

        FC.NonLivingTarget = Vector3.zero;
    }

    public List<aCompetence> ActualizedUsableComps()
    {
        usableCompetences.Clear();
        List<aCompetence> ToGive = new List<aCompetence>();

        for (int i = 0; i < myCompetences.Count; i++)
        {
            if(myCompetences[i].EnergyCost <= Energy)
            {
                ToGive.Add(myCompetences[i]);
            }
        }

        return ToGive;
    }

    #endregion

    #region Hited

    public void HitTaken(FightEntity Enemy)
    {
        float FHMultiplier = 1;

        float DodgeRoll = Random.Range(1, 100);
        bool Dodged = false;

        float ParadeRoll = Random.Range(1, 100);
        bool Pared = false;

        if (DodgeRoll <= Esquive && AlreadyBlocked)
        {
            myAnimator.SetTrigger("Dodge");
            Dodged = true;
            FHMultiplier = 0;
        }
        else if(ParadeRoll <= Parade && !AlreadyBlocked)
        {
            myAnimator.SetTrigger("Parade");
            Pared = true;
            FHMultiplier = 0;
        }
        else
        {
            myAnimator.SetTrigger("Hited");
        }

        if (Random.Range(0f, 100f) <= Enemy.FrappeHeroique)
            FHMultiplier = 2;

        int theHit = Mathf.RoundToInt(Random.Range(Enemy.UsedCompetence.MinDegats, Enemy.UsedCompetence.MaxDegats) / Enemy.ClasseDiviser * FHMultiplier);
        theHit += Mathf.RoundToInt((theHit * HitBonuses(Enemy)) / 100);
        theHit -= Mathf.RoundToInt(theHit * (Resistance / 100));
        int ToDistribute = theHit;
        int ArmorHit = 0;

        if (Armor > 0)
        {
            ArmorHit = Mathf.Clamp(ToDistribute, 0, Armor);
            Armor -= ArmorHit;
            ToDistribute -= ArmorHit;
        }

        int HpHit = ToDistribute;
        Hp -= HpHit;

        List<string> Displays = new List<string>();

        if(theHit > 0)
        {
            if (FHMultiplier > 1)
                Displays.Add("Negative/" + "Critical Hit <sprite=8>");
            if (ArmorHit > 0)
                Displays.Add("Negative/" + "-" + ArmorHit + " <sprite=3>");
            if (HpHit > 0)
                Displays.Add("Negative/" + "-" + HpHit + " <sprite=0>");

            myFollowingBar.BarChange();
            myInitiativeCadre.UpdateBars();
        }
        else if (Dodged)
        {
            Displays.Add("Positive/" + "Dodged <sprite=4>");
        }
        else if (Pared)
        {
            Displays.Add("Positive/" + "Blocked <sprite=2>");
        }
        else
        {
            Displays.Add("Positive/" + "Absorbed <sprite=1>");
        }

        if (!Dodged && !Pared)
        {
            for (int i = 0; i < Enemy.UsedCompetence.AppliedStates.Count; i++)
            {
                aState NewState = CloneOf(Enemy.UsedCompetence.AppliedStates[i]);

                if (NewState.ConditionAcceptables(this, null, false, Enemy.UsedCompetence.WeaponFilter))
                {
                    if (!isAStateAlreadyInList(NewState, ActiveStates))
                    {
                        ActiveStates.Add(NewState);
                        NewState.Activation(this, FirstWeaponTransform.gameObject.GetComponentInChildren<SpriteRenderer>(), Enemy.UsedCompetence);
                    }
                    else
                    {
                        AlreadyIncludedState(NewState, ActiveStates).ActiveTurns = NewState.ActiveTurns;
                    }
                    Displays.Add(NewState.Positiveness + "/" + NewState.name + " " + NewState.GlyphPath);
                }
            }

            if (Enemy.UsedCompetence.InitiativeDegats > 0)
            {
                ActualInitiative -= Mathf.RoundToInt(1000 * (Enemy.UsedCompetence.InitiativeDegats / 100));
                ActualInitiative = Mathf.Clamp(ActualInitiative, 0, 1000);
                ID.UpdateCadres(TM.activeFighters[TM.TurnIndex]);
                Displays.Add("Negative/" + "-" + Enemy.UsedCompetence.InitiativeDegats + "% of Initiative");
            }

            if (Hp <= 0)
            {
                PrepareDeath();
            }
        }

        SpawnChange(Displays);
    }

    public void PushTaken(FightEntity Enemy)
    {
        float FHMultiplier = 1;

        float DodgeRoll = Random.Range(1, 100);
        bool Dodged = false;

        float ParadeRoll = Random.Range(1, 100);
        bool Pared = false;

        if (DodgeRoll <= Esquive && AlreadyBlocked && Enemy.UsedCompetence.MaxDegats > 0)
        {
            myAnimator.SetTrigger("Dodge");
            Dodged = true;
            FHMultiplier = 0;
        }
        else if (ParadeRoll <= Parade && !AlreadyBlocked && Enemy.UsedCompetence.MaxDegats > 0)
        {
            myAnimator.SetTrigger("Parade");
            Pared = true;
            FHMultiplier = 0;
        }
        else
        {
            myAnimator.SetTrigger("Hited");
        }

        if (Random.Range(0f, 100f) <= Enemy.FrappeHeroique)
            FHMultiplier = 2;

        int theHit = Mathf.RoundToInt(Random.Range(Enemy.UsedCompetence.MinDegats, Enemy.UsedCompetence.MaxDegats) / Enemy.ClasseDiviser * FHMultiplier);
        theHit -= Mathf.RoundToInt(theHit * Resistance);
        theHit += Mathf.RoundToInt((theHit * HitBonuses(Enemy)) / 100);
        int ToDistribute = theHit;
        int ArmorHit = 0;

        if (Armor > 0)
        {
            ArmorHit = Mathf.Clamp(ToDistribute, 0, Armor);
            Armor -= ArmorHit;
            ToDistribute -= ArmorHit;
        }

        int HpHit = ToDistribute;
        Hp -= HpHit;

        List<string> Displays = new List<string>();

        if (theHit > 0)
        {
            if (FHMultiplier > 1)
                Displays.Add("Negative/" + "Critical Hit <sprite=8>");
            if (ArmorHit > 0)
                Displays.Add("Negative/" + "-" + ArmorHit + " <sprite=3>");
            if (HpHit > 0)
                Displays.Add("Negative/" + "-" + HpHit + " <sprite=0>");

            myFollowingBar.BarChange();
            myInitiativeCadre.UpdateBars();
        }
        else if (Enemy.UsedCompetence.MaxDegats > 0)
        {
            if (Dodged)
            {
                Displays.Add("Positive/" + "Dodged <sprite=4>");
            }
            else if (Pared)
            {
                Displays.Add("Positive/" + "Blocked <sprite=4>");
            }
            else
            {
                Displays.Add("Positive/" + "Absorbed <sprite=1>");
            }
        }

        if (!Dodged && !Pared)
        {
            for (int i = 0; i < Enemy.UsedCompetence.AppliedStates.Count; i++)
            {
                aState NewState = CloneOf(Enemy.UsedCompetence.AppliedStates[i]);

                if (NewState.ConditionAcceptables(this, null, false, Enemy.UsedCompetence.WeaponFilter))
                {
                    if (!isAStateAlreadyInList(NewState, ActiveStates))
                    {
                        ActiveStates.Add(NewState);
                        NewState.Activation(this, FirstWeaponTransform.gameObject.GetComponentInChildren<SpriteRenderer>(), Enemy.UsedCompetence);
                    }
                    else
                    {
                        AlreadyIncludedState(NewState, ActiveStates).ActiveTurns = NewState.ActiveTurns;
                    }
                    Displays.Add(NewState.Positiveness + "/" + NewState.name + " " + NewState.GlyphPath);
                }
            }

            if (Enemy.UsedCompetence.InitiativeDegats > 0)
            {
                ActualInitiative -= Mathf.RoundToInt(1000 * (Enemy.UsedCompetence.InitiativeDegats / 100));
                ActualInitiative = Mathf.Clamp(ActualInitiative, 0, 1000);
                ID.UpdateCadres(TM.activeFighters[TM.TurnIndex]);
                Displays.Add("Negative/" + "-" + Enemy.UsedCompetence.InitiativeDegats + "% of Initiative");
            }
        }

        if (Hp <= 0)
        {
            PrepareDeath();
        }

        SpawnChange(Displays);
        CalculatePush(Enemy.OccupiedCase, Enemy.UsedCompetence.Poussée);
    }

    public void BonusTaken(FightEntity Enemy)
    {
        List<string> Displays = new List<string>();

        for (int i = 0; i < Enemy.UsedCompetence.AppliedStates.Count; i++)
        {
            aState NewState = CloneOf(Enemy.UsedCompetence.AppliedStates[i]);

            if (NewState.ConditionAcceptables(this, null, false, Enemy.UsedCompetence.WeaponFilter))
            {
                if (!isAStateAlreadyInList(NewState, ActiveStates))
                {
                    ActiveStates.Add(NewState);
                    NewState.Activation(this, FirstWeaponTransform.gameObject.GetComponentInChildren<SpriteRenderer>(), Enemy.UsedCompetence);
                }
                else
                {
                    AlreadyIncludedState(NewState, ActiveStates).ActiveTurns = NewState.ActiveTurns;
                }
                Displays.Add(NewState.Positiveness + "/" + NewState.name + " " + NewState.GlyphPath);
            }
        }

        if (Hp <= 0)
        {
            PrepareDeath();
        }

        SpawnChange(Displays);
    }

    public void StateHitTaken(aState HitingState)
    {
        myAnimator.SetTrigger("Hited");

        float FHMultiplier = 1;

        int theHit = HitingState.InflictedDamages;
        theHit -= Mathf.RoundToInt(theHit * (Resistance / 100));
        int ToDistribute = theHit;
        int ArmorHit = 0;

        if (Armor > 0)
        {
            ArmorHit = Mathf.Clamp(ToDistribute, 0, Armor);
            Armor -= ArmorHit;
            ToDistribute -= ArmorHit;
        }

        int HpHit = ToDistribute;
        Hp -= HpHit;

        List<string> Displays = new List<string>();

        if (theHit > 0)
        {
            if (FHMultiplier > 1)
                Displays.Add("Negative/" + "Critical Hit <sprite=8>");
            if (ArmorHit > 0)
                Displays.Add("Negative/" + "-" + ArmorHit + " <sprite=3>");
            if (HpHit > 0)
                Displays.Add("Negative/" + "-" + HpHit + " <sprite=0>");

            myFollowingBar.BarChange();
            myInitiativeCadre.UpdateBars();
        }
        else
        {
            Displays.Add("Positive/" + "Absorbed <sprite=1>");
        }

        if (Hp <= 0)
        {
            PrepareDeath();
        }

        SpawnChange(Displays);
    }

    public void PrepareDeath()
    {
        GameObject myAnim = Instantiate(Resources.Load<GameObject>("UI/aKillAlert"), ID.transform);
        myAnim.GetComponentInChildren<KillAnimation>().Activation(this);
        TM.RemoveAnEntity(this);

        if (OccupiedCase)
            LeaveCase();
    }

    public void Death()
    {
        GenerateBlockZone(false);

        myInitiativeCadre.Remove();

        if(PM.selectedEntity == this)
        {
            PM.selectedEntity = null;
            CloseFollowingBar();
        }

        Destroy(gameObject);
    }

    public int HitBonuses(FightEntity Enemy, aCompetence ForcedComp = null)
    {
        aCompetence theCompetence = ForcedComp;
        if (!ForcedComp)
            theCompetence = Enemy.UsedCompetence;

        int PercentageToReturn = 0;

        if(theCompetence.myAttaqueType == AttaqueType.Tranchant)
        {
            PercentageToReturn += Enemy.Tranchant;
        }
        else if (theCompetence.myAttaqueType == AttaqueType.Perforant)
        {
            PercentageToReturn += Enemy.Perforant;
        }
        else if (theCompetence.myAttaqueType == AttaqueType.Magique)
        {
            PercentageToReturn += Enemy.Magique;
        }
        else if (theCompetence.myAttaqueType == AttaqueType.Choc)
        {
            PercentageToReturn += Enemy.Choc;
        }

        return PercentageToReturn;
    }

    #endregion

    #region Blocage

    public void GenerateBlockZone(bool DoIt)
    {
        if (BlockWeapon.MeleeWeapon && BlockWeapon.LinkedComps.Count > 0 && DoIt)
        {
            Node PlayingNode = null;
            if(TM.TurnIndex < TM.activeFighters.Count)
            {
                PlayingNode = theGrid.grid[(int)TM.activeFighters[TM.TurnIndex].OccupiedCase.PointInNode.x, (int)TM.activeFighters[TM.TurnIndex].OccupiedCase.PointInNode.y];
            }

            List<Node> myNodes = thePathfinding.LoadAttackReachables(OccupiedCase.PointInNode, BlockWeapon.LinkedComps[0].SelectableCases, BlockWeapon.LinkedComps[0], true, PlayingNode);

            for (int i = 0; i < myNodes.Count; i++)
            {
                MyBlockedCases.Add(myNodes[i].myCase);
                myNodes[i].myCase.AddBloqueur(this, true);
            }

            ShowZone(DoIt);
        }
        else if (!DoIt)
        {
            ShowZone(DoIt);

            for (int i = 0; i < MyBlockedCases.Count; i++)
            {
                MyBlockedCases[i].AddBloqueur(this, false);
            }

            MyBlockedCases = new List<Case>();
        }
    }

    public void ShowZone(bool DoIt)
    {
        // && myAlignement == Alignement.Ennemi
        if (DoIt && TM.TurnIndex < TM.activeFighters.Count && TM.activeFighters[TM.TurnIndex].myAlignement == Alignement.Membre )
        {
            for (int i = 0; i < MyBlockedCases.Count; i++)
            {
                //if (!MyBlockedCases[i].EntityOnTop)
                MyBlockedCases[i].HighlightBlocage(true);
            }
        }
        else
        {
            for (int i = 0; i < MyBlockedCases.Count; i++)
            {
                //if (!MyBlockedCases[i].EntityOnTop)
                MyBlockedCases[i].HighlightBlocage(false);
            }
        }
    }

    public void BlocageAttack(Case CaseToHit)
    {
        InBlocage = true;

        List<Node> myNodes = thePathfinding.LoadAttackReachables(OccupiedCase.PointInNode, BlockWeapon.LinkedComps[0].SelectableCases, BlockWeapon.LinkedComps[0]);

        List<Case> OnlyCase = new List<Case>();
        OnlyCase.Add(CaseToHit);
        AttackedPattern = OnlyCase;

        UsedCompetence = BlockWeapon.LinkedComps[0];

        for(int i = 0; i < myCompetences.Count; i++)
        {
            if(UsedCompetence.Name == myCompetences[i].Name)
            {
                UsedCompetence = myCompetences[i];
            }
        }

        myAnimator.SetTrigger(UsedCompetence.TriggerName);

        Vector2 ToLook = new Vector2(0, 1);

        switch (CaseToHit.myDirection)
        {
            case AttackDirection.Up:
                ToLook = new Vector2(0, 1);
                break;
            case AttackDirection.Down:
                ToLook = new Vector2(0, -1);
                break;
            case AttackDirection.Right:
                ToLook = new Vector2(1, 0);
                break;
            case AttackDirection.Left:
                ToLook = new Vector2(-1, 0);
                break;
            default:
                break;
        }

        transform.rotation = Quaternion.LookRotation(Vector3.forward, ToLook);
    }

    void BlocageLoop(Case CaseToHit)
    {
        if(CaseToHit.Bloqueurs.Count > BlocageIndex)
        {
            AlreadyBlocked = true;
            if ((myAlignement == Alignement.Ennemi && CaseToHit.Bloqueurs[BlocageIndex].myAlignement != Alignement.Ennemi) 
                || (myAlignement != Alignement.Ennemi && CaseToHit.Bloqueurs[BlocageIndex].myAlignement == Alignement.Ennemi))
            {
                CaseToHit.Bloqueurs[BlocageIndex].BlocageAttack(CaseToHit);
                BlocageIndex += 1;
                myAnimator.SetTrigger("Idle");
                mySituation = Situation.None;
            }
            else
            {
                BlocageIndex += 1;
                BlocageLoop(CaseToHit);
            }            
        }
        else
        {
            BlocageIndex = 0;
            targetIndex = 0;
            CM.ResetCases();
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }

    #endregion

    #region UISpawn

    public void SpawnChange(List<string> WhatToShow)
    {
        for (int i = 0; i < WhatToShow.Count; i++)
        {
            string[] myStrings = WhatToShow[i].Split(char.Parse("/"));
            GameObject theTextMarker = Instantiate(Resources.Load<GameObject>("UI/aChange"), ID.transform.parent);
            theTextMarker.GetComponent<FollowingUI>().theRenderer = GetComponentInChildren<Renderer>();
            theTextMarker.GetComponent<FollowingUI>().CaseSafety = theGrid.NodeFromWorldPoint(transform.position).myCase.GetComponentInChildren<Renderer>();
            theTextMarker.GetComponent<ChangeOnFighter>().Activation(myStrings[1], (float)i, Positiveness(myStrings[0]));
        }
    }

    public void SpawnChange(string WhatToShow)
    {
        string[] myStrings = WhatToShow.Split(char.Parse("/"));
        GameObject theTextMarker = Instantiate(Resources.Load<GameObject>("UI/aChange"), ID.transform.parent);
        theTextMarker.GetComponent<FollowingUI>().theRenderer = GetComponentInChildren<Renderer>();
        theTextMarker.GetComponent<FollowingUI>().CaseSafety = theGrid.NodeFromWorldPoint(transform.position).myCase.GetComponentInChildren<Renderer>();
        theTextMarker.GetComponent<ChangeOnFighter>().Activation(myStrings[1], 0, Positiveness(myStrings[0]));
    }

    int Positiveness(string Result)
    {
        if (Result == "Negative")
            return 0;
        else if (Result == "Positive")
            return 2;
        else
            return 1;
    }

    void GetAFollowingBar()
    {
        GameObject MyBars = Instantiate(Resources.Load<GameObject>("UI/aFollowingBar"), DM.transform.parent.GetChild(0));
        MyBars.GetComponent<FollowingUI>().theRenderer = GetComponentInChildren<SpriteRenderer>();
        myFollowingBar = MyBars.GetComponent<FollowingBar>();

        myFollowingBar.Activation(this);
    }

    #endregion

    #region States

    public bool isAStateAlreadyInList(aState toVerify, List<aState> myStates)
    {
        string VerifyName = toVerify.name;
        bool IsInside = false;
        for (int i = 0; i < myStates.Count; i++)
        {
            if(myStates[i].name == VerifyName)
            {
                IsInside = true;
                break;
            }
        }

        return IsInside;
    }

    public aState AlreadyIncludedState(aState toVerify, List<aState> myStates)
    {
        aState ToReturn = null;
        string theName = toVerify.name;
        for (int i = 0; i < myStates.Count; i++)
        {
            if (theName == myStates[i].name)
            {
                ToReturn = myStates[i];
                break;
            }
        }

        return ToReturn;
    }

    public aState CloneOf(string Path)
    {
        aState myClone = ScriptableObject.CreateInstance<aState>();
        aStateModel myModel = Resources.Load<aStateModel>("Etats/" + Path);

        myClone.name = myModel.name;

        myClone.myClass = myModel.myClass;

        myClone.WeaponFilter = myModel.WeaponFilter;
        myClone.myStateType = myModel.myStateType;
        myClone.Dependance = myModel.Dependance;

        myClone.StatModified = myModel.StatModified;
        myClone.ModifiedOf = myModel.ModifiedOf;
        myClone.InflictedDamages = myModel.InflictedDamages;
        myClone.ActiveTurns = myModel.ActiveTurns;
        myClone.myAnti = myModel.myAnti;

        myClone.EffectGivedOnAttackPath = myModel.EffectGivedOnAttackPath;

        myClone.Logo = myModel.Logo;
        myClone.GlyphPath = myModel.GlyphPath;
        myClone.TipToShowCrypted = myModel.TipToShowCrypted;
        myClone.Positiveness = myModel.Positiveness;

        myClone.Effect = myModel.Effect;
        myClone.GraphicEffectAppliedOn = myModel.GraphicEffectAppliedOn;

        return myClone;
    }

    aCompetence CloneOfComp(aCompetence CompModel)
    {
        aCompetence myNewComp = ScriptableObject.CreateInstance<aCompetence>();

        myNewComp.Name = CompModel.Name;
        myNewComp.Description = CompModel.Description;

        myNewComp.WeaponFilter = CompModel.WeaponFilter;
        myNewComp.myCompetenceType = CompModel.myCompetenceType;
        myNewComp.EnergyCost = CompModel.EnergyCost;
        myNewComp.myAttaqueType = CompModel.myAttaqueType;

        myNewComp.Anti_Perso = CompModel.Anti_Perso;
        myNewComp.Anti_Ground = CompModel.Anti_Ground;
        myNewComp.Vision_Affected = CompModel.Vision_Affected;
        myNewComp.TargetingAllies = CompModel.TargetingAllies;

        myNewComp.MinDegats = CompModel.MinDegats;
        myNewComp.MaxDegats = CompModel.MaxDegats;
        myNewComp.InitiativeDegats = CompModel.InitiativeDegats;
        myNewComp.ModelStates = CompModel.ModelStates;
        myNewComp.AppliedStates = new List<string>();
        for (int i = 0; i < CompModel.AppliedStates.Count; i++)
        {
            myNewComp.AppliedStates.Add(CompModel.AppliedStates[i]);
            myNewComp.ModelStates.Add(Resources.Load<aStateModel>("Etats/" + myNewComp.AppliedStates[i]));
        }
        myNewComp.Poussée = CompModel.Poussée;

        myNewComp.SelectableCases = CompModel.SelectableCases;
        myNewComp.PaternCase = CompModel.PaternCase;

        myNewComp.Logo = CompModel.Logo;

        myNewComp.TriggerName = CompModel.TriggerName;

        myNewComp.BaseOpportunity = CompModel.BaseOpportunity;
        myNewComp.PatternIsDirectionnal = CompModel.PatternIsDirectionnal;

        return myNewComp;
    }

    #endregion

    #region Save
    public void Save(AFighterSave TheData)
    {
        var saved = JsonUtility.ToJson(TheData);
        string path = Application.streamingAssetsPath + "/SavedCharacters/" + TheData.Nom + ".txt";
        if (TheData.HeroType != "")
            path = Application.streamingAssetsPath + "/SavedCharacters/" + TheData.HeroType + "/#" + SaveIndex + ".txt";
        File.WriteAllText(path, saved);
    }
    public AFighterSave LoadSavedCharacter(string FighterName)
    {
        // Real name for unique characters, type#SaveIndex for others
        string path = "";
        if (FighterName.Contains('#'))
        {
            string ValidName = "";

            for (int i = 0; i < FighterName.Length; i++)
            {
                char ToVerify = FighterName[i];
                if (ToVerify == ' ')
                    ValidName += '_';
                else if (ToVerify == 'é' || ToVerify == 'è')
                    ValidName += 'e';
                else
                    ValidName += ToVerify;
            }

            string[] SplittedName = ValidName.Split(char.Parse("#"));
            path = Application.streamingAssetsPath + "/SavedCharacters/" + SplittedName[0] + "/#" + SplittedName[1] + ".txt";
        }
        else
            path = Application.streamingAssetsPath + "/SavedCharacters/" + FighterName + ".txt";
        string thejson = File.ReadAllText(path);
        AFighterSave loadInto = ScriptableObject.CreateInstance<AFighterSave>();
        JsonUtility.FromJsonOverwrite(thejson, loadInto);

        return loadInto;
    }

    #endregion
}
