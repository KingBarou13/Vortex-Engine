using System;
using System.Collections;
using UnityEngine;

public class PlayerPhysics : MonoBehaviour
{
    public Rigidbody RB;
    public LayerMask layermask;
    public Vector3 horizontalVelocity => Vector3.ProjectOnPlane(RB.velocity, RB.transform.up);
    public Vector3 verticalVelocity => Vector3.Project(RB.velocity, RB.transform.up);
    public float verticalSpeed => Vector3.Dot(RB.velocity, RB.transform.up);

    public float speed => horizontalVelocity.magnitude;

    
    [SerializeField] float gravity;


    //Fixed Update

    public Action onPlayerPhysicsUpdate;

    void FixedUpdate() 
    { 
        onPlayerPhysicsUpdate?.Invoke();

        if(!groundInfo.ground)
        {
            Gravity();
        }

        if(groundInfo.ground && verticalSpeed < RB.sleepThreshold)
            RB.velocity = horizontalVelocity;

        StartCoroutine(LateFixedUpdateRoutine());

        IEnumerator LateFixedUpdateRoutine()
        {
            yield return new WaitForFixedUpdate();
            LateFixedUpdate();
        }
    }

    void Gravity()
    {
        RB.velocity -= Vector3.up * gravity * Time.deltaTime;
    }

    //Late Fixed Update

    void LateFixedUpdate()
    {
        Ground();
        Snap();

        if(groundInfo.ground)
            RB.velocity = horizontalVelocity;
    }


    //Ground
    [SerializeField] float groundDistance;
    
    public struct GroundInfo
    {
        public Vector3 point;
        public bool ground;
        public Vector3 normal;
    }

    [HideInInspector] public GroundInfo groundInfo;

    public Action onGroundEnter;

    public Action onGroundExit;

    void Ground()
    {
        float maxDistance = Mathf.Max(RB.centerOfMass.y, 0) + (RB.sleepThreshold *  Time.fixedDeltaTime);

        if(groundInfo.ground && verticalSpeed < RB.sleepThreshold)
            maxDistance += groundDistance;

        bool ground = Physics.Raycast(RB.worldCenterOfMass, -RB.transform.up, out RaycastHit hit, maxDistance, layermask, QueryTriggerInteraction.Ignore);

        Vector3 point = ground ? hit.point : RB.transform.position;

        Vector3 normal = ground ? hit.normal : Vector3.up;

        if (ground != groundInfo.ground)
        {
            if (ground)
                onGroundEnter?.Invoke();
            else
                onGroundExit?.Invoke();
        }

        groundInfo = new()
        {
            point  = point,
            normal = normal,
            ground  = ground
        };
    }

    //Snap
    void Snap()
    {
        RB.transform.up = groundInfo.normal;

        Vector3 goal = groundInfo.point;

        Vector3 difference = goal - RB.transform.position;

        if (RB.SweepTest(difference, out _, difference.magnitude, QueryTriggerInteraction.Ignore)) return;

        RB.transform.position = goal;
    }
}
