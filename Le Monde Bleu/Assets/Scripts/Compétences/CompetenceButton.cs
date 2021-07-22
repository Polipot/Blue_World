using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompetenceButton : MonoBehaviour
{
    PlayerManager PM;
    Image myImage;
    //[HideInInspector]
    public aCompetence RepresentedComp;
    aTip myTip;

    // Start is called before the first frame update
    public void Activate(aCompetence ShowedComp)
    {
        PM = PlayerManager.Instance;
        myTip = GetComponent<aTip>();
        myImage = GetComponent<Image>();
        RepresentedComp = ShowedComp;
        myImage.sprite = ShowedComp.Logo;
        myTip.ToShow = RepresentedComp.TipToShow();
    }

    public void ActivateCompetence()
    {
        if(PM.actualEntity.mySituation == Situation.ChooseMove)
        {
            PM.actualEntity.LoadAttack(RepresentedComp);
        }
        else if(PM.actualEntity.mySituation == Situation.ChooseAttack)
        {
            PM.actualEntity.EndAttack();
        }
    }
}
