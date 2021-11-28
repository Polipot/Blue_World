using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CompetenceType { Attaque, Téléportation, Poussée, Amélioration };
public enum AttaqueType { None, Tranchant, Perforant, Magique, Choc };

[CreateAssetMenu]
public class aCompetence : ScriptableObject
{
    TipsShower TS;

    [Header("Global")]
    public string Name;
    [HideInInspector]public FightEntity myFighter;
    public aWeapon LinkedWeapon;
    public WeaponType WeaponFilter;
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
    public List<aStateModel> ModelStates;
    [Header("Poussée")]
    public int Poussée;
    [Header("Pattern")]
    public List<Vector2> SelectableCases;
    public List<Vector2> PaternCase;
    [Header("Graphisms")]
    public Sprite Logo;
    [Header("Animations")]
    public string TriggerName;
    [Space]
    [Header("Infos pour l'IA"), Range(0,10)]
    public int BaseOpportunity;
    public bool PatternIsDirectionnal;

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
        if (!TS)
            TS = TipsShower.Instance;

        string toShow = "<b>" + Name + "</b>" + "\n";

        toShow += "<i>" + Description + "</i>" + "\n - - - - -\n";

        string DamageType = "";
        switch (myAttaqueType)
        {
            case AttaqueType.None:
                break;
            case AttaqueType.Tranchant:
                DamageType = " <sprite=5>Sharp";
                break;
            case AttaqueType.Perforant:
                DamageType = " <sprite=7>Piercing";
                break;
            case AttaqueType.Magique:
                DamageType = " <sprite=9>Magic";
                break;
            case AttaqueType.Choc:
                DamageType = " <sprite=6>Impact";
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
            toShow += "\n<color=green>Inflicts -" + InitiativeDegats + "% Initiative</color>";

        for (int i = 0; i < AppliedStates.Count; i++)
        {
            toShow += "\n<color=green>Applies <b>" + AppliedStates[i] + "</b></color>";
            if(WeaponFilter != WeaponType.Everything)
            {
                toShow += "\n<color=yellow>Appliable on </color>";
                WeaponType myWeaponType = WeaponFilter;
                List<string> AllowedTypes = new List<string>();

                foreach (WeaponType value in WeaponType.GetValues(typeof(WeaponType)))
                    if (myWeaponType.HasFlag(value) && value != WeaponType.None)
                        AllowedTypes.Add(TS.NameFromWeaponType(value));

                for(int a = 0; a < AllowedTypes.Count; a++)
                {
                    if (a != 0 && a + 1 < AllowedTypes.Count)
                        toShow += "<color=yellow>, </color>";
                    else if (a != 0 && a + 1 >= AllowedTypes.Count)
                        toShow += "<color=yellow> and </color>";
                    toShow += "<color=yellow>" + AllowedTypes[i] + "</color>";
                }
            }
        }

        toShow += "\n\n<color=red>Costs " + EnergyCost + " Energy</color>";
        return toShow;
    }
}
