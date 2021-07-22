using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InitiativeCadre : MonoBehaviour
{
    [Header("External")]
    InitiativeDisplayer ID;
    TurnManager TM;

    [HideInInspector]public FightEntity myEntity;
    [HideInInspector]public FightNature myNature;
    public float TurnsFromPlay;

    [Header("Elements")]
    Image Hero;
    Image Fond;
    Image Portrait;
    Image Cadre;
    Image Fond2;

    aTip myTip;
    
    TextMeshProUGUI Pourcentage;
    GameObject CadrePlaying;

    // Start is called before the first frame update
    public void Activation(FightEntity myNewEntity = null, FightNature myNewNature = null)
    {
        Hero = transform.GetChild(0).GetComponent<Image>();
        Fond = transform.GetChild(1).GetComponent<Image>();
        Portrait = transform.GetChild(2).GetComponent<Image>();
        Cadre = transform.GetChild(3).GetComponent<Image>();
        Fond2 = transform.GetChild(4).GetComponent<Image>();       
        Pourcentage = transform.GetChild(5).GetComponent<TextMeshProUGUI>();
        myTip = Portrait.GetComponent<aTip>();
        CadrePlaying = transform.GetChild(6).gameObject;

        CadrePlaying.SetActive(false);

        ID = InitiativeDisplayer.Instance;
        TM = TurnManager.Instance;

        if (myNewEntity)
        {

            myEntity = myNewEntity;
            myEntity.myInitiativeCadre = this;
            string RichTextColor = "white";

            if (myEntity.myAlignement == Alignement.Membre)
            {
                Fond.color = ID.Fond_Membre;
                Cadre.color = ID.Cadre_Membre;
                RichTextColor = "#0094FF";
            }
            else if (myEntity.myAlignement == Alignement.Allié)
            {
                Fond.color = ID.Fond_Allié;
                Cadre.color = ID.Cadre_Allié;
                RichTextColor = "#007F0E";
            }
            else if (myEntity.myAlignement == Alignement.Ennemi)
            {
                Fond.color = ID.Fond_Ennemi;
                Cadre.color = ID.Cadre_Ennemi;
                RichTextColor = "#FF0000";
            }

            if (myNewEntity.myClasse == Classe.Hero)
                Hero.color = Cadre.color;
            else
                Hero.gameObject.SetActive(false);

            Portrait.sprite = myEntity.PortraitZoom;
            Pourcentage.text = (myEntity.ActualInitiative / 10).ToString() + "%";
            myTip.ToShow = "<color="+RichTextColor+">" + myNewEntity.Nom + "</color> , " + myEntity.myClasse.ToString();
        }

        else if(myNewNature)
        {
            Hero.gameObject.SetActive(false);
            myNature = myNewNature;

            Fond.color = ID.Fond_Neutre;
            Cadre.color = ID.Cadre_Neutre;
            myTip.ToShow = "Terrain Evolution";
        }

        if(TM.myFS != FightSituation.Deployement)
            ID.UpdateGridConstraint();
    }

    public void Remove()
    {
        for (int i = 0; i < ID.allCadres.Count; i++)
        {
            if(ID.allCadres[i] == this)
            {
                ID.allCadres.RemoveAt(i);
                break;
            }
        }

        ID.UpdateGridConstraint();

        Destroy(gameObject);
    }

    public void Actualize()
    {
        if (myEntity)
        {
            Pourcentage.text = (Mathf.Clamp(myEntity.ActualInitiative / 10, 0, 100)).ToString() + "%";
            if (myEntity.IsPlaying)
                CadrePlaying.SetActive(true);
            else
                CadrePlaying.SetActive(false);
        }
        else
        {
            Pourcentage.text = (Mathf.Clamp(myNature.ActualInitiative / 10, 0, 100)).ToString() + "%";
            if (myNature.isPlaying)
                CadrePlaying.SetActive(true);
            else
                CadrePlaying.SetActive(false);
        }
    }

    public int PlaceInTurns()
    {
        int myIndex = 0;
        if (myEntity)
        {
            for (int i = 0; i < TM.activeFighters.Count; i++)
            {
                if (TM.activeFighters[i] == myEntity)
                {
                    myIndex = i;
                    break;
                }
            }
        }
        else
            myIndex = TM.activeFighters.Count;
        return myIndex;
    }

    #region Deduced Elements

    public int myMaxInitiative()
    {
        if (myEntity)
            return myEntity.MaxInitiative;
        else
            return 1000;
    }

    public int myInitiativeSpeed()
    {
        if (myEntity)
            return myEntity.InitiativeSpeed;
        else
            return myNature.InitiativeSpeed;
    }

    public int GetmyActualInitiative()
    {
        if (myEntity)
            return myEntity.ActualInitiative;
        else
            return myNature.ActualInitiative;
    }

    #endregion
}
