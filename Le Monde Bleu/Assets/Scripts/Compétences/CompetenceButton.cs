using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CompetenceButton : MonoBehaviour
{
    [Header("External references")]
    PlayerManager PM;
    [Space]

    [Header("Components")]
    Image myImage, myCadran1, myCadran2, myCostBackground;  
    aTip myTip;
    Animator myAnimator;
    TextMeshProUGUI myCost;

    [Header("Cost background colors")]
    public Color Usable, nonUsable;

    [HideInInspector]
    public aCompetence RepresentedComp;

    // Start is called before the first frame update
    public void Activate(aCompetence ShowedComp)
    {
        PM = PlayerManager.Instance;
        myTip = GetComponent<aTip>();
        myImage = GetComponent<Image>();
        myAnimator = GetComponent<Animator>();

        RepresentedComp = ShowedComp;
        myImage.sprite = ShowedComp.Logo;
        myTip.ToShow = RepresentedComp.TipToShow();

        myCadran1 = transform.GetChild(1).GetComponent<Image>(); myCadran1.color = ShowedComp.CultureColor;
        myCadran2 = transform.GetChild(2).GetComponent<Image>(); myCadran2.color = ShowedComp.CultureColor;

        myCostBackground = transform.GetChild(3).GetComponent<Image>();
        myCost = myCostBackground.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        myCost.text = ShowedComp.EnergyCost + " <sprite=11>";
        
        switch (PM.actualEntity.Energy >= ShowedComp.EnergyCost)
        {
            case true:
                myCostBackground.color = Usable;
                break;
            case false:
                myCostBackground.color = nonUsable;
                break;
        }
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

    public void Highlight(bool Doit)
    {
        if (Doit)
            myAnimator.SetTrigger("Highlight");
        else
            myAnimator.SetTrigger("Unlight");
    }
}
