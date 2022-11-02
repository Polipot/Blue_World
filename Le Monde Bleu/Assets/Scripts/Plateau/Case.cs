using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Case : MonoBehaviour
{
    [Header("Deploiement")]
    [HideInInspector] public bool Deployable;
    [HideInInspector] public Alignement DeployAlignement;

    [Header("References")]
    CaseManager CM;
    TurnManager TM;

    [Header("Composants")]
    aTip myTip;

    [Header("Sprites Renderers")]
    SpriteRenderer myCaseSprite;
    [HideInInspector] public SpriteRenderer myMarqueurSprite;
    SpriteRenderer myTexture;
    SpriteRenderer myBlockedSprite;
    Color MarqueurBaseColor;

    [Header("Nature et effets")]
    [HideInInspector] public string Nom;
    [HideInInspector] public CaseTypes myTypes;
    [HideInInspector] public List<CaseState> myEffects;
    [Space]
    CaseProperties BurnedProperties;

    [Header("Entité présente")]
    [HideInInspector] public FightEntity EntityOnTop;

    [Header("Blocage")]
    public List<FightEntity> Bloqueurs;

    [Header("Coordonnées")]
    [HideInInspector] public Vector2 PointInNode;
    [HideInInspector] public bool Walkable;
    [HideInInspector] public bool Reachable;
    [HideInInspector] public bool Attackable;
    [HideInInspector] public AttackDirection myDirection;

    public void Occupied(FightEntity newEntity)
    {
        EntityOnTop = newEntity;
        switch (newEntity.myAlignement)
        {
            case Alignement.Membre:
                myCaseSprite.color = CM.OccupiedMember;
                break;
            case Alignement.Allié:
                myCaseSprite.color = CM.OccupiedAllied;
                break;
            case Alignement.Ennemi:
                myCaseSprite.color = CM.OccupiedEnemy;
                break;
        }

        TakeEffects(newEntity);

        ActualizeTip();
    }

    public void TakeEffects(FightEntity newEntity, bool OnlyPassingBy = false, aCompetence UsedComp = null)
    {
        List<string> Displays = new List<string>();

        for (int i = 0; i < myEffects.Count; i++)
        {
            if (myEffects[i].GivedState != "")
            {
                aState NewState = newEntity.CloneOf(myEffects[i].GivedState);

                if (NewState.ConditionAcceptables(newEntity))
                {
                    if (!newEntity.isAStateAlreadyInList(NewState, newEntity.ActiveStates))
                    {
                        newEntity.ActiveStates.Add(NewState);
                        NewState.Activation(newEntity, newEntity.FirstWeaponTransform.gameObject.GetComponentInChildren<SpriteRenderer>(), UsedComp);
                        Displays.Add(NewState.Positiveness + "/" + NewState.name + " " + NewState.GlyphPath);
                    }
                    else
                    {
                        newEntity.AlreadyIncludedState(NewState, newEntity.ActiveStates).ActiveTurns = NewState.ActiveTurns;
                        if(!OnlyPassingBy)
                            Displays.Add(NewState.Positiveness + "/" + NewState.name + " " + NewState.GlyphPath);
                    }
                }
            }
        }

        newEntity.SpawnChange(Displays);
    }

    public void StopOccupation()
    {
        EntityOnTop = null;
        myCaseSprite.color = CM.Base;

        ActualizeTip();
    }

    #region Highlight

    public void HighlightCase(bool start)
    {
        if (start)
        {
            myMarqueurSprite.color = CM.MarqueurHighlighted;
            if (EntityOnTop != null)
            {
                EntityOnTop.OpenFollowingBar();
                EntityOnTop.myFollowingBar.Appear();
            }    
        }
        else
        {
            myMarqueurSprite.color = MarqueurBaseColor;
            if (EntityOnTop != null)
            {
                EntityOnTop.CloseFollowingBar();
                EntityOnTop.myFollowingBar.Disappear();
            }
        }
    }

    public void MakeWalkable(bool start)
    {
        if (start)
        {
            Reachable = true;
            if (BlockedByEnemy(TM.activeFighters[TM.TurnIndex]))
            {
                myMarqueurSprite.color = CM.Blockage;
                MarqueurBaseColor = CM.Blockage;
            }
            else
            {
                myMarqueurSprite.color = CM.MarqueurWalkable;
                MarqueurBaseColor = CM.MarqueurWalkable;
            }
        }
        else
        {
            Reachable = false;
            myMarqueurSprite.color = CM.MarqueurBase;
            MarqueurBaseColor = CM.MarqueurBase;
        }
    }

    public void ManualMovementHighlight(bool start)
    {
        if (start)
        {
            myMarqueurSprite.color = CM.MarqueurManualMovement;
            MarqueurBaseColor = CM.MarqueurManualMovement;
        }
        else
        {
            if (Reachable)
            {
                myMarqueurSprite.color = CM.MarqueurWalkable;
                MarqueurBaseColor = CM.MarqueurWalkable;
            }
            else
            {
                myMarqueurSprite.color = CM.MarqueurBase;
                MarqueurBaseColor = CM.MarqueurBase;
            }
        }
    }

    public void MakeSelectableAttack(bool start)
    {
        if (start)
        {
            Attackable = true;
            myMarqueurSprite.color = CM.MarqueurAttackSelectable;
            MarqueurBaseColor = CM.MarqueurAttackSelectable;
        }
        else
        {
            Attackable = false;
            myMarqueurSprite.color = CM.MarqueurBase;
            MarqueurBaseColor = CM.MarqueurBase;
        }
    }

    public void HighlightAttackCase(bool start, bool Simulate = false)
    {
        if (start)
        {
            myMarqueurSprite.color = CM.MarqueurAttackCovered;
            if (EntityOnTop != null)
            {
                if(Simulate)
                    EntityOnTop.myFollowingBar.SimulateChanges(TM.activeFighters[TM.TurnIndex], TM.activeFighters[TM.TurnIndex].UsedCompetence);
                else
                    EntityOnTop.OpenFollowingBar();

                EntityOnTop.OpenFollowingBar();
            }
        }
        else
        {
            myMarqueurSprite.color = MarqueurBaseColor;
            if (EntityOnTop != null)
            {
                if (!Simulate)
                    EntityOnTop.myFollowingBar.Disappear();
                else
                    EntityOnTop.myFollowingBar.ResetSimulation();

                EntityOnTop.CloseFollowingBar();
            }
        }
    }

    public void HighlightBlocage(bool start)
    {
        if (start)
        {
            //myCaseSprite.color = CM.Blockage;
            Color myColor = CM.Unblocked;
            int BlocageAlign = BlockageAlignement();
            switch (BlocageAlign)
            {
                case 1:
                    myColor = CM.BlockedByAlly;
                    break;
                case 2:
                    myColor = CM.BlockedByEnnemy;
                    break;
                case 3:
                    myColor = CM.BlockedByBoth;
                    break;
            }

            myBlockedSprite.color = myColor;
        }
        else
        {
            //myCaseSprite.color = CM.Base;
            myBlockedSprite.color = CM.Unblocked;
        }
    }

    #endregion

    #region blocage

    public void AddBloqueur(FightEntity FE, bool AddBl)
    {
        if (AddBl && !Bloqueurs.Contains(FE))
        {
            Bloqueurs.Add(FE);
        }
        else if(!AddBl)
        {
            Bloqueurs.Remove(FE);
        }

        ActualizeTip();
    }

    public bool BlockedByEnemy(FightEntity Playing)
    {
        bool toReturn = false;
        for (int i = 0; i < Bloqueurs.Count; i++)
        {
            if((Bloqueurs[i].myAlignement == Alignement.Ennemi && Playing.myAlignement != Alignement.Ennemi) || (Bloqueurs[i].myAlignement != Alignement.Ennemi && Playing.myAlignement == Alignement.Ennemi))
            {
                toReturn = true;
                break;
            }
        }

        return toReturn;
    }

    int BlockageAlignement()
    {
        bool BlockedByEn = false;
        bool BlockedByAl = false;

        for (int i = 0; i < Bloqueurs.Count; i++)
        {
            if (Bloqueurs[i].myAlignement == Alignement.Ennemi)
                BlockedByEn = true;
            else if (Bloqueurs[i].myAlignement != Alignement.Ennemi)
                BlockedByAl = true;

            if (BlockedByAl && BlockedByEn)
                break;
        }

        if (BlockedByEn && BlockedByAl)
            return 3;
        else if (BlockedByEn)
            return 2;
        else if (BlockedByAl)
            return 1;
        else
            return 0;
    }

    #endregion

    #region Traduction

    public void Traduction(CaseProperties myProperties)
    {
        CM = CaseManager.Instance;
        TM = TurnManager.Instance;
        myCaseSprite = GetComponent<SpriteRenderer>();
        myMarqueurSprite = transform.GetChild(0).GetComponent<SpriteRenderer>();
        myTexture = transform.GetChild(1).GetComponent<SpriteRenderer>();
        myBlockedSprite = transform.GetChild(2).GetComponent<SpriteRenderer>();
        Nom = myProperties.CaseName;
        myMarqueurSprite.color = CM.MarqueurBase;
        MarqueurBaseColor = CM.MarqueurBase;
        myTypes = myProperties.myCaseType;

        Walkable = myProperties.isWalkable;
        myTexture.sprite = myProperties.CaseTexture[Random.Range(0, myProperties.CaseTexture.Count)];

        if (myProperties.SpecialMaterial)
            myTexture.material = myProperties.SpecialMaterial;

        if (!Walkable)
            myCaseSprite.color = new Color(0f, 0f, 0f, 0.4f);

        for(int i = 0; i < myProperties.ConstantEffect.Count; i++)
        {
            ApplyNewCaseState(myProperties.ConstantEffect[i], 0, true);
        }

        BurnedProperties = myProperties.Burned; // ??? Hein?

        myTip = GetComponent<aTip>();
        ActualizeTip();
    }

    #endregion

    #region CaseStates

    public CaseState CloneOf(string Path, int HeritageM = 0)
    {
        CaseState myClone = ScriptableObject.CreateInstance<CaseState>();
        CaseStateModel myModel = Resources.Load<CaseStateModel>("CaseStates/" + Path);

        myClone.name = myModel.name;
        myClone.Logo = myModel.Logo;
        myClone.GlyphPath = myModel.GlyphPath;
        myClone.TipToShowCrypted = myModel.TipToShowCrypted;

        myClone.BaseTime = myModel.BaseTime;
        myClone.RemainingTime = myModel.BaseTime;
        myClone.myBehavior = myModel.myBehavior;

        myClone.CaseTypesAllowed = myModel.CaseTypesAllowed;
        myClone.GivedState = myModel.GivedState;

        myClone.Effect = myModel.Effect;

        myClone.BasePPGTChance = myModel.BasePPGTChance;
        myClone.LessPPGT = myModel.LessPPGT;
        myClone.MyPPGTChance = myClone.BasePPGTChance - myClone.LessPPGT * HeritageM;
        myClone.Heritage = HeritageM + 1;

        myClone.Dangerosity = myModel.Dangerosity;

        return myClone;
    }

    public void ApplyNewCaseState(string Path, int HeritageM = 0, bool FromDeployment = false)
    {
        CaseState toApply = CloneOf(Path, HeritageM);
        toApply.myCase = this;

        if ((myTypes.HasFlag(toApply.CaseTypesAllowed) || toApply.CaseTypesAllowed == CaseTypes.Everything) && !DoesEffectsContain(toApply.name))
        {
            myEffects.Add(toApply);
            if (!FromDeployment)
                ActualizeTip();

            if (toApply.Effect != null)
            {
                GameObject newEffect = Instantiate(toApply.Effect, transform);
                toApply.Effect = newEffect;
            }

            if (EntityOnTop && toApply.GivedState != "")
            {
                aState NewState = EntityOnTop.CloneOf(toApply.GivedState);

                if (NewState.ConditionAcceptables(EntityOnTop))
                {
                    if (!EntityOnTop.isAStateAlreadyInList(NewState, EntityOnTop.ActiveStates))
                    {
                        EntityOnTop.ActiveStates.Add(NewState);
                        NewState.Activation(EntityOnTop, EntityOnTop.FirstWeaponTransform.gameObject.GetComponentInChildren<SpriteRenderer>());
                    }
                    else
                    {
                        EntityOnTop.AlreadyIncludedState(NewState, EntityOnTop.ActiveStates).ActiveTurns += NewState.ActiveTurns;
                    }
                    string toDisplay = NewState.Positiveness + "/" + NewState.name + " " + NewState.GlyphPath;
                    EntityOnTop.SpawnChange(toDisplay);
                }
            }
        }
    }

    public void RemoveState(bool isEnd, string Path)
    {
        int myIndex = myEffects.Count;
        for(int i = 0; i < myEffects.Count; i++)
        {
            if(Path == myEffects[i].name)
            {
                myIndex = i;
                break;
            }
        }
        if(myIndex != myEffects.Count)
        {
            if (myEffects[myIndex].Effect != null)
            {
                myEffects[myIndex].Effect.gameObject.GetComponent<AutoDestruction>().DestroyActivated = true;
                ParticleLightSpawner particleLightSpawner = myEffects[myIndex].Effect.gameObject.GetComponent<ParticleLightSpawner>();
                if (particleLightSpawner)
                    particleLightSpawner.LightDeath();
            }
            if (BurnedProperties)
                Traduction(BurnedProperties);
            else
                ActualizeTip();
            myEffects.RemoveAt(myIndex); 
        }
    }

    public bool DoesEffectsContain(string EffectName)
    {
        bool ToReturn = false;
        for(int i = 0; i < myEffects.Count; i++)
        {
            if(myEffects[i].name == EffectName)
            {
                ToReturn = true;
                break;
            }
        }

        return ToReturn;
    }

    #endregion

    #region Deploiement

    public void TriggerDeploiement()
    {
        Deployable = true;
        if (DeployAlignement == Alignement.Allié)
        {
            myMarqueurSprite.color = CM.OccupiedAllied;
            MarqueurBaseColor = myMarqueurSprite.color;
            CM.CasesDAllied.Add(this);
        }            
        else if (DeployAlignement == Alignement.Ennemi)
        {
            myMarqueurSprite.color = CM.OccupiedEnemy;
            MarqueurBaseColor = myMarqueurSprite.color;
            CM.CasesDEnemy.Add(this);
        }
        else if (DeployAlignement == Alignement.Membre)
        {
            myMarqueurSprite.color = CM.OccupiedMember;
            MarqueurBaseColor = myMarqueurSprite.color;
            CM.CasesDMember.Add(this);
        }
    }

    public void UntriggerDeploiement()
    {
        myMarqueurSprite.color = CM.MarqueurBase;
        MarqueurBaseColor = myMarqueurSprite.color;
    }

    public void DeployHere(FightEntity myNewEntity)
    {
        myNewEntity.transform.position = transform.position;
        myNewEntity.OccupyCase();
    }

    #endregion

    #region Tips

    void ActualizeTip()
    {
        myTip.ToShow = "<b>" + Nom + "</b>";

        foreach(CaseState states in myEffects)
        {
            myTip.ToShow += "\n<color=" + GetRIchTextColor(states.Dangerosity) + ">"+ states.name + "</color>";
        }

        if (EntityOnTop)
            myTip.ToShow += "\n<color=" + GetRIchTextColor(EntityOnTop.myAlignement) + ">Occupied by " + EntityOnTop.Nom + "</color>";

        foreach(FightEntity blockers in Bloqueurs)
        {
            myTip.ToShow += "\n<color=" + GetRIchTextColor(blockers.myAlignement) + ">Blocked by " + blockers.Nom + "</color>";
        }
    }

    string GetRIchTextColor(Alignement alignement)
    {
        string myTextColor = "white";
        switch (alignement)
        {
            case Alignement.Membre:
                myTextColor = "green";
                break;
            case Alignement.Allié:
                myTextColor = "#69C0FF";
                break;
            case Alignement.Ennemi:
                myTextColor = "red";
                break;
            default:
                break;
        }

        return myTextColor;
    }

    string GetRIchTextColor(int dangerosity)
    {
        string myTextColor = "white";
        switch (dangerosity)
        {
            case 2:
                myTextColor = "green";
                break;
            case 1:
                myTextColor = "yellow";
                break;
            case 0:
                myTextColor = "red";
                break;
            default:
                break;
        }

        return myTextColor;
    }

    #endregion
}
