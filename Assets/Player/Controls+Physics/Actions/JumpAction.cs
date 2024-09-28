using UnityEngine;
using UnityEngine.InputSystem;

public class JumpAction : PlayerAction
{
    [SerializeField] private RisingAndFalling risingAndFalling;
    [SerializeField] int jumps;
    [SerializeField] float jumpForce;
    [SerializeField] float airJumpForce;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject spinBall;
    [SerializeField] private GameObject spinFX;
    int currentJumps;

    void Start()
    {
        if (risingAndFalling == null)
        {
            risingAndFalling = GetComponent<RisingAndFalling>();
        }

        // Ensure spinBall and spinFX are hidden at the start
        spinBall.SetActive(false);
        spinFX.SetActive(false);
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

        // Hide spinBall and spinFX when grounded
        spinBall.SetActive(false);
        spinFX.SetActive(false);

        risingAndFalling.OnJumpEnded();
    }

    void Jump()
    {
        if (currentJumps <= 0) return;

        currentJumps--;

        // Determine jump force based on whether the player is grounded or in the air
        float appliedJumpForce = playerPhysics.groundInfo.ground ? jumpForce : airJumpForce;

        // Apply vertical jump velocity, adding current horizontal velocity
        playerPhysics.RB.velocity = (playerPhysics.groundInfo.normal * appliedJumpForce) + playerPhysics.horizontalVelocity;

        // Update animator to indicate jumping
        animator.SetBool("IsJumping", true);

        // Show spinBall and spinFX when jumping
        spinBall.SetActive(true);
        spinFX.SetActive(true);

        // Notify Rising and Falling script that a jump has been initiated
        risingAndFalling.OnJumpInitiated();
    }
}
