using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class anApercu : MonoBehaviour
{
    CaseManager CM;

    [Header("Skins de fond")]
    public Sprite FondSoldat;
    public Sprite FondHero;

    [Header("Composants")]
    public GameObject Accroche;
    public Image Portrait;
    public Image FondFumee;
    public Image FondTexte;
    public TextMeshProUGUI Nom;
    public TextMeshProUGUI Vie, Armure, Résistance, Parade, Esquive;
    public TextMeshProUGUI Tranchant, Perforant, Magique, Choc, FrappeHeroique;
    public TextMeshProUGUI Energie, EnergieGain, Initiative, Mouvement;
    public GameObject AccrocheStates;
    aTip Description;
    public GameObject Item1, Item2, Item3;

    // Start is called before the first frame update
    void Awake()
    {
        CM = CaseManager.Instance;
        Description = Nom.GetComponent<aTip>();
    }

    public void ActualizeShowed(FightEntity newEntity = null)
    {
        if(newEntity != null)
        {
            Accroche.SetActive(true);

            switch (newEntity.myAlignement)
            {
                case Alignement.Membre:
                    FondFumee.color = CM.OccupiedMember;
                    break;
                case Alignement.Allié:
                    FondFumee.color = CM.OccupiedAllied;
                    break;
                case Alignement.Ennemi:
                    FondFumee.color = CM.OccupiedEnemy;
                    break;
            }

            Portrait.sprite = newEntity.Portrait;

            if (newEntity.myClasse == Classe.Hero)
                FondTexte.sprite = FondHero;
            else
                FondTexte.sprite = FondSoldat;

            Nom.text = newEntity.Nom;
            Description.ToShow = newEntity.Nom + "\n - - - - - \n" + newEntity.Description;

            if(newEntity.myClasse == Classe.Soldier)
                Description.ToShow += "\n - - - - - \n" + "<color=red>This is a Soldier, a less powerful fighter that can use only one ability each turn and who's damages are divided by 4</color>";

            Vie.text = newEntity.Hp + "/" + newEntity.MaxHp;
            Armure.text = newEntity.Armor + "/" + newEntity.maxArmor;
            ShowStat(Résistance, newEntity.Resistance + "%", newEntity.BaseResistance, newEntity.Resistance);
            ShowStat(Parade, newEntity.Parade + "%", newEntity.BaseParade, newEntity.Parade);
            ShowStat(Esquive, newEntity.Esquive + "%", newEntity.BaseEsquive, newEntity.Esquive);

            ShowStat(Tranchant, "+" + newEntity.Tranchant + "%", newEntity.BaseTranchant, newEntity.Tranchant);
            ShowStat(Perforant, "+" + newEntity.Perforant + "%", newEntity.BasePerforant, newEntity.Perforant);
            ShowStat(Magique, "+" + newEntity.Magique + "%", newEntity.BaseMagique, newEntity.Magique);
            ShowStat(Choc, "+" + newEntity.Choc + "%", newEntity.BaseChoc, newEntity.Choc);
            ShowStat(FrappeHeroique, newEntity.FrappeHeroique + "%", newEntity.BaseFrappeHeroique, newEntity.FrappeHeroique);

            Energie.text = newEntity.Energy + "/" + newEntity.MaxEnergy;
            ShowStat(EnergieGain, "+" + newEntity.EnergyGain, newEntity.BaseEnergyGain, newEntity.EnergyGain);
            ShowStat(Mouvement, newEntity.RemainingMovement.ToString() + "/" + newEntity.Speed.ToString(), newEntity.BaseSpeed, newEntity.Speed);
            ShowStat(Initiative, newEntity.InitiativeSpeed.ToString(), newEntity.BaseInitiativeSpeed, newEntity.InitiativeSpeed);

            #region Items
            if (newEntity.FirstWeaponStats)
            {
                Item1.SetActive(true);
                //replace image
                Item1.GetComponent<aTip>().ToShow = newEntity.FirstWeaponStats.ToShowOnTip();
            }
            else
                Item1.SetActive(false);
            if (newEntity.SideWeaponStats)
            {
                Item2.SetActive(true);
                //replace image
                Item2.GetComponent<aTip>().ToShow = newEntity.SideWeaponStats.ToShowOnTip();
            }
            else
                Item2.SetActive(false);
            if (newEntity.myArmor)
            {
                Item3.SetActive(true);
                // Replace image
                Item3.GetComponent<aTip>().ToShow = newEntity.myArmor.ToShowOnTip();
            }
            else
                Item3.SetActive(false);
            #endregion

            foreach (Transform child in AccrocheStates.transform)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < newEntity.ActiveStates.Count; i++)
            {
                GameObject IconState = Instantiate(Resources.Load<GameObject>("UI/aState"), AccrocheStates.transform);
                IconState.GetComponent<Image>().sprite = newEntity.ActiveStates[i].Logo;
                IconState.GetComponent<aTip>().ToShow = newEntity.ActiveStates[i].TipToShow();
            }
        }
        else
        {
            foreach(Transform child in AccrocheStates.transform)
            {
                Destroy(child.gameObject);
            }
            Accroche.SetActive(false);
        }
    }

    void ShowStat(TextMeshProUGUI theText, string textToShow, int BaseValue, int Value)
    {
        theText.text = textToShow;
        if (BaseValue > Value) theText.color = Color.red;
        else if (BaseValue < Value) theText.color = Color.green;
        else theText.color = Color.white;
    }
}
