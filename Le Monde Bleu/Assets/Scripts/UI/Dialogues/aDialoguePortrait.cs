using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class aDialoguePortrait : MonoBehaviour
{
    DialogueManager DM;

    Animator PortraitAnim;
    Image Portrait;
    [HideInInspector] public string Nom;
    [HideInInspector] public PortraitSituation myPortraitSituation;
    [HideInInspector] public PortraitHightlight myPortraitHighlight;

    private void Awake()
    {
        PortraitAnim = GetComponent<Animator>();
        Portrait = GetComponent<Image>();
    }

    public void myOrder(string Order, string newName = "")
    {
        if(Order == "Appear")
        {
            if (newName != "")
            {
                if (!DM)
                    DM = DialogueManager.Instance;

                for (int i = 0; i < DM.IdentitiesName.Count; i++)
                {
                    if (DM.IdentitiesName[i] == newName)
                    {
                        Nom = newName;
                        Portrait.sprite = DM.IdentitiesPortrait[i];
                        break;
                    }
                }
            }

            myPortraitSituation = PortraitSituation.First;
            myPortraitHighlight = PortraitHightlight.Hightlighted;
            PortraitAnim.SetTrigger("ToSpeaker");
        }

        if(Order == "ToSecond")
        {
            myPortraitSituation = PortraitSituation.Second;
            myPortraitHighlight = PortraitHightlight.NoHightlighted;
            PortraitAnim.SetTrigger("ToSecond");
        }

        if (Order == "Disappear")
        {
            myPortraitSituation = PortraitSituation.Hidden;
            myPortraitHighlight = PortraitHightlight.NoHightlighted;
            PortraitAnim.SetTrigger("Leave");
        }

        if (Order == "Highlight")
        {
            myPortraitHighlight = PortraitHightlight.Hightlighted;
            if(myPortraitSituation == PortraitSituation.First)
                PortraitAnim.SetTrigger("Highlight");
            else if(myPortraitSituation == PortraitSituation.Second)
                PortraitAnim.SetTrigger("HighlightS");
        }

        if (Order == "NoHighlight")
        {
            myPortraitHighlight = PortraitHightlight.NoHightlighted;
            if (myPortraitSituation == PortraitSituation.First)
                PortraitAnim.SetTrigger("NoHighlight");
            else if (myPortraitSituation == PortraitSituation.Second)
                PortraitAnim.SetTrigger("NoHighlightS");
        }
    }
}
