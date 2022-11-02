using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponFX : MonoBehaviour
{
    FightEntity myEntity;
    FightCamera FC;

    public TrailRenderer TR;
    public ParticleSystem PS;

    PopUpLight myLight;

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
        if (TR)
        {
            TR.emitting = true;
            myLight = Instantiate(Resources.Load<GameObject>("Lights/Point Light 2D"), TR.transform.position, TR.transform.rotation).GetComponent<PopUpLight>();
            myLight.Activation(TR.startColor, 0.1f , true ,TR.transform);
        }

        if (PS)
        {
            PS.Play();
            PS.GetComponent<ParticleLightSpawner>().ActivateLight();
        }

    }
    public void StopTrail()
    {
        if (TR)
            TR.emitting = false;
        if (PS)
        {
            PS.Stop();
            PS.GetComponent<ParticleLightSpawner>().LightPurgatory();
        }
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
