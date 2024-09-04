using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CameraRotator : MonoBehaviour
{
    public Transform TargetCameraTransform, PlayerMover;
    public Collider PlayerCollider;
    public float MovementLerpSpeed, RotationlerpSpeed;
    public bool OnlyRotateOnYAxis, UseLookAtRotation;
    public bool LerpedMovement, LerpedRotation;
    public float CameraMoveSpeed, CameraRotateSpeed, BaseRot, DistToAct;
    public float FloorDistance;
    public float PlayerViewRange;
    int CollidingObjects = 0;

    void FixedUpdate() //Start function
    {
        float PlayerDist = Vector3.Distance(PlayerMover.position, transform.position);
        if (PlayerDist > PlayerViewRange)
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
        }
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
                float DistFromCentre = GetDistanceFromScreenCenter(PlayerMover.position, GetComponent<Camera>()) + BaseRot;
                float step = CameraRotateSpeed * Vector3.Distance(PlayerMover.position, transform.position) / DistToAct;
                transform.rotation = Quaternion.Lerp(PrevRot, transform.rotation, step);
            }
        }
        UpdateCameraPos();
    }

    void UpdateCameraPos()
    {
        float MaxDistance = Vector3.Distance(TargetCameraTransform.position, PlayerMover.position);
        Vector3 direction = (TargetCameraTransform.position - PlayerMover.position).normalized;
        if (CollidingObjects > 0 && Physics.Raycast(PlayerMover.transform.position, direction, out RaycastHit hit, MaxDistance))
        {
            transform.position = PlayerMover.position + direction * (hit.distance - FloorDistance);
        }
    }

    float GetDistanceFromScreenCenter(Vector3 worldPosition, Camera cam)
    {
        // Get the object's position in screen space
        Vector3 screenPos = cam.WorldToScreenPoint(worldPosition);

        // Get the center of the screen
        Vector2 screenCenter = new(Screen.width / 2f, Screen.height / 2f);

        // Calculate the distance from the object's screen position to the center of the screen
        float distanceX = screenPos.x - screenCenter.x;
        float distanceY = screenPos.y - screenCenter.y;

        // Use the Pythagorean theorem to find the distance
        return Mathf.Sqrt(distanceX * distanceX + distanceY * distanceY);
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
