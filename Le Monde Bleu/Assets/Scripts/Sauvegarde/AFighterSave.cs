using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AFighterSave : ScriptableObject
{
    [Header("Global")]
    public string Nom;
    public string HeroType;
    public int SaveIndex;
    [TextArea(10, 30)]
    public string Description;
    public Classe myClasse;
    public Alignement myAlignement;

    [Header("Armes")]
    public string FirstWeaponName;
    public string SideWeaponName;

    [Header("Armure")]
    public string ArmorName;

    [Header("UI elements & Skin")]
    public string Path;

    [Header("Statistiques")]
    public int Hp;
    public int MaxHp;
    [Space]
    public int maxArmor;
    [Space]
    public int Resistance;
    public int Parade;
    public int Esquive;
    [Space]
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
    public int InitiativeSpeed;

    [Header("Competences")]
    public List<string> myNativeCompetences;
}
