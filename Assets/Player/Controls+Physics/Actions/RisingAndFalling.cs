using UnityEngine;

public class RisingAndFalling : MonoBehaviour
{
    [SerializeField] private PlayerPhysics playerPhysics;
    [SerializeField] private Animator animator;
    [SerializeField] private float risingThreshold = 2f; // Minimum upward speed to trigger rising animation
    [SerializeField] private float fallingThreshold = -2f; // Maximum downward speed to trigger falling animation

    private bool wasGrounded = true;
    private bool wasJumping = false;

    private void Update()
    {
        bool isGrounded = playerPhysics.groundInfo.ground;
        float verticalSpeed = playerPhysics.verticalSpeed;

        // Trigger rising or falling animation based on vertical speed when leaving the ground
        if (wasGrounded && !isGrounded && !wasJumping)
        {
            if (verticalSpeed > risingThreshold)
            {
                animator.SetTrigger("StartRising");
            }
            else if (verticalSpeed < fallingThreshold)
            {
                animator.SetTrigger("StartFalling");
            }
        }
        // Update rising/falling state based on vertical speed while in the air
        else if (!isGrounded)
        {
            if (verticalSpeed > risingThreshold)
            {
                animator.SetBool("IsRising", true);
                animator.SetBool("IsFalling", false);
            }
            else if (verticalSpeed < fallingThreshold)
            {
                animator.SetBool("IsRising", false);
                animator.SetBool("IsFalling", true);
            }
            else
            {
                animator.SetBool("IsRising", false);
                animator.SetBool("IsFalling", false);
            }
        }
        // Ensure both rising and falling states are false when grounded
        else
        {
            animator.SetBool("IsRising", false);
            animator.SetBool("IsFalling", false);
        }

        wasGrounded = isGrounded;
    }

    // Called when a jump is initiated
    public void OnJumpInitiated()
    {
        wasJumping = true;
    }

    // Called when a jump ends (landing or reaching peak height)
    public void OnJumpEnded()
    {
        wasJumping = false;
    }
}
