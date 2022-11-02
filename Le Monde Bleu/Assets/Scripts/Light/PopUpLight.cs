using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PopUpLight : MonoBehaviour
{
    UnityEngine.Rendering.Universal.Light2D myLight;

    Transform myFollow;

    bool Dying = false;
    bool Desactivate = false;

    public void Activation(Color color, float intensity, bool InstantDying = false ,Transform follow = null)
    {
        myLight = GetComponent<UnityEngine.Rendering.Universal.Light2D>();
        myLight.color = color;
        myLight.intensity = intensity;
        Dying = InstantDying;
        myFollow = follow;
    }

    public void SilenceAssignation(Color color, float intensity, Transform follow = null)
    {
        myLight = GetComponent<UnityEngine.Rendering.Universal.Light2D>();
        myLight.color = color;
        myLight.intensity = 0;
        myFollow = follow;
    }

    public void Death() => Dying = true;

    public void Desactivation() => Desactivate = true;

    private void Update()
    {
        if(myFollow)
            transform.position = myFollow.position;

        if (Dying || (Desactivate && myLight.intensity != 0))
        {
            myLight.intensity -= Time.deltaTime / 3;
            if (myLight.intensity <= 0)
            {
                if (Dying)
                    Destroy(gameObject);
                else
                {
                    Desactivate = false;
                    myLight.intensity = 0;
                }
            }
                
        }
    }
}
