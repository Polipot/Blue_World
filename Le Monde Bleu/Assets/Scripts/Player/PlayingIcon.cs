using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayingIcon : MonoBehaviour
{
    TurnManager TM;
    CaseManager CM;
    FightEntity Playing;

    SpriteRenderer myIcon;

    void Awake()
    {
        TM = TurnManager.Instance;
        CM = CaseManager.Instance;
        myIcon = transform.GetChild(0).GetComponent<SpriteRenderer>();
        myIcon.color = Color.clear;
    }

    void Update()
    {
        if (Playing)
        {
            transform.position = Playing.transform.position;
        }
    }

    public void ChangePlaying(FightEntity NewPlaying = null)
    {
        Playing = NewPlaying;

        if (Playing)
        {
            switch (Playing.myAlignement)
            {
                case Alignement.Membre:
                    myIcon.color = CM.DMember;
                    break;
                case Alignement.Allié:
                    myIcon.color = CM.DAllied;
                    break;
                case Alignement.Ennemi:
                    myIcon.color = CM.DEnemy;
                    break;
                default:
                    break;
            }
        }
        else
            myIcon.color = Color.clear;
    }
}
