using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CameraRotator : MonoBehaviour
{
    public Transform TargetCameraTransform, PlayerMover;
    public float MovementLerpSpeed, RotationlerpSpeed;
    public bool OnlyRotateOnYAxis, UseLookAtRotation;
    public bool LerpedMovement, LerpedRotation;
    public float CameraMoveSpeed, CameraRotateSpeed;
    public float FloorDistance;

    void Update() //Start function
    {
        if (LerpedMovement)
        {
            transform.position = Vector3.Lerp(transform.position, TargetCameraTransform.position, MovementLerpSpeed);
        }
        else
        {
            float step = CameraMoveSpeed * CameraMoveSpeed;
            transform.position = Vector3.Lerp(transform.position, TargetCameraTransform.position, step * Vector3.Distance(transform.position, TargetCameraTransform.position));
        }
        UpdateCameraPos();
        if (!UseLookAtRotation)
        {
            Quaternion SampleRot = Quaternion.Lerp(transform.localRotation, PlayerMover.localRotation, RotationlerpSpeed);
            if (OnlyRotateOnYAxis)
            {
                SampleRot.x = 0;
                SampleRot.z = 0;
                SampleRot.w = 0;
            }
            transform.localRotation = SampleRot;
        }
        else
        {
            /*
            Vector3 DirectionToPlayer = PlayerMover.position - TargetCameraTransform.position;
            Quaternion LookRot = Quaternion.LookRotation(DirectionToPlayer, Vector3.up);
            Quaternion SampleRot = Quaternion.Lerp(transform.rotation, LookRot, RotationlerpSpeed);
            transform.rotation = SampleRot;
            */
            Quaternion PrevRot = transform.rotation;
            transform.LookAt(PlayerMover, Vector3.up);
            if (LerpedRotation)
            {
                transform.rotation = Quaternion.Lerp(PrevRot, transform.rotation, RotationlerpSpeed);
            }
            else
            {
                float step = Mathf.Pow(CameraRotateSpeed, 2);
                transform.rotation = Quaternion.Lerp(PrevRot, transform.rotation, step * Vector3.Distance(Quaternion.ToEulerAngles(PrevRot), transform.eulerAngles));
            }
        }
    }

    void UpdateCameraPos()
    {
        float MaxDistance = Vector3.Distance(transform.position, PlayerMover.position);
        Vector3 direction = (transform.position - PlayerMover.position).normalized;
        RaycastHit hit;
        if (Physics.Raycast(PlayerMover.transform.position, direction, out hit, MaxDistance))
        {
            transform.position = PlayerMover.position + direction * (hit.distance - FloorDistance);
        }
    }
}
