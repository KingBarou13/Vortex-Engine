using System;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Splines;

public class GrindAction : MonoBehaviour
{
    [Header("Variables")]
    public bool onRail;
    [SerializeField] float grindSpeed;
    [SerializeField] float heightOffset;
    float timeForFullSpline;
    float elapsedTime;
    [SerializeField] float lerpSpeed = 10f;

    [Header("Scripts")]
    [SerializeField] RailScript currentRailScript;
    private PlayerPhysics playerPhysics;
    [SerializeField] private Animator animator;

    private void Start()
    {
        playerPhysics = GetComponent<PlayerPhysics>();
    }

    private void FixedUpdate()
    {
        if (onRail)
        {
            MovePlayerAlongRail();
            animator.SetBool("IsGrinding", true);
        }
        else
        {
            animator.SetBool("IsGrinding", false);
        }
    }

    void MovePlayerAlongRail()
    {
        if (currentRailScript != null && onRail)
        {
            float progress = elapsedTime / timeForFullSpline;

            if (progress < 0 || progress > 1)
            {
                ThrowOffRail();
                return;
            }

            float nextTimeNormalized = currentRailScript.normalDir 
                ? (elapsedTime + Time.deltaTime * grindSpeed) / timeForFullSpline 
                : (elapsedTime - Time.deltaTime * grindSpeed) / timeForFullSpline;

            float3 pos, tangent, up;
            float3 nextPosFloat, nextTan, nextUp;
            SplineUtility.Evaluate(currentRailScript.railSpline.Spline, progress, out pos, out tangent, out up);
            SplineUtility.Evaluate(currentRailScript.railSpline.Spline, nextTimeNormalized, out nextPosFloat, out nextTan, out nextUp);

            Vector3 worldPos = currentRailScript.LocalToWorldConversion(pos);
            Vector3 nextPos = currentRailScript.LocalToWorldConversion(nextPosFloat);

            playerPhysics.RB.position = worldPos + (transform.up * heightOffset);
            playerPhysics.RB.rotation = Quaternion.Lerp(playerPhysics.RB.rotation, Quaternion.LookRotation(nextPos - worldPos), lerpSpeed * Time.deltaTime);
            playerPhysics.RB.rotation = Quaternion.Lerp(playerPhysics.RB.rotation, Quaternion.FromToRotation(transform.up, up) * playerPhysics.RB.rotation, lerpSpeed * Time.deltaTime);

            elapsedTime += currentRailScript.normalDir ? Time.deltaTime * grindSpeed : -Time.deltaTime * grindSpeed;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Rail"))
        {
            onRail = true;
            currentRailScript = collision.gameObject.GetComponent<RailScript>();
            CalculateAndSetRailPosition();
        }
    }

    void CalculateAndSetRailPosition()
    {
        timeForFullSpline = currentRailScript.totalSplineLength / grindSpeed;
        Vector3 splinePoint;
        float normalisedTime = currentRailScript.CalculateTargetRailPoint(transform.position, out splinePoint);
        elapsedTime = normalisedTime * timeForFullSpline;

        float3 pos, forward, up;
        SplineUtility.Evaluate(currentRailScript.railSpline.Spline, normalisedTime, out pos, out forward, out up);
        currentRailScript.CalculateDirection(forward, transform.forward);

        transform.position = splinePoint + (transform.up * heightOffset);
    }

    public void ThrowOffRail()
    {
        onRail = false;
        currentRailScript = null;

        // Adjust the player's velocity when leaving the rail
        playerPhysics.RB.velocity += transform.forward * 1; 
    }

    // Called when jumping to exit the rail grind
    public void ExitRailOnJump()
    {
        ThrowOffRail();
    }
}
