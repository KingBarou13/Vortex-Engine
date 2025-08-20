using UnityEngine;
using Unity.Mathematics;

public class RotationHandler : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform referenceObject;
    [SerializeField] private float forwardRange = 5;
    [SerializeField] private float upRange = 5;
    [SerializeField] private MoveAction moveAction;
    [SerializeField] private RailGrindTrigger grindAction;
    [SerializeField] private DriftAction driftAction;
    [SerializeField] private CameraManager cameraManager;

    [SerializeField] private float rotationSpeed = 10f;

    void Update()
    {
        if (moveAction.IsBraking())
        {
            return;
        }

        if (driftAction != null && driftAction.IsDrifting)
        {
            RotateWhileDrifting();
        }
        else if (grindAction.isGrinding)
        {
            RotateAccordingToSpline();
        }
        else
        {
            Vector3 moveVector = moveAction.GetMoveVector();

            if (moveVector.sqrMagnitude > 0)
            {
                RotatePlayer(moveVector);
            }

            CheckForwardDirection();
            CheckUpDirection();
        }
    }

    private void RotateWhileDrifting()
    {
        if (cameraManager == null || cameraManager.GetActiveCamera() == null) return;

        Camera activeCamera = cameraManager.GetActiveCamera();
        Vector3 cameraForward = activeCamera.transform.forward;

        // Project camera forward vector onto the ground plane to get the desired look direction
        Vector3 lookDirection = Vector3.ProjectOnPlane(cameraForward, referenceObject.up).normalized;

        if (lookDirection.sqrMagnitude > 0.01f)
        {
            RotatePlayer(lookDirection);
        }
    }

    public void RotatePlayer(Vector3 moveDirection)
    {
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection, referenceObject.up);
        player.rotation = Quaternion.Slerp(player.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }


    void RotateAccordingToSpline()
    {
        Vector3 tangent = math.normalize(grindAction.currentSpline.EvaluateTangent(grindAction.splineProgress));

        if (grindAction.isReversing)
        {
            tangent = -tangent;
        }

        Quaternion targetRotation = Quaternion.LookRotation(tangent);

        player.rotation = Quaternion.Slerp(player.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    void CheckForwardDirection()
    {
        Vector3 forwardDirection = Vector3.forward;
        Ray forwardRay = new Ray(transform.position, transform.TransformDirection(forwardDirection * forwardRange));
        Debug.DrawRay(transform.position, transform.TransformDirection(forwardDirection * forwardRange), Color.blue);
    }

    void CheckUpDirection()
    {
        Vector3 upDirection = Vector3.up;
        Ray upRay = new Ray(transform.position, transform.TransformDirection(upDirection * upRange));
        Debug.DrawRay(transform.position, transform.TransformDirection(upDirection * upRange), Color.blue);
    }
}