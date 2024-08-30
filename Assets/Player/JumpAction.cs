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
    }


    //Jump

    [SerializeField] int jumps;
    [SerializeField] float jumpForce;

    [SerializeField] float airJumpForce;

    int currentJumps;

    void Jump()
    {
        if (currentJumps <= 0) return;

        currentJumps --;

        float jumpForce = groundInfo.ground ? this.jumpForce : airJumpForce;

        RB.velocity = (groundInfo.normal * jumpForce)
        + playerPhysics.horizontalVelocity;
    }
}
