using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightCamera : Singleton<FightCamera>
{
    public Transform target;
    public Transform shordLivedTarget;
    [HideInInspector] public Vector3 NonLivingTarget;
    bool hasTarget => shordLivedTarget || target || (NonLivingTarget != Vector3.zero); 
    public bool SoloTraject;
    [HideInInspector]
    public Animator CameraAnimator;
    Camera myCamera;
    float TargetZoom;

    // Start is called before the first frame update
    void Awake()
    {
        CameraAnimator = GetComponentInChildren<Animator>();
        myCamera = GetComponentInChildren<Camera>();
        TargetZoom = myCamera.orthographicSize;
    }

    // Update is called once per frame
    void Update()
    {
        SmoothFollow();
        Movement();
        Zoom();
    }

    void SmoothFollow()
    {
        Vector3 TargetPosition = Vector3.zero;
        if (shordLivedTarget)
            TargetPosition = shordLivedTarget.position;
        else if (NonLivingTarget != Vector3.zero)
            TargetPosition = NonLivingTarget;
        else if(target)
            TargetPosition = target.position;

        if (TargetPosition != Vector3.zero)
        {
            Vector3 HighPosition = new Vector3(TargetPosition.x, TargetPosition.y, -10);
            Vector3 smooth = HighPosition - transform.position;
            transform.position += smooth * Time.deltaTime * 10;
            if (SoloTraject && Vector3.Distance(HighPosition, transform.position) <= 0.1f)
            {
                SoloTraject = false;
                if(shordLivedTarget && TargetPosition == shordLivedTarget.position)
                    shordLivedTarget = null;
                else if (target && TargetPosition == target.position)
                    target = null;
            }
        }
    }

    void Zoom()
    {
        if(Input.mouseScrollDelta.y > 0 || Input.mouseScrollDelta.y < 0)
        {
            float newZoom = TargetZoom - Input.mouseScrollDelta.y;
            TargetZoom = Mathf.Clamp(newZoom, 5, 15);
        }
        if(myCamera.orthographicSize != TargetZoom)
        {
            float Distance = TargetZoom - myCamera.orthographicSize;
            myCamera.orthographicSize += Distance / 20;
        }
    }

    void Movement()
    {
        if(!hasTarget)
        {
            Vector3 theMovement = new Vector3(Input.GetAxisRaw("Horizontal") / 8, Input.GetAxisRaw("Vertical") / 8, 0);
            transform.position += theMovement * Time.deltaTime * 100;
        }
    }

    public void NewFollow(Transform newTarget)
    {
        target = newTarget;
    }

    public void QuickFollow(Transform newTarget)
    {
        SoloTraject = true;
        target = newTarget;
    }

    public Vector3 GetMiddlePoint(Vector3 FirstPosition, Vector3 SecondPosition) => SecondPosition + ((FirstPosition - SecondPosition) / 2);
}
