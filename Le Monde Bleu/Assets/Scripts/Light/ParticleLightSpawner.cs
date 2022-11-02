using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleLightSpawner : MonoBehaviour
{
    PopUpLight myLight;
    ParticleSystem myParticleSystem;
    public bool isEvent;
    public Color myLightColor;
    public float myIntensity = 1f;

    // Start is called before the first frame update
    void Awake()
    {
        myParticleSystem = GetComponent<ParticleSystem>();

        myLight = Instantiate(Resources.Load<GameObject>("Lights/Point Light 2D"), transform.position, transform.rotation).GetComponent<PopUpLight>();
        if (!isEvent)
            myLight.Activation(myLightColor, myIntensity, false, myParticleSystem.transform);
        else
            myLight.SilenceAssignation(myLightColor, myIntensity, myParticleSystem.transform);
    }

    public void ActivateLight() => myLight.Activation(myLightColor, myIntensity, false, myParticleSystem.transform);

    public void LightPurgatory()
    {
        if (isEvent)
            DesactivateLight();
        else
            LightDeath();
    }

    void DesactivateLight() => myLight.Desactivation();

    public void LightDeath() => myLight.Death();
}
