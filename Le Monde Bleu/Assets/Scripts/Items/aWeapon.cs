using System;
using System.Collections.Generic;
using UnityEngine;

public enum HandNumber { OneHanded, TwoHanded }
//public enum WeaponType { AttackMeleeWeapon, AttackRangedWeapon, HandCombat, DefenseWeapon, SpellCaster }
[Flags]public enum WeaponType
{
    None = 0,
    AttackMeleeWeapon = 1 << 1,
    AttackRangedWeapon = 2 << 2,
    HandCombat = 3 << 3,
    DefenseWeapon = 4 << 4,
    SpellCaster = 5 << 5,
    Everything = ~0,
}

[CreateAssetMenu]
public class aWeapon : ScriptableObject
{
    TipsShower TS;

    public WeaponType myWeaponType;
    public HandNumber myHandNumber;
    public RuntimeAnimatorController myAnimator;
    public List<aCompetence> LinkedComps;
    public GameObject myGameObject;
    public bool MeleeWeapon;
    [Header("Description")]
    public string Nom;
    [TextArea(10, 30)]
    public string Description;
    [Header("Bonus/Malus")]
    public int BonusCounter;
    public int BonusTranchant;
    public int BonusPerforant;
    public int BonusMagique;
    public int BonusChoc;
    public int BonusFrappeHeroique;
    public int EnergyGain;

    public string ToShowOnTip()
    {
        if (!TS)
            TS = TipsShower.Instance;

        string toShow = "<b>" + Nom + "</b>" + "\n";

        toShow += "<i>" + Description + "</i>" + "\n - - - - - \n";

        switch (myHandNumber)
        {
            case HandNumber.OneHanded:
                toShow += "<color=yellow>One handed </color>";
                break;
            case HandNumber.TwoHanded:
                toShow += "<color=yellow>Two handed </color>";
                break;
            default:
                break;
        }

        toShow += "<color=yellow>" + TS.NameFromWeaponType(myWeaponType) + "</color>";

        if (BonusCounter != 0)
        {
            string myTextColor = "white";
            string Additional = "";
            if (BonusCounter > 0)
            {
                myTextColor = "green"; Additional = "+";
            }
            if (BonusCounter < 0)
            {
                myTextColor = "red";
            }

            toShow += "\n<color=" + myTextColor + ">" + Additional + BonusCounter + "% of  <sprite=2>Counter Chances" + "</color>";
        }

        if (BonusTranchant != 0)
        {
            string myTextColor = "white";
            string Additional = "";
            if (BonusTranchant > 0)
            {
                myTextColor = "green"; Additional = "+";
            }
            if (BonusTranchant < 0)
            {
                myTextColor = "red";
            }

            toShow += "\n<color=" + myTextColor + ">" + Additional + BonusTranchant + "% of  <sprite=5>Sharp Damages" + "</color>";
        }
        if (BonusPerforant != 0)
        {
            string myTextColor = "white";
            string Additional = "";
            if (BonusPerforant > 0)
            {
                myTextColor = "green"; Additional = "+";
            }
            if (BonusPerforant < 0)
            {
                myTextColor = "red";
            }

            toShow += "\n<color=" + myTextColor + ">" + Additional + BonusPerforant + "% of  <sprite=7>Piercing Damages" + "</color>";
        }
        if (BonusMagique != 0)
        {
            string myTextColor = "white";
            string Additional = "";
            if (BonusMagique > 0)
            {
                myTextColor = "green"; Additional = "+";
            }
            if (BonusMagique < 0)
            {
                myTextColor = "red";
            }

            toShow += "\n<color=" + myTextColor + ">" + Additional + BonusMagique + "% of  <sprite=9>Magic Damages" + "</color>";
        }
        if (BonusChoc != 0)
        {
            string myTextColor = "white";
            string Additional = "";
            if (BonusChoc > 0)
            {
                myTextColor = "green"; Additional = "+";
            }
            if (BonusChoc < 0)
            {
                myTextColor = "red";
            }

            toShow += "\n<color=" + myTextColor + ">" + Additional + BonusChoc + "% of  <sprite=6>Impact Damages" + "</color>";
        }

        if (BonusFrappeHeroique != 0)
        {
            string myTextColor = "white";
            string Additional = "";
            if (BonusFrappeHeroique > 0)
            {
                myTextColor = "green"; Additional = "+";
            }
            if (BonusFrappeHeroique < 0)
            {
                myTextColor = "red";
            }

            toShow += "\n<color=" + myTextColor + ">" + Additional + BonusFrappeHeroique + "% of  <sprite=8>Heroic Hit Chances" + "</color>";
        }

        if (EnergyGain != 0)
        {
            string myTextColor = "white";
            string Additional = "";
            if (EnergyGain > 0)
            {
                myTextColor = "green"; Additional = "+";
            }
            if (EnergyGain < 0)
            {
                myTextColor = "red";
            }

            toShow += "\n<color=" + myTextColor + ">" + Additional + EnergyGain + "  <sprite=11>Energy Gain" + "</color>";
        }

        for (int i = 0; i < LinkedComps.Count; i++)
        {
            toShow += "\n" + "<color=green>Unlocks <b>" + LinkedComps[i].name + "</b></color>";
        }

        return toShow;
    }
}
