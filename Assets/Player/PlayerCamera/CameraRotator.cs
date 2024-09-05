using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CameraRotator : MonoBehaviour
{
    public Transform OriginTransform, PlayerMover, ParentTransform;
    public Collider PlayerCollider;
    [Header("Anti-clipping")]
    public float DistToAct;
    public float FloorDistance;
    [Header("Smoothly Changing State")]
    [Range(0, 1)]
    public float LerpSpeed;
    public float DistanceToReconnect;
    [Header("Cinematic Camera Positions")]
    int CollidingObjects = 0;
    [NonSerialized] public bool InCinematic = false;
    bool PrevCinematic = false;
    bool LerpingBack = false;
    [NonSerialized] public Transform CinematicObject;
    [Header("Returning From Cinematics")]
    float CurrentLerpSpeed;
    public float ReturnSpeed;

    private void Start()
    {
        CurrentLerpSpeed = LerpSpeed;
    }

    void Update() //Start function
    {
        OriginTransform.position = PlayerMover.position;
        if (InCinematic)
        {
            PrevCinematic = true;
            transform.position = Vector3.Lerp(transform.position, CinematicObject.position, LerpSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, CinematicObject.rotation, LerpSpeed);
        }
        else
        {
            if (!PrevCinematic && !LerpingBack)
            {
                transform.position = ParentTransform.position;
                transform.rotation = ParentTransform.rotation;
            }
            else if (PrevCinematic)
            {
                PrevCinematic = false;
                LerpingBack = true;
            }
            else if (LerpingBack)
            {
                CurrentLerpSpeed = Mathf.Lerp(CurrentLerpSpeed, 1, ReturnSpeed / Vector3.Distance(transform.position, ParentTransform.position));
                transform.position = Vector3.Lerp(transform.position, ParentTransform.position, CurrentLerpSpeed);
                transform.rotation = Quaternion.Lerp(transform.rotation, ParentTransform.rotation, CurrentLerpSpeed);
            }
            if (Vector3.Distance(transform.position, ParentTransform.position) < DistanceToReconnect)
            {
                LerpingBack = false;
                CurrentLerpSpeed = LerpSpeed;
            }
        }
        //UpdateCameraPos();
    }

    void UpdateCameraPos()
    {
        float MaxDistance = Vector3.Distance(transform.position, PlayerMover.position);
        Vector3 direction = (transform.position - PlayerMover.position).normalized;
        if (CollidingObjects > 0 && Physics.Raycast(PlayerMover.transform.position, direction, out RaycastHit hit, MaxDistance))
        {
            transform.position = PlayerMover.position + direction * (hit.distance - FloorDistance);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == PlayerCollider) return;
        CollidingObjects++;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == PlayerCollider) return;
        CollidingObjects--;
    }
}
