using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestruction : MonoBehaviour
{
    float Temps;
    public float Latence;
    public bool DestroyActivated;
    bool AlreadyStopped;

    public void Death()
    {
        ParticleLightSpawner particleLightSpawner = GetComponent<ParticleLightSpawner>();
        if (particleLightSpawner)
            particleLightSpawner.LightDeath();
    }

    // Update is called once per frame
    void Update()
    {
        if (DestroyActivated)
        {
            if (!AlreadyStopped)
            {
                AlreadyStopped = true;
                GetComponent<ParticleSystem>().Stop();
            }

            Temps += Time.deltaTime;
            if(Temps >= Latence)
            {
                Destroy(gameObject);
            }
        }
    }

    public void DirectDestructionOfParent()
    {
        if (transform.parent)
        {
            Destroy(transform.parent.gameObject);
        }
    }
}
