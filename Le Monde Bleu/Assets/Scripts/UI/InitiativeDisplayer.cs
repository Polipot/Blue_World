﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class InitiativeDisplayer : Singleton<InitiativeDisplayer>
{
    Animator myAnimator;
    GridLayoutGroup myGridGroup;
    public AnimationCurve ScalingByNumber;

    [Header("Couleurs de Cadre")]
    public Color Cadre_Membre;
    public Color Cadre_Allié;
    public Color Cadre_Ennemi;
    public Color Cadre_Neutre;
    [Space]
    [Header("Couleurs de Fond")]
    public Color Fond_Membre;
    public Color Fond_Allié;
    public Color Fond_Ennemi;
    public Color Fond_Neutre;

    public List<InitiativeCadre> allCadres;

    // Start is called before the first frame update
    void Awake()
    {
        if(Instance != this)
        {
            Destroy(this);
        }

        myGridGroup = GetComponent<GridLayoutGroup>();
        myAnimator = GetComponent<Animator>();
    }

    public void UpdateCadres()
    {
        for (int i = 0; i < allCadres.Count; i++)
        {
            float manquant = allCadres[i].myMaxInitiative() - allCadres[i].GetmyActualInitiative();
            allCadres[i].TurnsFromPlay = manquant / allCadres[i].myInitiativeSpeed();
            allCadres[i].Actualize();
        }

        InitiativeCadre[] InitToArray = new InitiativeCadre[allCadres.Count];
        for (int i = 0; i < allCadres.Count; i++)
        {
            InitToArray[i] = allCadres[i];
        }

        allCadres = InitToArray.OrderBy(m => m.TurnsFromPlay).ThenBy(m => m.PlaceInTurns()).ToList();

        for (int i = 0; i < allCadres.Count; i++)
        {
            allCadres[i].transform.SetSiblingIndex(i);
        }
    }

    public void UpdateGridConstraint()
    {
        myGridGroup.cellSize = new Vector2(ScalingByNumber.Evaluate(allCadres.Count), myGridGroup.cellSize.y);
    }

    // Update is called once per frame
    public void GetACadre(FightEntity newEntity)
    {
        GameObject theCadre = Instantiate (Resources.Load<GameObject>("UI/Initiative_aCadre"), transform);
        theCadre.GetComponent<InitiativeCadre>().Activation(newEntity);
        allCadres.Add(theCadre.GetComponent<InitiativeCadre>());
    }

    public void GetACadre(FightNature newEntity)
    {
        GameObject theCadre = Instantiate(Resources.Load<GameObject>("UI/Initiative_aCadre"), transform);
        theCadre.GetComponent<InitiativeCadre>().Activation(null, newEntity);
        allCadres.Add(theCadre.GetComponent<InitiativeCadre>());
    }

    public void Appear() => myAnimator.SetTrigger("Appear");
}
