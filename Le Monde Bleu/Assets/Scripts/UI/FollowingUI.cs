using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowingUI : MonoBehaviour
{
    public Renderer theRenderer;
    public Renderer CaseSafety;
    Renderer Choosed => theRenderer != null ? theRenderer : CaseSafety;
    bool HasBeenTrue;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Choosed != null && Camera.main != null)
        {
            var wantedposition = Camera.main.WorldToScreenPoint(Choosed.transform.position);
            if (Choosed.IsVisibleFrom(Camera.main)) transform.position = wantedposition;
            else transform.position = new Vector3(2000, 2000, 0);
        }
        else
        {
            transform.position = new Vector3(2000, 2000, 0);
        }
    }
}

public static class RendererExtensions
{
    public static bool IsVisibleFrom(this Renderer renderer, Camera camera)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
    }
}
