using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;

public class GrindAction : PlayerAction
{
    
    public bool onRail;
    [SerializeField] float grindSpeed;
    [SerializeField] float heightOffset;
    float timeForFullSpline;
    float elapsedTime;
    [SerializeField] float lerpSpeed = 10f;

    [SerializeField] RailScript currentRailScript;

    private void FixedUpdate()
    {
        if (onRail)
        {
            Grind();
        }
    }

    private void Update()
    {

    }
    
    void Grind()
    {
        if(currentRailScript != null && onRail)
        {
            float progress = elapsedTime / timeForFullSpline;


            if (progress < 0 || progress > 1)
            {
                ThrowOffRail();
                return;
            }


            float nextTimeNormalised;
            if (currentRailScript.normalDirection)
            {
                nextTimeNormalised = (elapsedTime + Time.deltaTime) / timeForFullSpline;
            }
            else
            {
                nextTimeNormalised = (elapsedTime - Time.deltaTime) / timeForFullSpline;
            }

            float3 pos, tangent, up;
            float3 nextPosfloat, nextTan, nextUp;
            SplineUtility.Evaluate(currentRailScript.railSpline.Spline, progress, out pos, out tangent, out up);
            SplineUtility.Evaluate(currentRailScript.railSpline.Spline, nextTimeNormalised, out nextPosfloat, out nextTan, out nextUp);

            Vector3 worldPos = currentRailScript.LocalToWorldConversion(pos);
            Vector3 nextPos = currentRailScript.LocalToWorldConversion(nextPosfloat);

            RB.transform.position = worldPos + (RB.transform.up * heightOffset);
            RB.transform.rotation = Quaternion.Lerp(RB.transform.rotation, Quaternion.LookRotation(nextPos - worldPos), lerpSpeed * Time.deltaTime);
            RB.transform.rotation = Quaternion.Lerp(RB.transform.rotation, Quaternion.FromToRotation(RB.transform.up, up) * RB.transform.rotation, lerpSpeed * Time.deltaTime);

            if (currentRailScript.normalDirection)
            {
                elapsedTime += Time.deltaTime;
            }
            else
            {
                elapsedTime -= Time.deltaTime;
            }
        }
    }

    private void OnCollisionEnter(Collision other) 
    {
        if (other.gameObject.tag == "Rail")
        {
            onRail = true;
            currentRailScript = other.gameObject.GetComponent<RailScript>();
            CalculateAndSetRailPosition();
        }
    }
    void CalculateAndSetRailPosition()
    {
        timeForFullSpline = currentRailScript.totalSplineLength / grindSpeed;

        Vector3 splinePoint;

        float normalisedTime = currentRailScript.CalculateTargetRailPoint(RB.transform.position, out splinePoint);
        elapsedTime = timeForFullSpline * normalisedTime;

        float3 pos, forward, up;
        SplineUtility.Evaluate(currentRailScript.railSpline.Spline, normalisedTime, out pos,out forward, out up);
        currentRailScript.CalculateDirection(forward, RB.transform.forward);
        RB.transform.position = splinePoint + (RB.transform.up * heightOffset);
    }
    void ThrowOffRail()
    {
        onRail = false;
        currentRailScript = null;
        RB.transform.position += RB.transform.forward * 1;
    }
}
