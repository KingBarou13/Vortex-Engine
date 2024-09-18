using UnityEngine;
using UnityEngine.InputSystem;

public class JumpAction : PlayerAction
{
    [SerializeField] private GrindAction grindAction;  
    [SerializeField] private RisingAndFalling risingAndFalling;

    [SerializeField] int jumps;
    [SerializeField] float jumpForce;
    [SerializeField] float airJumpForce;
    [SerializeField] private Animator animator;
    int currentJumps;

    void Start()
    {
        if (grindAction == null)
        {
            grindAction = GetComponentInParent<GrindAction>() ?? GetComponentInChildren<GrindAction>();
        }

        if (risingAndFalling == null)
        {
            risingAndFalling = GetComponent<RisingAndFalling>();
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
        risingAndFalling.OnJumpEnded();
    }

    void Jump()
    {
        // Exit rail grind if jumping while on a rail
        if (grindAction != null && grindAction.onRail)
        {
            grindAction.ExitRailOnJump();
        }

        if (currentJumps <= 0) return;

        currentJumps--;

        // Determine jump force based on whether the player is grounded or in the air
        float appliedJumpForce = playerPhysics.groundInfo.ground ? jumpForce : airJumpForce;

        // Apply vertical jump velocity, adding current horizontal velocity
        playerPhysics.RB.velocity = (playerPhysics.groundInfo.normal * appliedJumpForce) + playerPhysics.horizontalVelocity;

        // Update animator to indicate jumping
        animator.SetBool("IsJumping", true);

        // Notify Rising and Falling script that a jump has been initiated
        risingAndFalling.OnJumpInitiated();
    }
}
