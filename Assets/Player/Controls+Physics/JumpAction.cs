using UnityEngine;
using UnityEngine.InputSystem;

public class JumpAction : PlayerAction
{
    [SerializeField] private GrindAction grindAction;  // Reference to GrindAction, assign in Inspector if available

    [SerializeField] int jumps;
    [SerializeField] float jumpForce;
    [SerializeField] float airJumpForce;
    [SerializeField] private Animator animator;
    int currentJumps;

    void Start()
    {
        if (grindAction == null)
        {
            // Try finding GrindAction in parent or child objects
            grindAction = GetComponentInParent<GrindAction>();

            // If not found in parent, try children
            if (grindAction == null)
            {
                grindAction = GetComponentInChildren<GrindAction>();
            }
        }
    }

    public void OnJump(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.performed)
        {
            Jump();
        }
    }

    void OnEnable()
    {
        playerPhysics.onGroundEnter += OnGroundEnter;
    }

    void OnDisable()
    {
        playerPhysics.onGroundEnter -= OnGroundEnter;
    }

    void OnGroundEnter()
    {
        currentJumps = jumps;
        animator.SetBool("IsJumping", false);
    }

    void Jump()
    {
        // Check if the player is on a rail before jumping
        if (grindAction != null && grindAction.onRail)
        {
            // Exit the rail grind when jumping
            grindAction.ExitRailOnJump();
        }

        // If out of jumps, return
        if (currentJumps <= 0) return;

        // Decrement jump count
        currentJumps--;

        // Determine jump force based on whether the player is grounded or in the air
        float appliedJumpForce = playerPhysics.groundInfo.ground ? jumpForce : airJumpForce;

        // Apply vertical jump velocity, adding the player's current horizontal velocity
        playerPhysics.RB.velocity = (playerPhysics.groundInfo.normal * appliedJumpForce) + playerPhysics.horizontalVelocity;

        // Update animator to indicate the player is jumping
        animator.SetBool("IsJumping", true);
    }
}
