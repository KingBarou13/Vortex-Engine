using UnityEngine;

public class RotationHandler : MonoBehaviour
{
    [SerializeField] private Transform player; // The child object (player) to rotate
    [SerializeField] private MoveAction moveAction; // Reference to the MoveAction script

    [SerializeField] private float rotationSpeed = 5f; // Speed at which the object rotates

    void Update()
    {
        // Skip rotation if player is braking
        if (moveAction.IsBraking())
        {
            return;
        }

        // Get the current move vector from the MoveAction script
        Vector3 moveVector = moveAction.GetMoveVector();

        // Only rotate if there is movement and we are not braking
        if (moveVector.sqrMagnitude > 0)
        {
            RotatePlayer(moveVector);
        }
    }

    void RotatePlayer(Vector3 moveDirection)
    {
        // Get the target rotation based on the movement direction
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        Vector3 targetEuler = targetRotation.eulerAngles;

        // Apply only y-axis rotation
        Quaternion newRotation = Quaternion.Euler(0, targetEuler.y, 0);

        // Rotate the player smoothly towards the target direction
        player.rotation = Quaternion.Slerp(player.rotation, newRotation, rotationSpeed * Time.deltaTime);
    }
}
