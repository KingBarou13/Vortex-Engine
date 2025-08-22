using UnityEngine;
using UnityEngine.InputSystem;

public class JumpAction : PlayerAction
{
    [SerializeField] private RisingAndFalling risingAndFalling;
    public int jumps;
    [SerializeField] float jumpForce;
    [SerializeField] float airJumpForce;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject spinBall;
    [SerializeField] private GameObject spinFX;
    [SerializeField] private RailGrindTrigger railGrindTrigger;
    public int currentJumps;

    void Start()
    {
        if (risingAndFalling == null)
        {
            risingAndFalling = GetComponent<RisingAndFalling>();
        }

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

    public void Jump()
    {
        if (currentJumps <= 0) return;

        currentJumps--;

        bool isGroundedOrGrinding = playerPhysics.groundInfo.ground || railGrindTrigger.isGrinding;
        float appliedJumpForce = isGroundedOrGrinding ? jumpForce : airJumpForce;

        // Apply force only if Rigidbody is not kinematic
        if (!playerPhysics.RB.isKinematic)
        {
            playerPhysics.RB.velocity = (playerPhysics.groundInfo.normal * appliedJumpForce) + playerPhysics.horizontalVelocity;
        }
        else
        {
            playerPhysics.RB.AddForce(playerPhysics.groundInfo.normal * appliedJumpForce, ForceMode.VelocityChange);
        }

        animator.SetBool("IsJumping", true);
        spinBall.SetActive(true);
        spinFX.SetActive(true);
        risingAndFalling.OnJumpInitiated();

        if (railGrindTrigger.isGrinding)
        {
            railGrindTrigger.ExitGrind();
        }
    }


    void OnGroundEnter()
    {
        currentJumps = jumps;
        if (animator.GetBool("IsJumping"))
        {
            animator.SetBool("IsJumping", false);
            spinBall.SetActive(false);
            spinFX.SetActive(false);
        }
        risingAndFalling.OnJumpEnded();
    }


}
