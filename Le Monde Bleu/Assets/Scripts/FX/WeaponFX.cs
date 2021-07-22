using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponFX : MonoBehaviour
{
    FightEntity myEntity;
    FightCamera FC;

    public TrailRenderer TR;
    public ParticleSystem PS;

    // Start is called before the first frame update
    void Awake()
    {
        myEntity = transform.parent.GetComponent<FightEntity>();
        FC = FightCamera.Instance;
    }

    public void MakeTheHit()
    {
        FC.CameraAnimator.SetTrigger("Shake");
        myEntity.AttackHit();
    }

    public void ActivateTrail()
    {
        if(TR)
            TR.emitting = true;
        if(PS)
            PS.Play();
    }
    public void StopTrail()
    {
        if(TR)
            TR.emitting = false;
        if (PS)
            PS.Stop();
    }

    public void EndAnim()
    {
        myEntity.EndAttack();
    }

    public void EndHit()
    {
        if (myEntity.mySituation == Situation.BeforeTurn)
            myEntity.TempsCoolBeforeTurn = 0;
    }
}
