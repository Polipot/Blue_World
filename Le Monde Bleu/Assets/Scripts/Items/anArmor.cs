using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class anArmor : ScriptableObject
{
    [Header("Global")]
    public string Nom;
    [TextArea(10, 30)]
    public string Description;
    [Space]
    [Header("Armor Bonus")]
    public int ArmorValue;
    [Space]
    [Header("Defense Bonus/Malus")]
    public int ResistanceChange;
    public int DodgeChange;
    public int CounterChange;
    [Space]
    [Header("Movement changes")]
    public int MovementChange;

    public string ToShowOnTip()
    {
        string toShow = "<b>" + Nom + "</b>" + "\n";

        toShow += "<i>" + Description + "</i>" + "\n - - - - - ";

        if(ArmorValue != 0)
        {
            string myTextColor = "white";
            string Additional = "";
            if (ArmorValue > 0)
            {
                myTextColor = "green"; Additional = "+";
            } 
            if (ArmorValue < 0)
            {
                myTextColor = "red";
            }

            toShow += "\n<color=" + myTextColor + ">" + Additional + ArmorValue + "  <sprite=3>Armor" + "</color>";
        }

        if (ResistanceChange != 0)
        {
            string myTextColor = "white";
            string Additional = "";
            if (ResistanceChange > 0)
            {
                myTextColor = "green"; Additional = "+";
            }
            if (ResistanceChange < 0)
            {
                myTextColor = "red";
            }

            toShow += "\n<color=" + myTextColor + ">" + Additional + ResistanceChange + "% of  <sprite=1>Resistance" + "</color>";
        }
        if (DodgeChange != 0)
        {
            string myTextColor = "white";
            string Additional = "";
            if (DodgeChange > 0)
            {
                myTextColor = "green"; Additional = "+";
            }
            if (DodgeChange < 0)
            {
                myTextColor = "red";
            }

            toShow += "\n<color=" + myTextColor + ">" + Additional + DodgeChange + "% of  <sprite=4>Dodge Chances" + "</color>";
        }
        if (CounterChange != 0)
        {
            string myTextColor = "white";
            string Additional = "";
            if (CounterChange > 0)
            {
                myTextColor = "green"; Additional = "+";
            }
            if (CounterChange < 0)
            {
                myTextColor = "red";
            }

            toShow += "\n<color=" + myTextColor + ">" + Additional + CounterChange + "% of  <sprite=2>Counter Chances" + "</color>";
        }

        if (MovementChange != 0)
        {
            string myTextColor = "white";
            string Additional = "";
            if (MovementChange > 0)
            {
                myTextColor = "green"; Additional = "+";
            }
            if (MovementChange < 0)
            {
                myTextColor = "red";
            }

            toShow += "\n<color=" + myTextColor + ">" + Additional + MovementChange + "  <sprite=13>Movement" + "</color>";
        }

        return toShow;
    }
}
