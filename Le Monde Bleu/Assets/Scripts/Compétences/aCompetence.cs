using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CompetenceType { Attaque, Téléportation, Poussée, Amélioration };
public enum AttaqueType { None, Tranchant, Perforant, Magique, Choc };

[CreateAssetMenu]
public class aCompetence : ScriptableObject
{
    [Header("Global")]
    public string Name;
    [HideInInspector]public FightEntity myFighter;
    [TextArea(10, 30)]
    public string Description;
    public CompetenceType myCompetenceType;
    public int EnergyCost;
    public AttaqueType myAttaqueType;
    [Header("Vision et Cibles")]
    public bool Anti_Perso;
    public bool Anti_Ground;
    public bool Vision_Affected;
    [Space]
    public bool TargetingAllies;
    [Space]
    [Header("Degats physiques")]
    public int MinDegats;
    public int MaxDegats;
    [Header("Degats à l'initiative en %")]
    public float InitiativeDegats;
    [Header("Etats")]
    public List<string> AppliedStates;
    [Header("Poussée")]
    public int Poussée;
    [Header("Pattern")]
    public List<Vector2> SelectableCases;
    public List<Vector2> PaternCase;
    [Header("Graphisms")]
    public Sprite Logo;
    [Header("Animations")]
    public string TriggerName;

    public void AddAppliedState(aState newState)
    {
        if (!AppliedStates.Contains(newState.name))
        {
            AppliedStates.Add(newState.name);
        }
    }

    public void RevokeAppliedState(aState newState)
    {
        if (AppliedStates.Contains(newState.name))
        {
            AppliedStates.Remove(newState.name);
        }
    }

    public string TipToShow()
    {
        string toShow = "<b>" + Name + "</b>" + "\n";

        toShow += "<i>" + Description + "</i>" + "\n - - - - -\n";

        string DamageType = "";
        switch (myAttaqueType)
        {
            case AttaqueType.None:
                break;
            case AttaqueType.Tranchant:
                DamageType = "Sharp";
                break;
            case AttaqueType.Perforant:
                DamageType = "Piercing";
                break;
            case AttaqueType.Magique:
                DamageType = "Magic";
                break;
            case AttaqueType.Choc:
                DamageType = "Impact";
                break;
            default:
                break;
        }

        switch (myCompetenceType)
        {
            case CompetenceType.Attaque:
                toShow += "<color=yellow>Attack</color>";
                toShow += "\n<color=green>Inflicts between " + (MinDegats / myFighter.ClasseDiviser) + " and " + ( MaxDegats/ myFighter.ClasseDiviser) + " " + DamageType + " Damages</color>";
                break;
            case CompetenceType.Téléportation:
                toShow += "<color=yellow>Movement</color>";
                break;
            case CompetenceType.Poussée:
                toShow += "<color=yellow>Push</color>";
                toShow += "\n<color=green>Pushes back an ennemy from " + Poussée + "cases</color>";
                break;
            case CompetenceType.Amélioration:
                toShow += "<color=yellow>Upgrade</color>";
                break;
            default:
                break;
        }

        if(InitiativeDegats != 0)
            toShow += "\n<color=green>Inflicts -" + InitiativeDegats + " Initiative</color>";

        for (int i = 0; i < AppliedStates.Count; i++)
        {
            toShow += "\n<color=green>Applies <b>" + AppliedStates[i] + "</b></color>";
        }

        toShow += "\n\n<color=red>Costs " + EnergyCost + " Energy</color>";
        return toShow;
    }
}
