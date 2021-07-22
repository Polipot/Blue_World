using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaseManager : Singleton<CaseManager>
{
    [Header("Couleurs de Case")]
    public Color Base;
    public Color Mouvement;
    public Color OccupiedMember;
    public Color OccupiedAllied;
    public Color OccupiedEnemy;
    public Color Blockage;
    [Space]
    [Header("Déploiement")]
    public Color DMember;
    public Color DAllied;
    public Color DEnemy;
    public Color DAlliedReinforcement;
    public Color DEnemyReinforcement;
    [Space]
    [Header("Blocage")]
    public Color Unblocked;
    public Color BlockedByEnnemy;
    public Color BlockedByAlly;
    public Color BlockedByBoth;
    [HideInInspector] public List<Case> CasesDMember;
    [HideInInspector] public List<Case> CasesDAllied;
    [HideInInspector] public List<Case> CasesDEnemy;
    [HideInInspector] public List<Case> CasesDAllied_Reinforcement;
    [HideInInspector] public List<Case> CasesDEnemy_Reinforcement;
    [Space]
    [Header("Couleurs de Marqueur de Case")]
    public Color MarqueurBase;
    public Color MarqueurHighlighted;
    public Color MarqueurWalkable;
    public Color MarqueurManualMovement;
    public Color MarqueurAttackSelectable;
    public Color MarqueurAttackCovered;
    [HideInInspector]
    public List<Case> MovementClickableCases;
    [HideInInspector]
    public List<Case> AttackClickableCases;
    [HideInInspector]
    public List<Case> AllCases;

    void Awake()
    {
        if(Instance != this)
        {
            Destroy(this);
        }
    }

    public void GetAllCases()
    {
        foreach(Transform child in transform)
        {
            AllCases.Add(child.GetComponent<Case>());
        }
    }

    public void ResetCases()
    {
        for (int i = 0; i < MovementClickableCases.Count; i++)
        {
            MovementClickableCases[i].MakeWalkable(false);
        }

        for (int i = 0; i < AttackClickableCases.Count; i++)
        {
            AttackClickableCases[i].MakeSelectableAttack(false);
        }

        AttackClickableCases = new List<Case>();
    }

    public void UnmarkDeployment()
    {
        for (int i = 0; i < CasesDMember.Count; i++)
            CasesDMember[i].UntriggerDeploiement();
        for (int i = 0; i < CasesDAllied.Count; i++)
            CasesDAllied[i].UntriggerDeploiement();
        for (int i = 0; i < CasesDEnemy.Count; i++)
            CasesDEnemy[i].UntriggerDeploiement();
    }

    public void GetAlignDeploy(Vector3 ColorValues, Case CaseToVerify)
    {
        Color AlignColor = new Color(ColorValues.x / 255, ColorValues.y / 255, ColorValues.z / 255);

        if (AlignColor == DEnemy)
        {
            CaseToVerify.DeployAlignement = Alignement.Ennemi;
            CaseToVerify.TriggerDeploiement();
        }
        else if (AlignColor == DMember)
        {
            CaseToVerify.DeployAlignement = Alignement.Membre;
            CaseToVerify.TriggerDeploiement();
        }
        else if (AlignColor == DAllied)
        {
            CaseToVerify.DeployAlignement = Alignement.Allié;
            CaseToVerify.TriggerDeploiement();
        }
        else if (AlignColor == DAlliedReinforcement)
            CasesDAllied_Reinforcement.Add(CaseToVerify);
        else if (AlignColor == DEnemyReinforcement)
            CasesDEnemy_Reinforcement.Add(CaseToVerify);
    }
}
