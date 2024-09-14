using UnityEngine;
using UnityEngine.InputSystem;

public class JumpAction : PlayerAction
{
    //On Jump
    public void OnJump(InputAction.CallbackContext callbackContext)
    {
        if(callbackContext.performed)
            Jump();
    }

    //On Enable
    void OnEnable()
    {
        playerPhysics.onGroundEnter += OnGroundEnter;
    }

    //On Disable
    void OnDisable()
    {
        playerPhysics.onGroundEnter -= OnGroundEnter;
    }

    //On Ground Enter
    void OnGroundEnter()
    {
        currentJumps = jumps;
        animator.SetBool("IsJumping", false);  // Reset jumping state when grounded
    }

    //Jump
    [SerializeField] int jumps;
    [SerializeField] float jumpForce;
    [SerializeField] float airJumpForce;
    [SerializeField] private Animator animator;  // Animator reference for triggering animations

    int currentJumps;

    void Jump()
    {
        if (currentJumps <= 0) return;

        currentJumps--;

        float jumpForce = groundInfo.ground ? this.jumpForce : airJumpForce;

        RB.velocity = (groundInfo.normal * jumpForce)
        + playerPhysics.horizontalVelocity;

        animator.SetBool("IsJumping", true);  // Trigger jump animation
    }
}