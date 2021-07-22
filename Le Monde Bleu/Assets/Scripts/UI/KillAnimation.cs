using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class KillAnimation : MonoBehaviour
{
    Animator myAnimator;
    TextMeshProUGUI myTextMeshPro;
    FollowingUI myFollowingUI;

    FightEntity theFE;

    public void Activation(FightEntity newFightEntity)
    {
        myAnimator = GetComponent<Animator>();
        myTextMeshPro = GetComponentInChildren<TextMeshProUGUI>();
        myFollowingUI = transform.parent.GetComponent<FollowingUI>();
        theFE = newFightEntity;

        myFollowingUI.CaseSafety = newFightEntity.OccupiedCase.GetComponent<Renderer>();
        myFollowingUI.theRenderer = newFightEntity.GetComponentInChildren<Renderer>();

        myTextMeshPro.text = "Killed";
        myAnimator.SetTrigger("Killed");
    }

    public void Impact()
    {
        FightCamera.Instance.CameraAnimator.SetTrigger("Shake");
        theFE.Death();
    }
}
