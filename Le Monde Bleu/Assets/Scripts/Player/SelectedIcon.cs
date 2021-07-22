using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedIcon : MonoBehaviour
{
    PlayerManager PM;
    Animator myAnimator;
    bool Selected;

    // Start is called before the first frame update
    void Awake()
    {
        PM = PlayerManager.Instance;
        myAnimator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (PM.selectedEntity != null)
        {
            if (PM.selectedEntity.mySituation == Situation.Move)
                transform.position = PM.selectedEntity.transform.position;
            else
                transform.position = new Vector3(Mathf.RoundToInt(PM.selectedEntity.transform.position.x), Mathf.RoundToInt(PM.selectedEntity.transform.position.y), Mathf.RoundToInt(PM.selectedEntity.transform.position.z));
            if (!Selected)
            {
                myAnimator.SetTrigger("Select");
                Selected = true;
            }
        }
        else
        {
            if (Selected)
            {
                myAnimator.SetTrigger("Unselect");
                Selected = false;
            }
        }
    }
}
