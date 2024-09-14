using UnityEngine;

public class RotationHandler : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform referenceObject;
    [SerializeField] private float forwardRange = 5;
    [SerializeField] private float upRange = 5;
    [SerializeField] private MoveAction moveAction;

    [SerializeField] private float rotationSpeed = 10f;

    void Update()
    {
        // Skip rotation if player is braking
        if (moveAction.IsBraking())
        {
            return;
        }

        // Get the current move vector from the MoveAction script
        Vector3 moveVector = moveAction.GetMoveVector();

        // Only rotate if there is movement and not braking
        if (moveVector.sqrMagnitude > 0)
        {
            RotatePlayer(moveVector);
        }

        CheckForwardDirection();
        CheckUpDirection();
    }

    void RotatePlayer(Vector3 moveDirection)
    {
        // Create a rotation that faces the moveDirection but aligns the up direction to the reference object's up
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection, referenceObject.up);

        // Smoothly rotate the player towards the target direction
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
