using UnityEngine;

public class DashPanel : MonoBehaviour
{
    [SerializeField] private float dashSpeed; // Speed at which Sonic will dash
    [SerializeField] private float controlLockDuration; // Duration for the control lock

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object colliding with the dash panel is Sonic
        if (other.CompareTag("Player"))
        {
            PlayerPhysics playerPhysics = other.GetComponent<PlayerPhysics>();
            MoveAction moveAction = other.GetComponent<MoveAction>();

            if (playerPhysics != null && moveAction != null)
            {
                // Get the forward direction of the dash panel
                Vector3 dashDirection = transform.forward;

                // Apply the dash speed in the forward direction
                playerPhysics.RB.velocity = dashDirection * dashSpeed;

                // Trigger the control lock
                moveAction.TriggerControlLock(controlLockDuration);
            }
        }
    }
}
