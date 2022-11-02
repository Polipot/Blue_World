using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public enum GraphicEffectOn
{
    None = 0,
    Character = 1 << 1,
    Weapon = 2 << 2,
    Everything = ~0,
}

public enum StateClass
{
    None = 0,
    Fire = 1 << 1,
    Water = 2 << 2,
    Curse = 3 << 3,
    Everything = ~0,
}

public enum StateType
{
    None = 0,
    StartTurnDamageGiver = 1 << 1,
    StatChanger = 2 << 2,
    StateGiver = 3 << 3,
    AntiState = 4 << 4,
    Everything = ~0,
}

public class aState : ScriptableObject
{
    [Header("Category")]
    public StateClass myClass;

    [Header("Conditions")]
    public bool ShowConditions;
    public WeaponType WeaponFilter;
    [HideInInspector] public bool isDependantOfACaseState => Dependance != "";
    public string Dependance;


    [Header("Stat change")]
    public StateType myStateType;
    [Space]
    public bool ShowStatModifier;
    public string StatModified;
    public float ModifiedOf;
    [Space]
    public int InflictedDamages;
    [Space]
    public int ActiveTurns;
    [Space]
    public StateClass myAnti;

    [Header("Effect giver")]
    public string EffectGivedOnAttackPath;
    [HideInInspector]public aState EffectGivedOnAttack;

    [Space]

    [Header("UI")]
    public bool ShowUISettings;
    public Sprite Logo;
    public string GlyphPath;
    [Space, TextArea(10,30)]
    public string TipToShowCrypted;
    public string Positiveness;

    [Space, Header("Graphics")]
    public bool ShowGraphics;
    public GameObject Effect;
    public GraphicEffectOn GraphicEffectAppliedOn;

    public void Activation(FightEntity myFightEntity, SpriteRenderer myWeaponRenderer, aCompetence UsedComp = null)
    {
        if (myStateType == StateType.StatChanger)
            myFightEntity.GetType().GetField(StatModified).SetValue(myFightEntity, (int)myFightEntity.GetType().GetField(StatModified).GetValue(myFightEntity) + (int)ModifiedOf);

        if (myStateType == StateType.AntiState)
            ApplyAnti(myFightEntity);

        if (Effect != null)
        {
            if (GraphicEffectAppliedOn == GraphicEffectOn.Character)
            {
                GameObject EffectInstance = Instantiate(Effect, myFightEntity.transform);
                ParticleSystem myParticleSystem = EffectInstance.GetComponent<ParticleSystem>();
                var shape = myParticleSystem.shape;
                shape.shapeType = ParticleSystemShapeType.Circle;
                shape.radius = 0.5f;
                Effect = EffectInstance;
            }
            else if (GraphicEffectAppliedOn == GraphicEffectOn.Weapon)
            {
                GameObject EffectInstance = Instantiate(Effect, myFightEntity.FirstWeaponTransform);
                ParticleSystem myParticleSystem = EffectInstance.GetComponent<ParticleSystem>();
                var shape = myParticleSystem.shape;
                shape.shapeType = ParticleSystemShapeType.SpriteRenderer;
                shape.spriteRenderer = myWeaponRenderer;
                Effect = EffectInstance;
            }
        }

        if(UsedComp)
            WeaponFilter = UsedComp.WeaponFilter;

        if (EffectGivedOnAttackPath != "")
        {
            EffectGivedOnAttack = CloneOf(EffectGivedOnAttackPath);

            for (int i = 0; i < myFightEntity.myCompetences.Count; i++)
            {
                if (ConditionAcceptables(myFightEntity, myFightEntity.myCompetences[i].LinkedWeapon, true))
                    myFightEntity.myCompetences[i].AddAppliedState(EffectGivedOnAttack);
            }
        }
    }

    public bool isThereAnAnti(FightEntity myFightEntity)
    {
        bool toReturn = false;

        for(int i = 0; i < myFightEntity.ActiveStates.Count; i++)
        {
            if(myFightEntity.ActiveStates[i].myStateType == StateType.AntiState && myFightEntity.ActiveStates[i].myAnti.HasFlag(myClass))
            {
                toReturn = true;
                break;
            }
        }

        return toReturn;
    }

    public void ApplyAnti(FightEntity myFightEntity)
    {
        List<aState> NewList = new List<aState>();
        List<int> ToEnd = new List<int>();

        for (int i = 0; i < myFightEntity.ActiveStates.Count; i++)
        {
            if (!myFightEntity.ActiveStates[i].myClass.HasFlag(myAnti))
            {
                NewList.Add(myFightEntity.ActiveStates[i]);
            }
            else
            {
                ToEnd.Add(i);
            }
        }

        for(int i = 0; i < ToEnd.Count; i++)
        {
            myFightEntity.ActiveStates[ToEnd[i] - i].ATurnLess(myFightEntity, true);
        }

        //myFightEntity.ActiveStates = NewList;
    }

    public void ATurnLess(FightEntity myFightEntity, bool InstantFinish = false)
    {
        if (!InstantFinish)
        {
            ActiveTurns -= 1;
            if (ActiveTurns <= -1)
            {
                if (myStateType == StateType.StatChanger)
                    myFightEntity.GetType().GetField(StatModified).SetValue(myFightEntity, (int)myFightEntity.GetType().GetField(StatModified).GetValue(myFightEntity) - (int)ModifiedOf);

                if (Effect)
                {
                    for (int i = 0; i < myFightEntity.myCompetences.Count; i++)
                        if(EffectGivedOnAttack)myFightEntity.myCompetences[i].RevokeAppliedState(EffectGivedOnAttack);
                    Effect.GetComponent<AutoDestruction>().Death();
                    Effect.GetComponent<ParticleSystem>().Stop();
                }
                myFightEntity.ActiveStates.Remove(this);
                Destroy(this);
            }
        }
        else
        {
            if (myStateType == StateType.StatChanger)
                myFightEntity.GetType().GetField(StatModified).SetValue(myFightEntity, (int)myFightEntity.GetType().GetField(StatModified).GetValue(myFightEntity) - (int)ModifiedOf);

            if (Effect)
            {
                if(GraphicEffectAppliedOn == GraphicEffectOn.Weapon)
                {
                    for (int i = 0; i < myFightEntity.myCompetences.Count; i++)
                        if (EffectGivedOnAttack) myFightEntity.myCompetences[i].RevokeAppliedState(EffectGivedOnAttack);
                }
                Effect.GetComponent<AutoDestruction>().Death();
                Effect.GetComponent<ParticleSystem>().Stop();
            }
            myFightEntity.ActiveStates.Remove(this);
            Destroy(this);
        }
    }

    public string TipToShow()
    {
        string toShow = "";
        string TitleColor = "yellow";
        if (Positiveness == "Positive") TitleColor = "green";
        else if (Positiveness == "Negative") TitleColor = "red";
        toShow += "<b><color=" + TitleColor + ">" + name + "</b></color>\n";
        string[] splitArray = TipToShowCrypted.Split('|');
        for (int i = 0; i < splitArray.Length; i++)
        {
            if (splitArray[i].Contains("StatModified"))
                splitArray[i] = "<color=yellow>" + StatModified + "</color>";
            else if (splitArray[i].Contains("ModifiedOf"))
            {
                string theColor = "yellow";
                if (ModifiedOf > 0) theColor = "green";
                else if (ModifiedOf < 0) theColor = "red";
                splitArray[i] = "<color=" + theColor + ">" + ModifiedOf.ToString() + "</color>";
            }
            else if (splitArray[i].Contains("InflictedDamages"))
                splitArray[i] = "<color=red>" + InflictedDamages.ToString() + "</color>";
            else if (splitArray[i].Contains("ActiveTurns"))
                splitArray[i] = "<color=yellow>" + (ActiveTurns + 1).ToString() + " turns</color>";
            else if (splitArray[i].Contains("EffectGivedOnAttack"))
            {
                aStateModel mychild = Resources.Load<aStateModel>("Etats/" + EffectGivedOnAttackPath);
                string theColor = "yellow";
                if (mychild.Positiveness == "Positive") theColor = "green";
                else if (mychild.Positiveness == "Negative") theColor = "red";
                splitArray[i] = "<color=" + theColor +">" + EffectGivedOnAttackPath + "</color>";
            }

            toShow += splitArray[i];
        }

        if(isDependantOfACaseState)
            toShow += "\n - - - - - \n" + "Dependant of a Case";
        else
            toShow += "\n - - - - - \n" + "Disapears in " + "<color=yellow>" + (ActiveTurns + 1).ToString() + " turns</color>";

        return toShow;
    }

    public bool ConditionAcceptables(FightEntity target, aWeapon SpecificWeapon = null, bool ToBeSpecific = false, WeaponType ForcedFilter = WeaponType.Everything)
    {
        bool Acceptable = true;

        if (ForcedFilter != WeaponType.Everything)
            WeaponFilter = ForcedFilter;

        if(WeaponFilter != WeaponType.Everything)
        {
            if (!ToBeSpecific)
            {
                if (!WeaponFilter.HasFlag(target.FirstWeaponStats.myWeaponType))
                {
                    Acceptable = false;
                }

                if (Acceptable == false && target.SideWeaponStats != null && WeaponFilter.HasFlag(target.SideWeaponStats.myWeaponType))
                    Acceptable = true;
            }
            else
            {
                if (!SpecificWeapon || !WeaponFilter.HasFlag(SpecificWeapon.myWeaponType))
                {
                    Acceptable = false;
                }
            }
        }

        if (isThereAnAnti(target))
            Acceptable = false;

        return Acceptable;
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

    public void EffectDeath()
    {
        if(Effect)
            Effect.GetComponent<AutoDestruction>().Death();
    }
}